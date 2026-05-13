# Changelog

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
