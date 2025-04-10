using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using WCKYWCKF.RxUI.Model2ViewModel.Model;
using static System.String;

namespace WCKYWCKF.RxUI.Model2ViewModel;

#pragma warning disable RS1038 RS1041
[Generator]
#pragma warning restore RS1038 RS1041
public partial class M2VMGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("WCKYWCKF.Model2ViewModel.M2VMGenerator.Attributes.g.cs", M2VMHelper.M2VMAttributeContent));

        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                M2VMHelper.M2VMGenerationInfoAttributeStr,
                Predicate,
                GetGenerateViewModelInfo)
            .Where(x => x is not null)
            .Collect();

        // var provider = context.CompilationProvider.Combine(classDeclarations);

        context.RegisterSourceOutput(classDeclarations, static (spc, source) => Execute(spc, source!));
    }

    private bool Predicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return syntaxNode
            is ClassDeclarationSyntax { AttributeLists.Count: > 0 }
            or RecordDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private M2VMGenerationInfo? GetGenerateViewModelInfo(
        GeneratorAttributeSyntaxContext generatorAttributeSyntaxContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (generatorAttributeSyntaxContext.TargetNode is not TypeDeclarationSyntax) return null;

        return generatorAttributeSyntaxContext.TargetSymbol is not ITypeSymbol symbol
            ? null
            : new M2VMGenerationInfo(symbol, cancellationToken);
        // var attributeDatas = symbol.GetAttributes()
        //     .Where(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         return x.AttributeClass?.ContainingNamespace
        //             .ToDisplayString(GetFullTypeText)
        //             .StartsWith("WCKYWCKF.Model2ViewModel") is true;
        //     })
        //     .GroupBy(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         return x.AttributeClass?.Name;
        //     })
        //     .ToList();
        //
        // var gMode = attributeDatas.Find(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         return x.Key is "GenerateViewModelModeAttribute";
        //     })?
        //     .Select(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         TryGetNamedArgument(x, "EnableUseNewInit", out bool? enableUseNewInit);
        //         TryGetNamedArgument(x, "EnableGenerateToModel", out bool? enableGenerateToModel);
        //         return (enableUseNewInit, enableGenerateToModel);
        //     })
        //     .FirstOrDefault();
        // return new M2VMGenerationInfo(
        //     symbol.ContainingNamespace.ToDisplayString(),
        //     getGenerateMode(),
        //     (attributeDatas.Find(x =>
        //         {
        //             cancellationToken.ThrowIfCancellationRequested();
        //             return x.Key is GenerateViewModelName;
        //         })?
        //         .Select(x =>
        //         {
        //             cancellationToken.ThrowIfCancellationRequested();
        //             TryGetNamedArgument(x, "ModelType", out INamedTypeSymbol? modelType);
        //             return modelType ?? null;
        //         })
        //         .Where(x =>
        //         {
        //             cancellationToken.ThrowIfCancellationRequested();
        //             return x is not null;
        //         })
        //         .ToList() ?? [])!,
        //     (attributeDatas.Find(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         return x.Key is GenerateViewModelIgnoreName;
        //     })?.Select(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         TryGetNamedArgument(x, "PropertyName", out string? propertyName);
        //         TryGetNamedArgument(x, "ModelType", out INamedTypeSymbol? modelType);
        //         if (IsNullOrEmpty(propertyName) || modelType is null) return null;
        //         return new M2VMPropertyOrFieldOperationInfo(propertyName, modelType);
        //     }).ToList() ?? [])!,
        //     (attributeDatas.Find(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         return x.Key is GenerateViewModelReplaceName;
        //     })?.Select(x =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         TryGetNamedArgument(x, "PropertyName", out string? propertyName);
        //         TryGetNamedArgument(x, "ModelType", out INamedTypeSymbol? modelType);
        //         TryGetNamedArgument(x, "ReplaceWithType", out INamedTypeSymbol? replaceWithType);
        //         if (IsNullOrEmpty(propertyName)
        //             || modelType is null
        //             || replaceWithType is null) return null;
        //         return new M2VMReplaceGenerationInfo(propertyName, modelType, replaceWithType);
        //     }).ToList() ?? [])!,
        //     gMode?.enableUseNewInit ?? false,
        //     gMode?.enableGenerateToModel ?? false
        // );
    }
}