using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Anastasya.Metaheuristics.Simd.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Anastasya.Metaheuristics.Simd.Generators.Tests;

public sealed class SimdCascadeGeneratorTests
{
    [Theory]
    [InlineData("double", "FloatingPoint")]
    [InlineData("float", "FloatingPoint")]
    [InlineData("int", "Integer")]
    public void ExpandsSupportedElementTypes(string elementType, string capability)
    {
        var result = Run(CreateTemplate(elementType, capability));

        Assert.Empty(result.Diagnostics);
        var source = Assert.Single(result.GeneratedSources).SourceText.ToString();
        Assert.Contains($"Vector512<{elementType}>.Count", source, StringComparison.Ordinal);
        Assert.Contains($"Vector256<{elementType}>.Count", source, StringComparison.Ordinal);
        Assert.Contains($"Vector128<{elementType}>.Count", source, StringComparison.Ordinal);
        Assert.DoesNotContain("__Vector", source, StringComparison.Ordinal);
        Assert.DoesNotContain("__Width", source, StringComparison.Ordinal);
    }

    [Fact]
    public void GoldenOutputIsDeterministic()
    {
        const string template = """
            [assembly: SimdTemplate("double", SimdCapabilities.FloatingPoint)]
            namespace Demo;
            internal static partial class Kernel
            {
                internal static int Count()
                {
                    var count = 0;
                    __SimdExpandWidths(() =>
                    {
                        if (Vector512.IsHardwareAccelerated)
                        {
                            count += __Vector<double>.Count;
                            Consume__Width();
                        }
                    });
                    return count;
                }
            }
            """;

        var first = Run(template);
        var second = Run(template);

        Assert.Empty(first.Diagnostics);
        Assert.Equal(
            Assert.Single(first.GeneratedSources).SourceText.ToString(),
            Assert.Single(second.GeneratedSources).SourceText.ToString());
        Assert.Equal(
            Assert.Single(first.GeneratedSources).HintName,
            Assert.Single(second.GeneratedSources).HintName);

        var source = Assert.Single(first.GeneratedSources).SourceText.ToString();
        var vector512 = source.IndexOf("Vector512<double>.Count", StringComparison.Ordinal);
        var vector256 = source.IndexOf("Vector256<double>.Count", StringComparison.Ordinal);
        var vector128 = source.IndexOf("Vector128<double>.Count", StringComparison.Ordinal);
        Assert.True(vector512 >= 0 && vector512 < vector256 && vector256 < vector128);
        Assert.Contains("Consume512();", source, StringComparison.Ordinal);
        Assert.Contains("Consume256();", source, StringComparison.Ordinal);
        Assert.Contains("Consume128();", source, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(15)]
    [InlineData(16)]
    public void GeneratedCascadePreservesScalarTail(int length)
    {
        const string template = """
            using System.Runtime.Intrinsics;
            [assembly: SimdTemplate("int", SimdCapabilities.Integer)]
            namespace Demo;
            internal static partial class Kernel
            {
                public static int CountProcessed(int length)
                {
                    var index = 0;
                    var processed = 0;
                    __SimdExpandWidths(() =>
                    {
                        if (__Vector.IsHardwareAccelerated)
                        {
                            var widthEnd = length - __Vector<int>.Count;
                            while (index <= widthEnd)
                            {
                                processed += __Vector<int>.Count;
                                index += __Vector<int>.Count;
                            }
                        }
                    });
                    while (index < length)
                    {
                        processed++;
                        index++;
                    }

                    return processed;
                }
            }
            """;

        var runResult = Run(template);
        Assert.Empty(runResult.Diagnostics);
        var generatedSource = Assert.Single(runResult.GeneratedSources).SourceText.ToString();
        var assembly = Compile(generatedSource);
        var method = assembly.GetType("Demo.Kernel", throwOnError: true)!
            .GetMethod("CountProcessed", BindingFlags.Public | BindingFlags.Static)!;

        Assert.Equal(length, method.Invoke(null, [length]));
    }

    [Fact]
    public void DistinctPathsProduceDistinctStableHintNames()
    {
        var template = CreateTemplate("double", "FloatingPoint");
        var result = Run([
            new TestAdditionalText("a/Kernel.simd.cs", template),
            new TestAdditionalText(
                "b/Kernel.simd.cs",
                template.Replace("class Kernel", "class OtherKernel", StringComparison.Ordinal)),
        ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(2, result.GeneratedSources.Length);
        Assert.NotEqual(result.GeneratedSources[0].HintName, result.GeneratedSources[1].HintName);
    }

    [Fact]
    public void ReportsConflictingTargetMethodsAcrossTemplates()
    {
        var template = CreateTemplate("double", "FloatingPoint");
        var result = Run([
            new TestAdditionalText("a/Kernel.simd.cs", template),
            new TestAdditionalText("b/Kernel.simd.cs", template),
        ]);

        Assert.Equal(2, result.Diagnostics.Count(static diagnostic => diagnostic.Id == "SIMDGEN005"));
        Assert.All(
            result.Diagnostics.Where(static diagnostic => diagnostic.Id == "SIMDGEN005"),
            static diagnostic => Assert.EndsWith("Kernel.simd.cs", diagnostic.Location.GetLineSpan().Path));
    }

    [Fact]
    public void UnchangedTemplateUsesIncrementalCache()
    {
        var compilation = CSharpCompilation.Create("GeneratorTests");
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new SimdCascadeGenerator().AsSourceGenerator()],
            additionalTexts: [new TestAdditionalText(
                "Kernel.simd.cs",
                CreateTemplate("double", "FloatingPoint"))],
            parseOptions: new CSharpParseOptions(LanguageVersion.CSharp14),
            driverOptions: new GeneratorDriverOptions(
                IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var result = Assert.Single(driver.GetRunResult().Results);
        var steps = result.TrackedSteps["SimdTemplates"];
        Assert.All(
            steps.SelectMany(static step => step.Outputs),
            static output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason));
    }

    [Theory]
    [InlineData("double", "Integer")]
    [InlineData("int", "FloatingPoint")]
    [InlineData("decimal", "FloatingPoint")]
    public void RejectsUnsupportedTypeCapabilityPairs(string elementType, string capability)
    {
        var result = Run(CreateTemplate(elementType, capability));

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("SIMDGEN002", diagnostic.Id);
        Assert.Empty(result.GeneratedSources);
    }

    [Fact]
    public void RejectsMissingOrDuplicateExpansionBlocks()
    {
        var missing = Run(CreateTemplate("double", "FloatingPoint").Replace(
            "__SimdExpandWidths(() =>",
            "Consume(() =>",
            StringComparison.Ordinal));
        var duplicate = Run(CreateTemplate("double", "FloatingPoint").Replace(
            "return count;",
            "__SimdExpandWidths(() => { count++; }); return count;",
            StringComparison.Ordinal));

        Assert.Equal("SIMDGEN003", Assert.Single(missing.Diagnostics).Id);
        Assert.Equal("SIMDGEN003", Assert.Single(duplicate.Diagnostics).Id);
    }

    [Fact]
    public void RejectsUnknownPlaceholder()
    {
        var result = Run(CreateTemplate("double", "FloatingPoint").Replace(
            "return count;",
            "return __Unknown;",
            StringComparison.Ordinal));

        Assert.Equal("SIMDGEN004", Assert.Single(result.Diagnostics).Id);
    }

    [Fact]
    public void RejectsFloatingPointOperationForIntegerCapability()
    {
        var result = Run(CreateTemplate("int", "Integer").Replace(
            "count += __Vector<int>.Count;",
            "count += (int)__Vector.Sqrt(default).GetElement(0);",
            StringComparison.Ordinal));

        Assert.Equal("SIMDGEN004", Assert.Single(result.Diagnostics).Id);
    }

    [Fact]
    public void DiagnosticKeepsTemplateLocation()
    {
        var result = Run(CreateTemplate("double", "FloatingPoint").Replace(
            "return count;",
            "return __Unknown;",
            StringComparison.Ordinal),
            "templates/Kernel.simd.cs");

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("templates/Kernel.simd.cs", diagnostic.Location.GetLineSpan().Path);
        Assert.True(diagnostic.Location.GetLineSpan().StartLinePosition.Line > 0);
    }

    private static GeneratorRunResult Run(string template, string path = "Kernel.simd.cs") =>
        Run([new TestAdditionalText(path, template)]);

    private static GeneratorRunResult Run(ImmutableArray<AdditionalText> additionalTexts)
    {
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            syntaxTrees: [],
            references: [],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new SimdCascadeGenerator().AsSourceGenerator()],
            additionalTexts: additionalTexts,
            parseOptions: new CSharpParseOptions(LanguageVersion.CSharp14));

        driver = driver.RunGenerators(compilation);
        return Assert.Single(driver.GetRunResult().Results);
    }

    private static Assembly Compile(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            "GeneratedCascade" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14))],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }

    private static string CreateTemplate(string elementType, string capability) => $$"""
        [assembly: SimdTemplate("{{elementType}}", SimdCapabilities.{{capability}})]
        namespace Demo;
        internal static partial class Kernel
        {
            internal static int Count()
            {
                var count = 0;
                __SimdExpandWidths(() =>
                {
                    if (__Vector.IsHardwareAccelerated)
                    {
                        count += __Vector<{{elementType}}>.Count;
                        Consume__Width();
                    }
                });
                return count;
            }
        }
        """;

    private sealed class TestAdditionalText(string path, string text) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            SourceText.From(text, Encoding.UTF8);
    }
}
