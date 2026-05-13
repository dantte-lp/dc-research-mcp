namespace DcResearchMcp.Embedding;

/// <summary>
/// Produces dense vector embeddings for short text inputs.
/// All implementations are expected to be thread-safe and disposable
/// (the underlying ONNX <c>InferenceSession</c> holds native resources).
/// </summary>
public interface IEmbedder : IDisposable
{
    /// <summary>Output vector dimension (e.g. 384 for multilingual-e5-small).</summary>
    int Dimension { get; }

    /// <summary>
    /// Embed a batch of inputs. <paramref name="isQuery"/> selects the prefix:
    /// <c>"query: "</c> for retrieval queries, <c>"passage: "</c> for indexed
    /// documents (per the E5 family's training contract).
    /// </summary>
    Task<float[][]> EmbedAsync(IReadOnlyList<string> texts, bool isQuery, CancellationToken cancellationToken);
}
