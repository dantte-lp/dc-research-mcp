# Changelog

## [Unreleased] — Sprint 2 (Embedding — ONNX CPU)

### Added — Embedding

- `IEmbedder` — public surface: `int Dimension`, `Task<float[][]> EmbedAsync(IReadOnlyList<string>, bool isQuery, CancellationToken)`; `IDisposable`.
- `IPretokenizer` + `TokenizedBatch` — tokenizer surface decoupled from model format (SentencePiece / WordPiece / BPE); returns row-major int64 `InputIds` + `AttentionMask` with batch + seq dims.
- `EmbeddingOptions` — model/tokenizer paths, dimension (384), max sequence (512), batch size (16 CPU-friendly default), `QueryPrefix`/`PassagePrefix` for the E5 contract (`"query: "` / `"passage: "`), optional GPU EP.
- `Pooling` — `MeanPool(hidden, attentionMask, batch, seq, hidden)` with masked averaging, `L2NormalizeInPlace(span)` with zero-norm guard. Pure math, fully testable without an ONNX file.
- `E5OnnxEmbedder` — XLM-RoBERTa-based pipeline (`input_ids` + `attention_mask` int64 → ONNX run → `last_hidden_state` → mean pool → L2 normalize); per-batch `Task.Run` for async, `IDisposable` releases the ORT session.
- `scripts/download-models.ps1` — `hf download intfloat/multilingual-e5-small --include "*.onnx" "tokenizer.json" "sentencepiece.bpe.model" "config.json" ...` into `models/multilingual-e5-small/`; resumable + checksummed; replaces the deprecated `huggingface-cli`.

### Tests

- 8 `PoolingTests` (full-mask average, padding-ignored, multi-batch independence, all-zero-mask guard, L2 unit-vector, L2 zero-vector, argument validation for both shapes).

### Verified

- `dotnet build -c Release` — 0 warnings / 0 errors.
- `dotnet test` — **17/17 passing** (6 Core + 8 Embedding + 1 Data + 1 Server + 1 E2E).
- `dotnet format --verify-no-changes` — clean.

### Known follow-ups (Sprint 2 add-on)

- `IPretokenizer` concrete: SentencePiece reader for XLM-R / multilingual-e5 (`sentencepiece.bpe.model` + `tokenizer.json`). Slot the implementation behind the existing interface — `E5OnnxEmbedder` pipeline is ready to receive any conforming tokenizer.
- Live `SkippableFact` for `E5OnnxEmbedder` end-to-end against a real downloaded model: skip when `models/multilingual-e5-small/model.onnx` is absent, run when present (uses the script above to bootstrap).

## [Unreleased] — Sprint 1 add-on (EF migrations + chunker)

### Added — Data

- `src/DcResearchMcp.Data/DataSourceFactory.cs` — `NpgsqlDataSource` wired up for Pgvector (`UseVector()`).
- `src/DcResearchMcp.Data/DesignTimeDbContextFactory.cs` — consumed by `dotnet ef migrations add / database update`; reads `DC_RESEARCH_PG_CONN`, falls back to local dev creds (`localhost:5435/dc_research/dc_research/dc_research`).
- `Embedding` (`halfvec(384)`) column on `ChunkEntity` for `intfloat/multilingual-e5-small`; HNSW index `halfvec_cosine_ops` with `m=16`, `ef_construction=200`.
- Server-side `CreatedAt DEFAULT now()`.
- EF Core migrations:
  - `20260513155017_Initial` — creates `chunks` table, all metadata indexes, the halfvec HNSW index, and the five Postgres extension annotations (`vector`, `vchord`, `vchord_bm25`, `pg_tokenizer`, `pg_trgm`).
  - `20260513155207_AddBm25Column` — raw-SQL: adds `bm25v bm25vector` column, calls `tokenizer_catalog.create_custom_model_tokenizer_and_trigger` against `mixed_analyzer` (russian-stemmed Cyrillic + Latin acronyms passthrough — defined in `docker/init.sql`), creates `idx_chunks_bm25 USING bm25 (bm25v bm25_ops)`. Reversible `Down()` drops the trigger, tokenizer model, BM25 index, and the column.
- `src/DcResearchMcp.Data/Migrations/.editorconfig` — suppresses CA1707/CA1825/CA1861/IDE0161/MA0005/MA0048/MA0051 on auto-generated EF code; marks the directory `generated_code = true`.

### Added — Core (Chunking)

- `Section` record (`H1`/`H2`/`H3`/`Body`).
- `ChunkDraft` record (pre-DB chunk with text, index, headings, size).
- `IChunker` interface.
- `MarkdownChunker` — header-aware split by H1/H2/H3 (honours fenced code blocks), then recursive split by `\n\n` → `\n` → `. ` → `? ` → `! ` → `; ` → ` ` to fit `TargetChars`, with `OverlapChars` carry-over and `MinChunkChars` floor. Uses `[GeneratedRegex]` source generation.

### Tests

- `MarkdownChunkerTests` (6 tests): single short section → one chunk with headings; new H2 resets H3, keeps H1; `#` inside fenced code is not a heading; oversized section splits with overlap (verified char-budget + overlap presence); chunks below min drop.
- `InternalsVisibleTo("DcResearchMcp.Core.Tests")` on the Core project so chunker internals (`SplitByHeaders`, `RecursiveSplit`) are testable directly.

### Verified

- `dotnet build -c Release` — 0 warnings, 0 errors across 12 projects (10 + 2 generated migration assemblies).
- `dotnet test` — 10/10 passing (6 chunker + 4 scaffold).
- `dotnet format --verify-no-changes` — clean.

## [Unreleased] — Sprint 1 starter (Core domain + Data context)

### Added

- `src/DcResearchMcp.Core/Catalog/Origin.cs` — `Origin` enum (Research / TechSpec / Overview / Standard / Vendor), mirrors the indexer's source roots.
- `src/DcResearchMcp.Core/Catalog/KnownCategories.cs` — canonical discipline strings (A-strategy … J-ai-specific, _aggregated, PRES-presentation-design, tech-spec, overview, intl-standards, uz-standards, vendor, general), `All` set for validation, `IsKnown(string)`.
- `src/DcResearchMcp.Core/Catalog/DocumentMetadata.cs` — sealed record with `SourceFile` / `Origin` / `Category` / `LlmSource` / `PromptId` / `SizeBytes` / `LastModified`.
- `src/DcResearchMcp.Core/Catalog/Chunk.cs` — sealed record (Id, Text, Document, ChunkIndex, H1/H2/H3); `Id` is SHA-256-derived and content-stable across re-runs.
- `src/DcResearchMcp.Core/Chunking/ChunkingOptions.cs` — record with `TargetChars` (2000) / `OverlapChars` (320) / `MinChunkChars` (200).
- `src/DcResearchMcp.Data/Catalog/ChunkEntity.cs` — EF Core POCO mirroring `Core.Catalog.Chunk` plus `CreatedAt` / `SizeChars` / `ContentId` (denormalised facets for fast filtering).
- `src/DcResearchMcp.Data/DcResearchDbContext.cs` — EF Core 9 DbContext with primary constructor; declares Postgres extensions (`vector`, `vchord`, `vchord_bm25`, `pg_tokenizer`, `pg_trgm`); maps `chunks` table with unique index on `ContentId`, secondary indexes on `SourceFile` / `Category` / `Origin` / `PromptId`; column lengths capped (HasMaxLength).

Embedding column `embedding vector(384)` and `vchord_bm25` bm25v column will be attached via raw-SQL migration in the Sprint 1 add-on (EF migrations cannot infer Pgvector + vchord_bm25 mappings from attributes alone).

### Verified

- `dotnet build -c Release` — 0 warnings, 0 errors across 10 projects after fix.
- `dotnet format --verify-no-changes` — clean (rewrote `dotnet new console` `Program.cs` stubs as LF + UTF-8 no-BOM after analyzer flagged ENDOFLINE/CHARSET).
- `dotnet test` — 5/5 passing.

## [Unreleased] — Sprint 0.5 (quality stack)

### Added

- `.markdownlint.jsonc` + `.markdownlintignore` — strict markdownlint-cli2 ruleset (line length 120 for prose, code blocks and tables exempt; MD024/MD034/MD041 relaxed for CHANGELOG/index ergonomics).
- `.githooks/pre-commit` — runs `dotnet format --verify-no-changes` against staged `*.cs` files; opt-out via `SKIP_FORMAT=1`. Enable per clone: `git config core.hooksPath .githooks`.
- `CONTRIBUTING.md` — Conventional Commits 1.0.0 scope table (`core`/`data`/`embedding`/`server`/`cli`/`docker`/`tests`), quality-gate map, PVS-Studio local-run recipe, test layering.
- `SECURITY.md` — supported versions table, GHSA reporting channel, severity → acknowledge/fix/disclosure SLAs, in-scope vs upstream surface, known-accepted risks (default `dc_research/dc_research/dc_research` dev creds — production MUST override; commercial standards text in corpus is a licensing matter, not a vulnerability).
- `.github/dependabot.yml` — weekly NuGet + GitHub Actions, minor/patch bundled, major as separate PRs.
- `.github/workflows/ci.yml` — six-job gate:
  1. `build + unit tests` — `dotnet build -c Release` + unit-only test filter (excludes `Data.Tests` and `E2E`); uploads `test-results` and `analyzers.sarif`.
  2. `integration tests (Postgres)` — service-container `tensorchord/vchord-suite:pg18-latest`, applies `docker/init.sql`, runs `Data.Tests` against `dc_research_test`.
  3. `dotnet format` — `--verify-no-changes`.
  4. `docs lint` — `markdownlint-cli2 '**/*.md'`.
  5. `semgrep` — `--config=auto --config=p/security-audit --config=p/secrets --error`, scoped to `src/` + `docker/`, file types `*.cs`/`*.yaml`/`*.yml`/`*.sql`.
  6. `pvs-studio (C#)` — installs `pvs-studio-dotnet`, scans each `src/DcResearchMcp.*` csproj, converts `.plog` → SARIF; gated on `PVS_USER`/`PVS_KEY` secrets (auto-skip when absent or on cross-fork PRs); tolerates exit bits 256 (findings present) and 1024 (license expiring).
- `.github/workflows/security.yml` — CodeQL (csharp, `security-and-quality` queries; public-repo gated) + `actions/dependency-review-action` on PRs (`fail-on-severity: high`). Weekly cron Monday 06:00 UTC.

### Verified

- `dotnet build -c Release` — 0 warnings, 0 errors across 10 projects.
- `dotnet test -c Release --no-build` — 5/5 passing (Core/Embedding/Data/Server/E2E scaffold tests).
- `dotnet format --verify-no-changes` — exit 0 (style suggestions remain `info`-level only).

## [0.1.0] — 2026-05-13 — Sprint 0 (scaffolding)

Initial scaffold for the .NET 10 + PostgreSQL + pgvector + vchord_bm25 + ONNX (CPU) hybrid-retrieval MCP server. No functional code yet — sets the build, packaging, and runtime container shape for the upcoming sprints.

### Added

- Solution `dc-research-mcp.slnx` (modern XML format) with 5 `src/` projects + 5 `tests/` projects:
  - `DcResearchMcp.Core` (class library) — domain models, abstractions (Sprint 1).
  - `DcResearchMcp.Embedding` (class library) — ONNX-based embedder (Sprint 2).
  - `DcResearchMcp.Data` (class library) — EF Core 9 + Pgvector + COPY loader (Sprint 1).
  - `DcResearchMcp.Server` (console app) — MCP stdio host (Sprint 4).
  - `DcResearchMcp.Cli` (console app) — ingest / reindex / search / info commands (Sprint 3).
  - Five corresponding `*.Tests` / `E2E` projects (xUnit).
- `Directory.Build.props` — net10.0, version 0.1.0, central package management, analyzer pack: Meziantou, Roslynator, SonarAnalyzer, AsyncFixer, BannedApiAnalyzers; deterministic CI builds; SARIF error log; `TreatWarningsAsErrors=true`.
- `Directory.Packages.props` — centrally pinned versions: ModelContextProtocol 1.2.0, EF Core 9.0.15, Npgsql.EFCore 9.0.4, Pgvector 0.3.2, Pgvector.EFCore 0.3.0, Microsoft.ML.OnnxRuntime 1.24.4, System.CommandLine 2.0.6, Spectre.Console 0.55.2, Testcontainers.PostgreSql 4.11.0, xunit 2.9.3, FluentAssertions 8.9.0, NSubstitute 5.3.0, OpenTelemetry 1.15.3, plus CVE-patched System.Security.Cryptography.Xml 10.0.6.
- `BannedSymbols.txt` — bans `DateTime`/`DateTime.Now` (use `DateTimeOffset.UtcNow` / `TimeProvider`), `WebClient`, `BinaryFormatter`, `MD5`, `SHA1`, `Thread.Abort`.
- `docker/compose.yaml` — Podman/Docker stack: PostgreSQL 18 (image `dc-research-mcp-postgres:18` built from `Containerfile`), preloads `vector,vchord,vchord_bm25,pg_tokenizer`, host port `0.0.0.0:5435:5432` (WSL2/Podman interop), volume `dc-research-pgdata`, healthcheck `pg_isready`.
- `docker/Containerfile` — base `tensorchord/vchord-suite:pg18-latest` with `init.sql` mounted.
- `docker/init.sql` — enables `vector`, `vchord`, `vchord_bm25`, `pg_tokenizer`, `pg_trgm`; sets HNSW iterative scan + ef_search + maintenance_work_mem on the DB; creates three tokenizer analyzers: `english_analyzer` (porter2 + nltk stopwords), `russian_analyzer` (russian_stemmer + nltk_russian stopwords), `mixed_analyzer` (russian_stemmer, Latin acronyms pass through lower-cased).
- `LICENSE` (MIT), `.gitignore` (build outputs, `models/**`, `.claude/settings.local.json`, …), `.editorconfig`, `.gitattributes`, `global.json` (.NET SDK pin `10.0.201`).
- `models/README.md` — documents the default ONNX embedding model (`intfloat/multilingual-e5-small`, 118 M, dim 384, CPU INT8) and alternatives (BGE-M3 for GPU servers).

### Notes

This is the architectural twin of `arista-mcp` for the DC Greenfield Tashkent corpus. The lightweight pure-Python BM25-only sibling lives at [`dc-research-mcp-py`](https://github.com/dantte-lp/dc-research-mcp-py) for laptop bootstrap.
