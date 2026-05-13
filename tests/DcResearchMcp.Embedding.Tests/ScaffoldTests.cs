namespace DcResearchMcp.Embedding.Tests;

public class ScaffoldTests
{
    [Fact]
    public void Scaffold_compiles_and_references_Embedding()
    {
        typeof(Embedding.AssemblyMarker).Assembly.GetName().Name.Should().Be("DcResearchMcp.Embedding");
    }
}
