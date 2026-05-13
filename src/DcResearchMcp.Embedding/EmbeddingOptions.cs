namespace DcResearchMcp.Embedding;

/// <summary>Runtime configuration for <see cref="E5OnnxEmbedder"/>.</summary>
public sealed record EmbeddingOptions
{
    /// <summary>Path to the exported ONNX model (e.g. <c>models/multilingual-e5-small.onnx</c>).</summary>
    public required string ModelPath { get; init; }

    /// <summary>Path to the tokenizer asset (HF <c>tokenizer.json</c> or SP <c>sentencepiece.bpe.model</c>).</summary>
    public required string TokenizerPath { get; init; }

    /// <summary>Output vector dimension. 384 for multilingual-e5-small.</summary>
    public int Dimension { get; init; } = 384;

    /// <summary>Maximum input sequence length (tokens). 512 is the model cap.</summary>
    public int MaxSequenceLength { get; init; } = 512;

    /// <summary>Batch size for ONNX inference. Tune to memory; CPU-friendly default = 16.</summary>
    public int BatchSize { get; init; } = 16;

    /// <summary>
    /// Prefix added to query inputs (E5 contract). Empty for non-E5 models.
    /// </summary>
    public string QueryPrefix { get; init; } = "query: ";

    /// <summary>
    /// Prefix added to passage inputs (E5 contract). Empty for non-E5 models.
    /// </summary>
    public string PassagePrefix { get; init; } = "passage: ";

    /// <summary>If <c>true</c> and CUDA is available, register the CUDA execution provider.</summary>
    public bool Gpu { get; init; }
}
