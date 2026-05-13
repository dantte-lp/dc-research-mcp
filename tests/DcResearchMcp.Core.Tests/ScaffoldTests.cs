namespace DcResearchMcp.Core.Tests;

public class ScaffoldTests
{
    [Fact]
    public void Scaffold_compiles_and_references_Core()
    {
        typeof(Core.AssemblyMarker).Assembly.GetName().Name.Should().Be("DcResearchMcp.Core");
    }
}
