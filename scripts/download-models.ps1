# Download ONNX assets for dc-research-mcp embedder.
#
# Defaults: intfloat/multilingual-e5-small (118M params, dim 384, multilingual incl. Russian).
# Targets:  models/multilingual-e5-small/{model.onnx, tokenizer.json, ...}
#
# Why a script: avoids committing the ~120 MB binary blob to git, and lets a
# laptop bootstrap fetch just what it needs over `hf` CLI (faster, resumable,
# checksummed via etag) without pulling the full HF repo.
#
# Usage:
#   pwsh scripts/download-models.ps1
#   pwsh scripts/download-models.ps1 -Model "BAAI/bge-m3" -OutDir "models/bge-m3"
#
# Requirements: Python 3.11+, `pip install -U huggingface_hub` (provides the `hf` CLI).

[CmdletBinding()]
param(
    [string] $Model    = "intfloat/multilingual-e5-small",
    [string] $OutDir   = "models/multilingual-e5-small",
    [string] $Revision = "main"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command hf -ErrorAction SilentlyContinue)) {
    Write-Host "hf CLI not found. Install with:  pip install -U huggingface_hub" -ForegroundColor Yellow
    throw "huggingface_hub CLI required"
}

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$target = Join-Path $root $OutDir
New-Item -ItemType Directory -Force -Path $target | Out-Null

Write-Host "Downloading $Model @$Revision  → $target" -ForegroundColor Cyan

# Pull only what the embedder actually needs:
#   - model.onnx (or onnx/model.onnx in HF's standard layout)
#   - tokenizer.json + sentencepiece.bpe.model
#   - config.json (for dim / max_seq sanity-check)
#
# `hf download` is the modern entry point replacing huggingface-cli; it supports
# --include glob filtering and writes directly to the local-dir.
hf download $Model `
    --revision $Revision `
    --local-dir $target `
    --include "*.onnx" "*.onnx_data" "tokenizer.json" "tokenizer_config.json" `
              "sentencepiece.bpe.model" "config.json" "special_tokens_map.json"

Write-Host ""
Write-Host "Done. Contents:" -ForegroundColor Green
Get-ChildItem -Path $target -Recurse -File |
    Sort-Object FullName |
    ForEach-Object { "{0,12:N0}  {1}" -f $_.Length, $_.FullName.Substring($target.Length + 1) } |
    Write-Host
