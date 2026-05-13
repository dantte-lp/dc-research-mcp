# Security policy

## Supported versions

Security fixes are backported only to the most recent minor release.

| Version  | Supported         |
|----------|-------------------|
| 0.1.x    | Yes (current)     |
| < 0.1    | No                |

## Reporting a vulnerability

**Do not open a public issue** for security-sensitive reports.

Preferred channel: GitHub private security advisory —
<https://github.com/dantte-lp/dc-research-mcp/security/advisories/new>.

Include:

- Affected version(s) and commit hash if known.
- Reproducer (minimal — a query payload, a config snippet).
- Expected impact: data exposure, auth bypass, DoS, RCE.
- Any PoC code / logs that help triage.

## Response targets

| Severity   | Acknowledge within | Fix target           | Public disclosure |
|------------|--------------------|----------------------|-------------------|
| Critical   | 24 h               | 7 days               | GHSA on day of fix |
| High       | 72 h               | 14 days              | GHSA within 7 days |
| Medium     | 7 days             | 30 days              | next minor release |
| Low        | 14 days            | next release         | release notes      |

### In scope

- The .NET server code under `src/` (retrieval, embedder, MCP transports, CLI).
- Docker Compose manifest under `docker/` (PostgreSQL + extensions).
- Scripts under `scripts/`.

### Out of scope (report upstream)

- **ONNX models** — `intfloat/multilingual-e5-small`, `BAAI/bge-*`, etc.
  Report to the respective upstream projects.
- **PostgreSQL extensions** — `pgvector`, `vchord`, `vchord_bm25`,
  `pg_tokenizer`. Report to the respective upstream projects.
- **.NET SDK / ONNX Runtime / EF Core / Npgsql / Pgvector.EFCore /
  Microsoft.ML.Tokenizers / ModelContextProtocol** — report to
  the vendor; we track the bump via Dependabot.

### Known-accepted risks (NOT vulnerabilities)

- Default connection string in `docker/compose.yaml` carries a hard-coded
  local-dev password (`dc_research / dc_research`). Production deployments
  MUST override via env (`DC_RESEARCH_PG_CONN` or
  `DCRESEARCH_MCP__ConnectionString`).
- The MCP stdio transport ships without authentication; local-only is the
  intended deployment. Put it behind a reverse proxy with auth if exposed.
- The research corpus contains commercially-licensed standards text
  (TIA-942-C, ASHRAE, NFPA, BICSI, Uptime, ISO …). Distribution of the
  ingested data outside the licensee scope is a licensing matter, not a
  vulnerability of this code.

## Disclosure philosophy

We prefer coordinated disclosure. If a public disclosure is necessary
(e.g. to warn operators of active exploitation), we will publish a
GHSA advisory simultaneously with the fix.

No bug-bounty program yet — acknowledgement in the advisory only.
