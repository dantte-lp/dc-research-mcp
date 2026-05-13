# Contributing

## Quick start for contributors

```bash
git clone https://github.com/dantte-lp/dc-research-mcp
cd dc-research-mcp

# enable the format-on-commit hook (one-time per clone)
git config core.hooksPath .githooks

# build + tests
dotnet restore
dotnet build -c Release
dotnet test  -c Release --no-build
```

## Commit messages — Conventional Commits 1.0.0

Format: `<type>[optional scope]: <description>`. See <https://www.conventionalcommits.org/en/v1.0.0/>.

| type | when |
|--|--|
| `feat` | new user-visible capability (CLI verb, MCP tool, ingest stage) |
| `fix` | bug fix (regression, wrong DDL, broken serialisation) |
| `perf` | runtime / index / query speedup with no behaviour change |
| `refactor` | internal rework, no behaviour change |
| `test` | add or refine tests, no production code change |
| `docs` | README / docs/ / CHANGELOG / plan |
| `build` | csproj, Directory.Packages.props, global.json, Containerfile |
| `ci` | GitHub Actions, dependabot, semgrep, pvs-studio jobs |
| `chore` | uninteresting maintenance (gitignore, formatter pass) |

Breaking changes use `!` after type/scope **and** a `BREAKING CHANGE:` footer:

```text
feat(data)!: switch chunk PK to bigint snowflake

BREAKING CHANGE: existing migrations cannot upgrade in-place;
drop and re-ingest required.
```

Optional scopes matching the layered architecture: `core`, `data`, `embedding`,
`server`, `cli`, `docker`, `tests`. Cross-cutting changes can omit the scope.

## Quality gates

Every PR is gated by `.github/workflows/ci.yml`:

| Gate | Tool | Local equivalent |
|---|---|---|
| Build (Release) | `dotnet build -c Release` | same |
| Unit tests | `dotnet test` | same |
| Integration tests (Postgres) | service container (PG 18 + vchord_bm25) | `podman compose up -d` then run `Data.Tests` |
| Format | `dotnet format --verify-no-changes` | enforced by `.githooks/pre-commit` |
| Docs lint | `markdownlint-cli2 '**/*.md'` | `npx markdownlint-cli2 '**/*.md'` |
| Semgrep | `semgrep scan --config=auto --error` | `semgrep scan --config=auto --error src/` |
| PVS-Studio | `pvs-studio-dotnet -t` per project | requires PVS license (set `PVS_USER`/`PVS_KEY` env or skip locally) |
| CodeQL | GitHub Code Scanning | run via Actions only |

### PVS-Studio locally (optional)

PVS-Studio runs in CI when the `PVS_USER` / `PVS_KEY` secrets are configured.
For local runs:

```bash
# install (Debian/Ubuntu)
wget -q -O- https://files.pvs-studio.com/etc/pubkey.txt | sudo apt-key add -
sudo wget -q -O /etc/apt/sources.list.d/viva64.list https://files.pvs-studio.com/etc/viva64.list
sudo apt-get update && sudo apt-get install -y pvs-studio-dotnet

# license (one of: free individual / OSS / commercial)
pvs-studio-dotnet credentials -u "<email>" -n "<key>"

# scan
mkdir -p pvs-out
for proj in src/DcResearchMcp.*/DcResearchMcp.*.csproj; do
  name=$(basename "$proj" .csproj)
  pvs-studio-dotnet -t "$proj" -c Release -o "pvs-out/${name}.plog" || true
done
```

The `.plog` reports are XML; open in PVS-Studio Java/Standalone viewer, or convert with `plog-converter` to HTML/SARIF.

## Tests layering

| Project | Layer | External resources |
|---|---|---|
| `DcResearchMcp.Core.Tests` | pure unit | none |
| `DcResearchMcp.Embedding.Tests` | unit + (optional) ONNX model | `models/*.onnx` (skipped via `SkippableFact` if missing) |
| `DcResearchMcp.Data.Tests` | integration | PostgreSQL 18 + extensions (via Testcontainers or service container) |
| `DcResearchMcp.Server.Tests` | unit | none |
| `DcResearchMcp.E2E` | end-to-end | Postgres + models, full MCP stdio exchange |

CI splits these: unit tests run on every push; integration tests run on the `e2e` matrix; the gate sequence is build → unit → integration → format → docs → semgrep → PVS.

## Code style

- C# 13 / .NET 10, `Nullable` enabled, `TreatWarningsAsErrors=true`.
- Analyzer pack: Meziantou, Roslynator, SonarAnalyzer, AsyncFixer, BannedApiAnalyzers — all gated; suppress only with a documented `# pragma` comment.
- Test naming: `Given_When_Then` underscores allowed (relaxed by `tests/Directory.Build.props`).
- No `DateTime.Now` / `DateTime.UtcNow` — use `TimeProvider` and `DateTimeOffset.UtcNow` (see `BannedSymbols.txt`).
- No `MD5`/`SHA1` for new code — use SHA-256+.

## Reporting issues

- Bugs: <https://github.com/dantte-lp/dc-research-mcp/issues>
- Security issues: see [SECURITY.md](SECURITY.md).
