using System.Text.RegularExpressions;

namespace DcResearchMcp.Core.Chunking;

/// <summary>
/// Header-aware markdown chunker matching the Python sibling
/// (<c>dc-research-mcp-py</c>) baseline: split by H1/H2/H3 first, then recursively
/// by <c>\n\n</c> / <c>\n</c> / <c>. </c> / <c>? </c> / <c>! </c> / <c>; </c> /
/// space until each chunk fits <see cref="ChunkingOptions.TargetChars"/>. Adjacent
/// chunks overlap by <see cref="ChunkingOptions.OverlapChars"/> characters; chunks
/// shorter than <see cref="ChunkingOptions.MinChunkChars"/> are dropped.
/// </summary>
public sealed partial class MarkdownChunker(ChunkingOptions options) : IChunker
{
    private static readonly string[] s_separators = ["\n\n", "\n", ". ", "? ", "! ", "; ", " "];

    private readonly ChunkingOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    [GeneratedRegex(@"^(?<hashes>\#{1,3})\s+(?<title>.+?)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex HeaderRegex();

    /// <inheritdoc />
    public IReadOnlyList<ChunkDraft> Chunk(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var sections = SplitByHeaders(markdown);
        var drafts = new List<ChunkDraft>();
        var idx = 0;

        foreach (var section in sections)
        {
            foreach (var piece in RecursiveSplit(section.Body, _options.TargetChars, _options.OverlapChars))
            {
                if (piece.Length < _options.MinChunkChars)
                {
                    continue;
                }
                drafts.Add(new ChunkDraft(
                    Text: piece,
                    ChunkIndex: idx++,
                    H1: section.H1,
                    H2: section.H2,
                    H3: section.H3,
                    SizeChars: piece.Length));
            }
        }

        return drafts;
    }

    /// <summary>
    /// Split markdown by H1/H2/H3 headings. Each emitted section carries the
    /// nearest headings of each level visible at its start. Code fences (``` ... ```)
    /// are honoured — `#` lines inside code fences are not treated as headings.
    /// </summary>
    internal static IReadOnlyList<Section> SplitByHeaders(string markdown)
    {
        var sections = new List<Section>();
        string? h1 = null;
        string? h2 = null;
        string? h3 = null;
        var buffer = new List<string>();
        var inCode = false;

        void Flush()
        {
            var body = string.Join('\n', buffer).Trim();
            if (body.Length > 0)
            {
                sections.Add(new Section(h1, h2, h3, body));
            }
            buffer.Clear();
        }

        foreach (var raw in markdown.Split('\n'))
        {
            var trimmedStart = raw.TrimStart();
            if (trimmedStart.StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
                buffer.Add(raw);
                continue;
            }
            if (inCode)
            {
                buffer.Add(raw);
                continue;
            }

            var match = HeaderRegex().Match(raw);
            if (match.Success)
            {
                Flush();
                var level = match.Groups["hashes"].Length;
                var title = match.Groups["title"].Value.Trim();
                switch (level)
                {
                    case 1:
                        h1 = title; h2 = null; h3 = null;
                        break;
                    case 2:
                        h2 = title; h3 = null;
                        break;
                    case 3:
                        h3 = title;
                        break;
                }
                buffer.Add(raw);
            }
            else
            {
                buffer.Add(raw);
            }
        }
        Flush();
        return sections;
    }

    /// <summary>
    /// Split <paramref name="text"/> into pieces of at most <paramref name="targetChars"/>
    /// characters using a cascade of separators; produce <paramref name="overlap"/>-character
    /// overlap between adjacent pieces.
    /// </summary>
    internal static IReadOnlyList<string> RecursiveSplit(string text, int targetChars, int overlap)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= targetChars)
        {
            return string.IsNullOrEmpty(text) ? [] : [text];
        }

        var pieces = new List<string> { text };
        foreach (var sep in s_separators)
        {
            var next = new List<string>(pieces.Count);
            foreach (var p in pieces)
            {
                if (p.Length <= targetChars)
                {
                    next.Add(p);
                    continue;
                }
                var parts = p.Split(sep);
                var cur = string.Empty;
                foreach (var part in parts)
                {
                    var add = string.IsNullOrEmpty(cur) ? part : cur + sep + part;
                    if (add.Length <= targetChars)
                    {
                        cur = add;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(cur))
                        {
                            next.Add(cur);
                        }
                        cur = part;
                    }
                }
                if (!string.IsNullOrEmpty(cur))
                {
                    next.Add(cur);
                }
            }
            pieces = next;
            if (pieces.TrueForAll(p => p.Length <= targetChars))
            {
                break;
            }
        }

        if (overlap <= 0 || pieces.Count <= 1)
        {
            return pieces;
        }

        var output = new List<string>(pieces.Count) { pieces[0] };
        for (var i = 1; i < pieces.Count; i++)
        {
            var prev = pieces[i - 1];
            var tail = prev.Length <= overlap ? prev : prev[^overlap..];
            output.Add(tail + ' ' + pieces[i]);
        }
        return output;
    }
}
