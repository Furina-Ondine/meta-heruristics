namespace Anastasya.Metaheuristics.Tests;

/// <summary>
/// 验证测试宿主使用项目声明的 .NET 运行时版本运行。
/// </summary>
public sealed class RuntimeTargetTests
{
    /// <summary>
    /// 验证当前运行时的主版本为 .NET 10。
    /// </summary>
    [Xunit.Fact]
    public void TestHostRunsOnNet10()
    {
        Xunit.Assert.Equal(10, Environment.Version.Major);
    }
}
