namespace DcResearchMcp.Data.Tests;

public class ScaffoldTests
{
    [Fact]
    public void Scaffold_compiles_and_references_Data()
    {
        typeof(Data.AssemblyMarker).Assembly.GetName().Name.Should().Be("DcResearchMcp.Data");
    }
}
