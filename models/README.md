# models/

Локальные ONNX-модели для эмбеддингов. **Не коммитятся в git** (`.gitignore` исключает `models/**`, кроме этого README и `.gitkeep`).

## По умолчанию (CPU, sprint 2)

| Модель | Размер | Размерность | Multilingual | Файл |
|---|---|---|---|---|
| `intfloat/multilingual-e5-small` | ~118 M | 384 | да (≥ 100 языков, в т. ч. русский) | `multilingual-e5-small.onnx` |

Скачивание: `pwsh scripts/download-models.ps1` (будет добавлен в Sprint 2). Источник — Hugging Face Hub, экспорт через `optimum-cli export onnx`.

## Альтернативы

- `BAAI/bge-m3` (~570 M, dim 1024) — выше качество, но тяжелее; для CPU INT8 даёт ~2× медленнее. Для серверов с GPU — предпочтительный выбор.
- `BAAI/bge-large-en-v1.5` — только английский, не подходит для корпуса с русскоязычными ШНК/КМК и research-отчётами на русском.

Выбор фиксируется в `appsettings.json` / переменной `DC_RESEARCH_EMBEDDING_MODEL`.
