using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Anastasya.Metaheuristics.Simd.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class SimdCascadeGenerator : IIncrementalGenerator
{
    private const string TemplateSuffix = ".simd.cs";
    private const string AcceleratedWidthExpansionMarker = "__SimdExpandHardwareAcceleratedWidths";
    private static readonly int[] Widths = [512, 256, 128];

    private static readonly DiagnosticDescriptor InvalidSyntax = new(
        "SIMDGEN001",
        "Invalid SIMD template syntax",
        "SIMD template '{0}' contains invalid C# syntax: {1}",
        "SimdGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidMetadata = new(
        "SIMDGEN002",
        "Invalid SIMD template metadata",
        "SIMD template '{0}' must declare exactly one [SimdTemplate(\"element-type\", SimdCapabilities.<capability>)] attribute; {1}",
        "SimdGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidExpansionBlock = new(
        "SIMDGEN003",
        "Invalid SIMD expansion block",
        "Method '{0}' must contain exactly one supported SIMD expansion block",
        "SimdGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedPlaceholder = new(
        "SIMDGEN004",
        "Unsupported SIMD placeholder or capability",
        "SIMD template '{0}' uses unsupported placeholder or capability '{1}'",
        "SimdGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConflictingTarget = new(
        "SIMDGEN005",
        "Conflicting SIMD generation target",
        "SIMD templates generate the same target method '{0}'",
        "SimdGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidContainer = new(
        "SIMDGEN006",
        "Invalid SIMD template container",
        "SIMD template '{0}' may contain only namespaces and partial type containers with methods",
        "SimdGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var templates = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(TemplateSuffix, StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (pair, cancellationToken) =>
            {
                var file = pair.Left;
                var options = pair.Right.GetOptions(file);
                var path = options.TryGetValue(
                    "build_metadata.AdditionalFiles.SimdTemplatePath",
                    out var configuredPath)
                    ? configuredPath
                    : file.Path;
                return new TemplateInput(path, file.GetText(cancellationToken)?.ToString());
            })
            .WithTrackingName("SimdTemplates");

        context.RegisterSourceOutput(templates, static (productionContext, input) =>
            Generate(productionContext, input));
        context.RegisterSourceOutput(templates.Collect(), static (productionContext, inputs) =>
            ReportConflicts(productionContext, inputs));
    }

    private static void ReportConflicts(
        SourceProductionContext context,
        ImmutableArray<TemplateInput> inputs)
    {
        var targets = inputs
            .Where(static input => input.Text is not null)
            .SelectMany(static input => GetTargetMethods(input))
            .GroupBy(static target => target.Key, StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any());

        foreach (var conflict in targets)
        {
            foreach (var target in conflict)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ConflictingTarget,
                    target.Location,
                    conflict.Key));
            }
        }
    }

    private static ImmutableArray<TargetMethod> GetTargetMethods(TemplateInput input)
    {
        var tree = CSharpSyntaxTree.ParseText(input.Text!, path: input.Path);
        if (tree.GetDiagnostics().Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return [];
        }

        var methods = tree.GetCompilationUnitRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
        return methods.Select(method =>
        {
            var containers = method.Ancestors()
                .Where(static ancestor => ancestor is BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax)
                .Reverse()
                .Select(static ancestor => ancestor switch
                {
                    BaseNamespaceDeclarationSyntax @namespace => @namespace.Name.ToString(),
                    TypeDeclarationSyntax type => type.Identifier.ValueText,
                    _ => string.Empty,
                });
            var parameterTypes = string.Join(",", method.ParameterList.Parameters.Select(static parameter =>
                string.Join(" ", parameter.Modifiers.Select(static modifier => modifier.ValueText))
                + ":" + parameter.Type?.WithoutTrivia().ToString()));
            var key = string.Join(".", containers)
                + "." + method.Identifier.ValueText
                + "(" + parameterTypes + ")";
            var location = CreateLocation(input.Path, method.Identifier.GetLocation());
            return new TargetMethod(key, location);
        }).ToImmutableArray();
    }

    private static void Generate(SourceProductionContext context, TemplateInput input)
    {
        if (input.Text is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidSyntax, Location.None, input.Path, "the file could not be read"));
            return;
        }

        var sourceText = SourceText.From(input.Text, Encoding.UTF8);
        var tree = CSharpSyntaxTree.ParseText(sourceText, path: input.Path);
        var root = tree.GetCompilationUnitRoot(context.CancellationToken);
        var syntaxError = tree.GetDiagnostics(context.CancellationToken)
            .FirstOrDefault(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        if (syntaxError is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidSyntax,
                CreateLocation(input.Path, syntaxError.Location),
                input.Path,
                syntaxError.GetMessage(CultureInfo.InvariantCulture)));
            return;
        }

        if (!TryReadMetadata(root, input.Path, context, out var metadata, out var rootWithoutMetadata))
        {
            return;
        }

        if (!ValidateContainers(rootWithoutMetadata, input.Path, context))
        {
            return;
        }

        var methods = rootWithoutMetadata.DescendantNodes().OfType<MethodDeclarationSyntax>().ToImmutableArray();
        foreach (var method in methods)
        {
            var expansions = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(IsExpansionInvocation)
                .ToImmutableArray();
            if (expansions.Length != 1 || !IsValidExpansionBlock(expansions[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InvalidExpansionBlock,
                    CreateLocation(input.Path, method.Identifier.GetLocation()),
                    method.Identifier.ValueText));
                return;
            }
        }

        var invalidPlaceholder = rootWithoutMetadata.DescendantTokens()
            .FirstOrDefault(static token => token.IsKind(SyntaxKind.IdentifierToken)
                && token.ValueText.StartsWith("__", StringComparison.Ordinal)
                && token.ValueText != "__Vector"
                && token.ValueText != AcceleratedWidthExpansionMarker);
        if (invalidPlaceholder.RawKind != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                UnsupportedPlaceholder,
                CreateLocation(input.Path, invalidPlaceholder.GetLocation()),
                input.Path,
                invalidPlaceholder.ValueText));
            return;
        }

        if (metadata.Capability == "Integer" && rootWithoutMetadata.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .Any(static access => access.Name.Identifier.ValueText is "Sqrt" or "Floor" or "Ceiling"))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                UnsupportedPlaceholder,
                Location.Create(input.Path, default, default),
                input.Path,
                "floating-point operation with Integer capability"));
            return;
        }

        var expanded = (CompilationUnitSyntax)new CascadeExpansionRewriter().Visit(rootWithoutMetadata)!;
        expanded = (CompilationUnitSyntax)new GeneratedMethodAttributeRewriter().Visit(expanded)!;

        var normalizedPath = input.Path.Replace('\\', '/');
        var generated = "// <auto-generated/>\n#nullable enable\n"
            + "#line 1 \"" + normalizedPath.Replace("\"", "\"\"") + "\"\n"
            + expanded.NormalizeWhitespace(eol: "\n").ToFullString()
            + "\n#line default\n";

        context.AddSource(CreateHintName(normalizedPath), SourceText.From(generated, Encoding.UTF8));
    }

    private static bool TryReadMetadata(
        CompilationUnitSyntax root,
        string path,
        SourceProductionContext context,
        out TemplateMetadata metadata,
        out CompilationUnitSyntax rootWithoutMetadata)
    {
        var attributes = root.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Where(static attribute => attribute.Name.ToString() == "SimdTemplate")
            .ToImmutableArray();

        if (attributes.Length != 1 || root.AttributeLists.Count != 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidMetadata,
                attributes.Length == 0
                    ? Location.Create(path, default, default)
                    : CreateLocation(path, attributes[0].GetLocation()),
                path,
                "metadata is missing or duplicated"));
            metadata = default;
            rootWithoutMetadata = root;
            return false;
        }

        var arguments = attributes[0].ArgumentList?.Arguments;
        var elementType = arguments is { Count: 2 }
            ? (arguments.Value[0].Expression as LiteralExpressionSyntax)?.Token.ValueText
            : null;
        var capability = arguments is { Count: 2 }
            ? (arguments.Value[1].Expression as MemberAccessExpressionSyntax)?.Name.Identifier.ValueText
            : null;

        var expectedCapability = elementType switch
        {
            "double" or "float" => "FloatingPoint",
            "int" => "Integer",
            _ => null,
        };

        if (expectedCapability is null || capability != expectedCapability)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidMetadata,
                CreateLocation(path, attributes[0].GetLocation()),
                path,
                $"element type '{elementType ?? "<missing>"}' requires a supported matching capability"));
            metadata = default;
            rootWithoutMetadata = root;
            return false;
        }

        metadata = new TemplateMetadata(elementType!, capability!);
        rootWithoutMetadata = root.WithAttributeLists(default);
        return true;
    }

    private static bool ValidateContainers(
        CompilationUnitSyntax root,
        string path,
        SourceProductionContext context)
    {
        if (root.Members.Count == 0
            || root.DescendantNodes().OfType<MethodDeclarationSyntax>().Any() is false
            || root.Members.Any(static member => member is not BaseNamespaceDeclarationSyntax and not TypeDeclarationSyntax))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidContainer, Location.Create(path, default, default), path));
            return false;
        }

        foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (!type.Modifiers.Any(SyntaxKind.PartialKeyword)
                || type.Members.Any(static member => member is not TypeDeclarationSyntax and not MethodDeclarationSyntax))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InvalidContainer,
                    CreateLocation(path, type.Identifier.GetLocation()),
                    path));
                return false;
            }
        }

        foreach (var @namespace in root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
        {
            if (@namespace.Members.Any(static member =>
                member is not BaseNamespaceDeclarationSyntax and not TypeDeclarationSyntax))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InvalidContainer,
                    CreateLocation(path, @namespace.Name.GetLocation()),
                    path));
                return false;
            }
        }

        return true;
    }

    private static bool IsExpansionInvocation(InvocationExpressionSyntax invocation) =>
        invocation.Expression is IdentifierNameSyntax identifier
        && identifier.Identifier.ValueText == AcceleratedWidthExpansionMarker;

    private static bool IsValidExpansionBlock(InvocationExpressionSyntax invocation) =>
        invocation.Parent is ExpressionStatementSyntax
        && invocation.ArgumentList.Arguments.Count == 1
        && invocation.ArgumentList.Arguments[0].Expression is ParenthesizedLambdaExpressionSyntax { Block: not null };

    private static bool TryGetExpansionBlock(
        StatementSyntax statement,
        out BlockSyntax block)
    {
        if (statement is ExpressionStatementSyntax
            {
                Expression: InvocationExpressionSyntax invocation,
            }
            && IsExpansionInvocation(invocation)
            && invocation.ArgumentList.Arguments[0].Expression is ParenthesizedLambdaExpressionSyntax
            {
                Block: { } templateBlock,
            })
        {
            block = templateBlock;
            return true;
        }

        block = null!;
        return false;
    }

    private static IfStatementSyntax CreateHardwareAccelerationGuard(int width, BlockSyntax block) =>
        SyntaxFactory.IfStatement(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("Vector" + width.ToString(CultureInfo.InvariantCulture)),
                SyntaxFactory.IdentifierName("IsHardwareAccelerated")),
            block);

    private static Location CreateLocation(string path, Location syntaxLocation)
    {
        var lineSpan = syntaxLocation.GetLineSpan().Span;
        return Location.Create(path, syntaxLocation.SourceSpan, lineSpan);
    }

    private static string CreateHintName(string path)
    {
        uint hash = 2166136261;
        foreach (var character in path)
        {
            hash = (hash ^ character) * 16777619;
        }

        var fileName = path.Split('/').Last();
        var sanitized = new string(fileName.Select(static character =>
            char.IsLetterOrDigit(character) ? character : '_').ToArray());
        return $"SimdCascade.{sanitized}.{hash:x8}.g.cs";
    }

    private readonly struct TemplateInput
    {
        public TemplateInput(string path, string? text)
        {
            Path = path;
            Text = text;
        }

        public string Path { get; }

        public string? Text { get; }
    }

    private readonly struct TemplateMetadata
    {
        public TemplateMetadata(string elementType, string capability)
        {
            ElementType = elementType;
            Capability = capability;
        }

        public string ElementType { get; }

        public string Capability { get; }
    }

    private readonly struct TargetMethod
    {
        public TargetMethod(string key, Location location)
        {
            Key = key;
            Location = location;
        }

        public string Key { get; }

        public Location Location { get; }
    }

    private sealed class CascadeExpansionRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) =>
            node.WithMembers(ExpandMembers(node.Members));

        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) =>
            node.WithMembers(ExpandMembers(node.Members));

        public override SyntaxNode? VisitBlock(BlockSyntax node)
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            foreach (var statement in node.Statements)
            {
                if (TryGetExpansionBlock(statement, out var templateBlock))
                {
                    foreach (var width in Widths)
                    {
                        var expandedBlock = (BlockSyntax)new WidthPlaceholderRewriter(width).Visit(templateBlock)!;
                        statements.Add(CreateHardwareAccelerationGuard(width, expandedBlock));
                    }
                }
                else
                {
                    statements.Add((StatementSyntax)base.Visit(statement)!);
                }
            }

            return node.WithStatements(SyntaxFactory.List(statements));
        }

        private SyntaxList<MemberDeclarationSyntax> ExpandMembers(SyntaxList<MemberDeclarationSyntax> members)
        {
            var expandedMembers = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();
            foreach (var member in members)
            {
                expandedMembers.Add((MemberDeclarationSyntax)Visit(member)!);
            }

            return SyntaxFactory.List(expandedMembers);
        }
    }

    private sealed class WidthPlaceholderRewriter : CSharpSyntaxRewriter
    {
        private readonly int width;

        public WidthPlaceholderRewriter(int width)
        {
            this.width = width;
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (!token.IsKind(SyntaxKind.IdentifierToken))
            {
                return base.VisitToken(token);
            }

            var replacement = token.ValueText == "__Vector" ? $"Vector{width}" : null;

            return replacement is null
                ? base.VisitToken(token)
                : SyntaxFactory.Identifier(token.LeadingTrivia, replacement, token.TrailingTrivia);
        }
    }

    private sealed class GeneratedMethodAttributeRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var visited = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
            var leadingTrivia = visited.GetLeadingTrivia();
            visited = visited.WithoutLeadingTrivia();
            var attributes = SyntaxFactory.AttributeList(
                SyntaxFactory.SeparatedList([
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::System.CodeDom.Compiler.GeneratedCode"))
                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(nameof(SimdCascadeGenerator)))),
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal("1.0.0"))),
                        ]))),
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::System.Runtime.CompilerServices.CompilerGenerated")),
                ])).WithLeadingTrivia(leadingTrivia);
            return visited.AddAttributeLists(attributes);
        }
    }
}
