namespace Metaheuristics.Tests;

public sealed class RuntimeTargetTests
{
    [Xunit.Fact]
    public void TestHostRunsOnNet10()
    {
        Xunit.Assert.Equal(10, Environment.Version.Major);
    }
}
