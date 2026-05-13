using Microsoft.ML.OnnxRuntime;

namespace DcResearchMcp.Embedding;

/// <summary>
/// Runs the <c>intfloat/multilingual-e5-small</c> ONNX export on CPU
/// (or CUDA via <see cref="EmbeddingOptions.Gpu"/>). XLM-RoBERTa-based:
/// inputs are <c>input_ids</c> and <c>attention_mask</c> (int64); output
/// is <c>last_hidden_state</c> (float, shape <c>B × L × 384</c>). Pipeline
/// = tokenize → ORT run → masked mean-pool → L2-normalise.
/// </summary>
public sealed class E5OnnxEmbedder : IEmbedder
{
    private readonly EmbeddingOptions _options;
    private readonly IPretokenizer _tokenizer;
    private readonly InferenceSession _session;
    private bool _disposed;

    /// <inheritdoc />
    public int Dimension => _options.Dimension;

    /// <summary>Initialise with the loaded ONNX session and a configured tokenizer.</summary>
    public E5OnnxEmbedder(EmbeddingOptions options, IPretokenizer tokenizer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tokenizer);

        _options = options;
        _tokenizer = tokenizer;

        var so = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            IntraOpNumThreads = Environment.ProcessorCount,
        };
        if (options.Gpu)
        {
            so.AppendExecutionProvider_CUDA();
        }
        _session = new InferenceSession(options.ModelPath, so);
    }

    /// <inheritdoc />
    public async Task<float[][]> EmbedAsync(
        IReadOnlyList<string> texts,
        bool isQuery,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(texts);
        if (texts.Count == 0)
        {
            return [];
        }

        var prefix = isQuery ? _options.QueryPrefix : _options.PassagePrefix;
        var prepped = prefix.Length == 0
            ? [.. texts]
            : texts.Select(t => prefix + t).ToList();

        var results = new float[texts.Count][];
        for (var start = 0; start < prepped.Count; start += _options.BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var end = Math.Min(start + _options.BatchSize, prepped.Count);
            var slice = prepped.GetRange(start, end - start);
            var batch = await Task.Run(() => EmbedBatch(slice), cancellationToken).ConfigureAwait(false);
            for (var i = 0; i < batch.Length; i++)
            {
                results[start + i] = batch[i];
            }
        }
        return results;
    }

    private float[][] EmbedBatch(IReadOnlyList<string> batch)
    {
        var tok = _tokenizer.Encode(batch, _options.MaxSequenceLength);
        long[] shape = [tok.BatchSize, tok.SeqLen];

        using var ids = OrtValue.CreateTensorValueFromMemory(tok.InputIds, shape);
        using var mask = OrtValue.CreateTensorValueFromMemory(tok.AttentionMask, shape);

        var inputs = new Dictionary<string, OrtValue>(StringComparer.Ordinal)
        {
            ["input_ids"] = ids,
            ["attention_mask"] = mask,
        };

        using var runOptions = new RunOptions();
        using var outputs = _session.Run(runOptions, inputs, _session.OutputNames);

        // Expected output: last_hidden_state — float[B, L, dim]
        var hidden = outputs[0];
        var hiddenSpan = hidden.GetTensorDataAsSpan<float>();

        var pooled = Pooling.MeanPool(hiddenSpan, tok.AttentionMask, tok.BatchSize, tok.SeqLen, _options.Dimension);
        foreach (var vec in pooled)
        {
            Pooling.L2NormalizeInPlace(vec);
        }
        return pooled;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _session.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
