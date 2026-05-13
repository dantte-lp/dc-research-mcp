using DcResearchMcp.Embedding;

namespace DcResearchMcp.Embedding.Tests;

public class PoolingTests
{
    [Fact]
    public void Mean_pool_with_full_mask_averages_each_dim()
    {
        // batch=1, seq=2, hidden=3
        float[] hidden = [1, 2, 3, 5, 6, 7];      // tok0 = (1,2,3), tok1 = (5,6,7)
        long[] mask = [1, 1];
        var pooled = Pooling.MeanPool(hidden, mask, batchSize: 1, seqLen: 2, hiddenSize: 3);

        pooled.Should().HaveCount(1);
        pooled[0].Should().HaveCount(3);
        pooled[0][0].Should().BeApproximately(3f, 1e-6f);  // (1+5)/2
        pooled[0][1].Should().BeApproximately(4f, 1e-6f);  // (2+6)/2
        pooled[0][2].Should().BeApproximately(5f, 1e-6f);  // (3+7)/2
    }

    [Fact]
    public void Mean_pool_ignores_padding_tokens()
    {
        // batch=1, seq=3, hidden=2; the third token is padding (mask=0).
        float[] hidden = [1, 1, 3, 3, 99, 99];
        long[] mask = [1, 1, 0];
        var pooled = Pooling.MeanPool(hidden, mask, 1, 3, 2);

        pooled[0][0].Should().BeApproximately(2f, 1e-6f);  // (1+3)/2 — padding excluded
        pooled[0][1].Should().BeApproximately(2f, 1e-6f);
    }

    [Fact]
    public void Mean_pool_handles_multiple_batches_independently()
    {
        // batch=2, seq=1, hidden=2
        float[] hidden = [1, 2, 10, 20];
        long[] mask = [1, 1];
        var pooled = Pooling.MeanPool(hidden, mask, 2, 1, 2);

        pooled[0].Should().Equal(1f, 2f);
        pooled[1].Should().Equal(10f, 20f);
    }

    [Fact]
    public void Mean_pool_with_all_zero_mask_returns_zero_vector_without_divide_by_zero()
    {
        float[] hidden = [1, 2, 3];
        long[] mask = [0];
        var pooled = Pooling.MeanPool(hidden, mask, 1, 1, 3);

        pooled[0].Should().Equal(0f, 0f, 0f);
    }

    [Fact]
    public void L2_normalize_makes_unit_vector()
    {
        Span<float> v = [3f, 4f];
        Pooling.L2NormalizeInPlace(v);

        v[0].Should().BeApproximately(0.6f, 1e-6f);
        v[1].Should().BeApproximately(0.8f, 1e-6f);
        var norm = MathF.Sqrt((v[0] * v[0]) + (v[1] * v[1]));
        norm.Should().BeApproximately(1f, 1e-6f);
    }

    [Fact]
    public void L2_normalize_zero_vector_stays_zero()
    {
        Span<float> v = [0f, 0f, 0f];
        Pooling.L2NormalizeInPlace(v);
        v[0].Should().Be(0f);
        v[1].Should().Be(0f);
        v[2].Should().Be(0f);
    }

    [Fact]
    public void Mean_pool_argument_validation_rejects_wrong_buffer_sizes()
    {
        float[] hidden = [1, 2, 3];
        long[] mask = [1];
        var act = () => Pooling.MeanPool(hidden, mask, batchSize: 1, seqLen: 2, hiddenSize: 3);
        act.Should().Throw<ArgumentException>().WithParameterName("hidden");

        float[] hidden2 = new float[6];
        long[] mask2 = [1];
        var act2 = () => Pooling.MeanPool(hidden2, mask2, batchSize: 1, seqLen: 2, hiddenSize: 3);
        act2.Should().Throw<ArgumentException>().WithParameterName("attentionMask");
    }
}
