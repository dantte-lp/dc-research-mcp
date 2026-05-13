namespace DcResearchMcp.Embedding;

/// <summary>
/// Tokenizer surface decoupled from the concrete model format
/// (SentencePiece / WordPiece / BPE). Returns int64 input_ids and an
/// attention_mask aligned to the maximum sequence length in the batch.
/// </summary>
public interface IPretokenizer
{
    /// <summary>Encode a batch of strings into padded int64 arrays + attention mask.</summary>
    /// <param name="texts">Inputs (already prefixed if applicable).</param>
    /// <param name="maxSequenceLength">Hard cap on per-text sequence length.</param>
    /// <returns>
    /// <c>InputIds</c> and <c>AttentionMask</c> are length <c>texts.Count * SeqLen</c>;
    /// <c>SeqLen</c> is the actual padded length used for this batch.
    /// </returns>
    TokenizedBatch Encode(IReadOnlyList<string> texts, int maxSequenceLength);
}

