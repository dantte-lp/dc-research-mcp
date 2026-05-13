namespace DcResearchMcp.Embedding;

/// <summary>Outcome of <see cref="IPretokenizer.Encode"/>.</summary>
/// <param name="InputIds">Row-major batched int64 input ids of size <c>BatchSize × SeqLen</c>.</param>
/// <param name="AttentionMask">Row-major batched int64 attention mask of size <c>BatchSize × SeqLen</c>.</param>
/// <param name="BatchSize">Number of input texts.</param>
/// <param name="SeqLen">Padded sequence length used for this batch.</param>
public sealed record TokenizedBatch(long[] InputIds, long[] AttentionMask, int BatchSize, int SeqLen);
