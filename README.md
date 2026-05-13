# dc-research-mcp

Hybrid-retrieval MCP server for the **DC Greenfield Tashkent** research corpus.
**.NET 10 + PostgreSQL 18 + pgvector + vchord_bm25 + ONNX Runtime (CPU)**.

Architectural analogue of [`dantte-lp/arista-mcp`](https://github.com/dantte-lp/arista-mcp) and [`dantte-lp/nutanix-mcp`](https://github.com/dantte-lp/nutanix-mcp). For a lightweight pure-Python BM25-only alternative (laptop bootstrap, no Docker) see [`dantte-lp/dc-research-mcp-py`](https://github.com/dantte-lp/dc-research-mcp-py).

## What's indexed

The corpus lives in a separate repository `dc_greenfield_research`. This MCP indexes:

| Source | Origin |
|---|---|
| `30-findings/**/*.md` (research findings + follow-up reports) | `research` |
| `60-tech-spec/**/*.md` (Техническое задание / ТЗ) | `tech-spec` |
| `00-overview/**/*.md` (project overview) | `overview` |
| `assets/02-standards/converted/**/*.md` (TIA-942-C, ASHRAE, NFPA, BICSI, EN 50600, ShNK/KMK …) | `standard` |
| `assets/01-from-uzum-tz/converted/**/*.md` (vendor / former-vendor sources) | `vendor` |

Each chunk gets metadata: `source_file`, `origin`, `category` (`A-strategy`…`J-ai-specific`, `tech-spec`, `intl-standards`, `uz-standards`), `h1/h2/h3`, `chunk_index`, `llm_source` (`claude`/`openai`), `prompt_id` (`A1`, `C-FU-2`, …).

## Architecture

```
30-findings + 60-tech-spec + 00-overview + assets/02-standards
        │
        ▼  CLI: ingest → chunker → ONNX embedder (CPU)
   ┌────────────────────────────────────────┐
   │ PostgreSQL 18 (podman / docker)         │
   │   vector + vchord + vchord_bm25         │
   │   pg_tokenizer (russian, english, mixed)│
   │ ┌──────────────┐  ┌───────────────────┐ │
   │ │ chunks(text) │  │ embeddings(vector)│ │
   │ │ + bm25       │  │ + hnsw            │ │
   │ └──────────────┘  └───────────────────┘ │
   └────────────────────────────────────────┘
        │
        ▼  hybrid search: vchord_bm25 ⊕ pgvector HNSW → RRF
   MCP Server (stdio) — ModelContextProtocol 1.2
        │
        ▼
   Claude Code / any MCP client
```

## Quick start

Prerequisites: **.NET 10 SDK** (`10.0.201` or newer), **Podman 5+** (or Docker), and access to the `dc_greenfield_research` corpus on disk.

```bash
# 1. start Postgres with vchord + vchord_bm25 + pg_tokenizer
podman compose -f docker/compose.yaml up -d
# (Postgres listens on host port 5435; arista-mcp uses 5434 to avoid collision)

# 2. restore + build
dotnet restore
dotnet build -c Release

# 3. run migrations (sprint 1)
# dotnet run --project src/DcResearchMcp.Cli -- migrate

# 4. ingest the corpus (sprint 3)
# DC_RESEARCH_SOURCE_DIR=C:/SHARE/dc_greenfield_research \
#   dotnet run --project src/DcResearchMcp.Cli -- ingest

# 5. start MCP server (sprint 4) — stdio
# dotnet run --project src/DcResearchMcp.Server
```

## Connecting to Claude Code

`~/.claude/mcp.json` or per-project `.mcp.json`:

```jsonc
{
  "mcpServers": {
    "dc-research": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/SHARE/dc-research-mcp/src/DcResearchMcp.Server",
        "-c", "Release",
        "--no-build"
      ],
      "env": {
        "DC_RESEARCH_SOURCE_DIR": "C:/SHARE/dc_greenfield_research",
        "DC_RESEARCH_PG_CONN": "Host=localhost;Port=5435;Database=dc_research;Username=dc_research;Password=dc_research"
      }
    }
  }
}
```

## Variants

| Use case | Project |
|---|---|
| Laptop bootstrap, no Docker, no DB, ~50 MB RAM | [`dc-research-mcp-py`](https://github.com/dantte-lp/dc-research-mcp-py) (BM25-only, bm25s) |
| Workstation, Docker available, hybrid retrieval (this repo) | **dc-research-mcp** (.NET 10 + Postgres + ONNX CPU) |
| Pattern for other corpora | [`nutanix-mcp`](https://github.com/dantte-lp/nutanix-mcp), [`arista-mcp`](https://github.com/dantte-lp/arista-mcp) |

## Status

**Sprint 0 (scaffolding)** — initial release `0.1.0`. Solution + 5 projects + 5 test projects + Postgres compose with `vchord_bm25` + analyzers (russian, english, mixed). `dotnet build` passes on empty stubs; `podman compose up` brings up Postgres with required extensions. Functional code lands in subsequent sprints — see `CHANGELOG.md`.

Roadmap (planned):

- Sprint 1 — `Core` + `Data` (models, EF Core, migrations, COPY loader)
- Sprint 2 — `Embedding` (ONNX wrapper + multilingual-e5-small)
- Sprint 3 — `Cli` (ingest / reindex / search / info)
- Sprint 4 — `Server` (MCP tools: search, get_chunk, list_sources, list_files, index_info)
- Sprint 5 — Testcontainers + xunit + GitHub Actions CI
- Sprint 6 — `0.2.0` release

## License

MIT. See [LICENSE](LICENSE).
