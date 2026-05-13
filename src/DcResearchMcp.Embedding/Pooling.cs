namespace DcResearchMcp.Embedding;

/// <summary>
/// Numerical helpers used by <see cref="E5OnnxEmbedder"/>. Kept in their own type so
/// they can be exercised in isolation without an ONNX model file present.
/// </summary>
public static class Pooling
{
    /// <summary>
    /// Mean-pool a batched last-hidden-state tensor with an attention mask.
    /// </summary>
    /// <param name="hidden">Row-major float buffer of shape <c>batch × seq × hidden</c>.</param>
    /// <param name="attentionMask">Row-major int64 mask of shape <c>batch × seq</c>; <c>1</c> = real token, <c>0</c> = padding.</param>
    /// <param name="batchSize">Number of rows in <paramref name="hidden"/>.</param>
    /// <param name="seqLen">Per-row sequence length.</param>
    /// <param name="hiddenSize">Per-token hidden size (e.g. 384).</param>
    /// <returns>One pooled vector per batch row (<c>batchSize</c> × <c>hiddenSize</c>).</returns>
    public static float[][] MeanPool(
        ReadOnlySpan<float> hidden,
        ReadOnlySpan<long> attentionMask,
        int batchSize,
        int seqLen,
        int hiddenSize)
    {
        if (hidden.Length != batchSize * seqLen * hiddenSize)
        {
            throw new ArgumentException(
                $"hidden buffer length {hidden.Length} != batch*seq*hidden {batchSize * seqLen * hiddenSize}",
                nameof(hidden));
        }
        if (attentionMask.Length != batchSize * seqLen)
        {
            throw new ArgumentException(
                $"attentionMask length {attentionMask.Length} != batch*seq {batchSize * seqLen}",
                nameof(attentionMask));
        }

        var output = new float[batchSize][];
        for (var b = 0; b < batchSize; b++)
        {
            var pooled = new float[hiddenSize];
            long countTokens = 0;
            for (var s = 0; s < seqLen; s++)
            {
                var maskValue = attentionMask[(b * seqLen) + s];
                if (maskValue == 0)
                {
                    continue;
                }
                countTokens++;
                var hOffset = ((b * seqLen) + s) * hiddenSize;
                for (var h = 0; h < hiddenSize; h++)
                {
                    pooled[h] += hidden[hOffset + h];
                }
            }
            var denom = countTokens > 0 ? (float)countTokens : 1f;
            for (var h = 0; h < hiddenSize; h++)
            {
                pooled[h] /= denom;
            }
            output[b] = pooled;
        }
        return output;
    }

    /// <summary>
    /// L2-normalise a vector in place: divide each component by the Euclidean norm.
    /// Zero-norm vectors are left untouched (NaN-safe).
    /// </summary>
    public static void L2NormalizeInPlace(Span<float> vector)
    {
        double sumSquares = 0;
        for (var i = 0; i < vector.Length; i++)
        {
            sumSquares += (double)vector[i] * vector[i];
        }
        if (sumSquares <= 0)
        {
            return;
        }
        var inverseNorm = (float)(1.0 / Math.Sqrt(sumSquares));
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] *= inverseNorm;
        }
    }
}
