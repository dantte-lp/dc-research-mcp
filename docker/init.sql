-- dc-research-mcp initial extensions + analyzers.
-- Runs once on the first container start, after POSTGRES_DB is created.
-- DB-agnostic: ALTER DATABASE targets current_database() dynamically so the
-- same script works for the production `dc_research` DB and for the
-- `dc_research_test` DB used by Testcontainers.

CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS vchord CASCADE;
CREATE EXTENSION IF NOT EXISTS pg_tokenizer;
CREATE EXTENSION IF NOT EXISTS vchord_bm25 CASCADE;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

DO $$
DECLARE
    db text := current_database();
BEGIN
    EXECUTE format(
        'ALTER DATABASE %I SET search_path TO "$user", public, tokenizer_catalog, bm25_catalog',
        db
    );
    EXECUTE format('ALTER DATABASE %I SET hnsw.iterative_scan = %L', db, 'relaxed_order');
    EXECUTE format('ALTER DATABASE %I SET hnsw.max_scan_tuples = 20000', db);
    EXECUTE format('ALTER DATABASE %I SET hnsw.ef_search = 100', db);
    EXECUTE format('ALTER DATABASE %I SET maintenance_work_mem = %L', db, '4GB');
    EXECUTE format('ALTER DATABASE %I SET jit = off', db);
END $$;

-- English analyzer (latin tokens / standards titles).
SELECT tokenizer_catalog.create_text_analyzer('english_analyzer', $$
    pre_tokenizer = "unicode_segmentation"
    [[character_filters]]
    to_lowercase = {}
    [[character_filters]]
    unicode_normalization = "nfkd"
    [[token_filters]]
    skip_non_alphanumeric = {}
    [[token_filters]]
    stopwords = "nltk_english"
    [[token_filters]]
    stemmer = "english_porter2"
$$);

-- Russian analyzer (main corpus language: research findings + ТЗ + RU standards).
SELECT tokenizer_catalog.create_text_analyzer('russian_analyzer', $$
    pre_tokenizer = "unicode_segmentation"
    [[character_filters]]
    to_lowercase = {}
    [[character_filters]]
    unicode_normalization = "nfkd"
    [[token_filters]]
    skip_non_alphanumeric = {}
    [[token_filters]]
    stopwords = "nltk_russian"
    [[token_filters]]
    stemmer = "russian_stemmer"
$$);

-- Mixed analyzer: russian-first stemmer, kept for queries that combine
-- Cyrillic narrative with Latin acronyms (Tier III, PUE, GB200 NVL72, etc.).
-- Cyrillic words pass through russian_stemmer; Latin tokens are lowercased
-- but not stemmed here — keep technical acronyms as-is.
SELECT tokenizer_catalog.create_text_analyzer('mixed_analyzer', $$
    pre_tokenizer = "unicode_segmentation"
    [[character_filters]]
    to_lowercase = {}
    [[character_filters]]
    unicode_normalization = "nfkd"
    [[token_filters]]
    skip_non_alphanumeric = {}
    [[token_filters]]
    stemmer = "russian_stemmer"
$$);
