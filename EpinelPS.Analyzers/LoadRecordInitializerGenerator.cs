using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace EpinelPS.Analyzers;

[Generator]
public class LoadRecordInitializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Filter for field declarations with attributes
        var fieldDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is FieldDeclarationSyntax fds && fds.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetTargetFieldInfo(ctx)
            )
            .Where(static m => m is not null)
            .Collect();
            

        // Step 2: Generate the code
        context.RegisterSourceOutput(fieldDeclarations, (spc, fieldInfos) =>
        {
            var source = GenerateInitializerCode(fieldInfos!);
            spc.AddSource("GameDataInitializer.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static LoadFieldInfo? GetTargetFieldInfo(GeneratorSyntaxContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDecl)
            return null;

        var variable = fieldDecl.Declaration.Variables.FirstOrDefault();
        if (variable == null)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(variable) is not IFieldSymbol symbol)
            return null;

        foreach (var attr in symbol.GetAttributes())
        {

            if (attr.ConstructorArguments.Length == 3)
            {
                if (attr.ConstructorArguments[0].Value is not string fileName || attr.ConstructorArguments[1].Value is not string key || attr.ConstructorArguments[2].Value is not INamedTypeSymbol recordType)
                    return null;

                return new LoadFieldInfo
                {
                    ContainingClass = symbol.ContainingType.ToDisplayString(),
                    FieldName = symbol.Name,
                    FileName = fileName,
                    Key = key,
                    RecordTypeName = recordType.ToDisplayString()
                };
            }
        }

        return null;
    }

    private static string GenerateInitializerCode(ImmutableArray<LoadFieldInfo> fieldInfos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("namespace EpinelPS.Data;");
        sb.AppendLine("public static class GameDataInitializer");
        sb.AppendLine("{");
        sb.AppendFormat($"public static int TotalFiles = {fieldInfos.Length};");
        sb.AppendLine("    public static async Task InitializeGameData(IProgress<double> progress = null)");
        sb.AppendLine("    {");

        foreach (var info in fieldInfos)
        {
            var tempVar = $"data_{info.FieldName}";
            sb.AppendLine($"        var {tempVar} = await {info.ContainingClass}.Instance.LoadZip<{info.RecordTypeName}>(\"{info.FileName}\", progress);");
            sb.AppendLine($"        foreach (var obj in {tempVar}.records)");
            sb.AppendLine("        {");
            sb.AppendLine($"            {info.ContainingClass}.Instance.{info.FieldName}.Add(obj.{info.Key}, obj);");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private class LoadFieldInfo
    {
        public string ContainingClass = "";
        public string FieldName = "";
        public string FileName = "";
        public string Key = "";
        public string RecordTypeName = "";
    }
}