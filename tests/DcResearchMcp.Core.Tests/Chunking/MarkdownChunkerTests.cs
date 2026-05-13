using DcResearchMcp.Core.Chunking;

namespace DcResearchMcp.Core.Tests.Chunking;

public class MarkdownChunkerTests
{
    private static MarkdownChunker Make(int target = 200, int overlap = 50, int min = 20)
        => new(new ChunkingOptions { TargetChars = target, OverlapChars = overlap, MinChunkChars = min });

    [Fact]
    public void Single_short_section_yields_one_chunk_with_headings_carried()
    {
        var md = "# Title\n\n## Section A\n\nShort body about adjacency rules and Li-ion batteries.";
        var chunks = Make().Chunk(md);

        chunks.Should().HaveCount(1);
        chunks[0].H1.Should().Be("Title");
        chunks[0].H2.Should().Be("Section A");
        chunks[0].H3.Should().BeNull();
        chunks[0].Text.Should().Contain("Li-ion");
        chunks[0].ChunkIndex.Should().Be(0);
    }

    [Fact]
    public void New_H2_resets_H3_but_keeps_H1()
    {
        var md = """
            # Doc

            ## A

            ### A1

            body A1

            ## B

            body B
            """;
        var chunks = Make(target: 100, min: 1).Chunk(md);

        chunks.Should().Contain(c => c.Text.Contains("body A1", StringComparison.Ordinal)
            && c.H1 == "Doc" && c.H2 == "A" && c.H3 == "A1");
        chunks.Should().Contain(c => c.Text.Contains("body B", StringComparison.Ordinal)
            && c.H1 == "Doc" && c.H2 == "B" && c.H3 == null);
    }

    [Fact]
    public void Hash_marks_inside_code_fence_are_not_headings()
    {
        var md = """
            # Real heading

            ```bash
            # not a heading — shell comment
            echo hi
            ```

            tail body
            """;
        var sections = MarkdownChunker.SplitByHeaders(md);
        sections.Should().HaveCount(1);
        sections[0].H1.Should().Be("Real heading");
    }

    [Fact]
    public void Oversized_section_splits_with_overlap()
    {
        var sentence = "Каждое требование ОБЯЗАНО быть верифицируемым. ";
        var body = string.Concat(Enumerable.Repeat(sentence, 30));   // ~1300 chars
        var md = $"# Doc\n\n## Section\n\n{body}";

        var chunks = Make(target: 300, overlap: 80, min: 50).Chunk(md);

        chunks.Should().HaveCountGreaterThan(1);
        foreach (var c in chunks)
        {
            c.SizeChars.Should().BeLessThanOrEqualTo(300 + 80 + 16); // overlap tail + separator
            c.H2.Should().Be("Section");
        }

        // Overlap check: tail of chunk[i-1] appears at the head of chunk[i].
        for (var i = 1; i < chunks.Count; i++)
        {
            var prevTail = chunks[i - 1].Text[^Math.Min(20, chunks[i - 1].Text.Length)..];
            chunks[i].Text.Should().Contain(prevTail[..Math.Min(10, prevTail.Length)]);
        }
    }

    [Fact]
    public void Chunks_below_min_chars_are_dropped()
    {
        var md = "# Doc\n\n## Tiny\n\nXYZ\n\n## Real\n\n" + new string('a', 500);
        var chunks = Make(target: 200, overlap: 0, min: 100).Chunk(md);

        chunks.Should().NotBeEmpty();
        chunks.Should().OnlyContain(c => c.SizeChars >= 100);
    }
}
