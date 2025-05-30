﻿using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public static class M2VMHelper
{
    public const string M2VMSaveGenerationInfoAttributeStr = "WCKYWCKF.Model2ViewModel.M2VMSaveGenerationInfoAttribute";
    public const string M2VMGenerationInfoAttributeStr = "WCKYWCKF.Model2ViewModel.M2VMGenerationInfoAttribute";
    public const string M2VMUseAutoFieldAttributeStr = "WCKYWCKF.Model2ViewModel.M2VMUseAutoFieldAttribute";

    public const string M2VMPropertyOrFieldOperationInfoAttributeStr =
        "WCKYWCKF.Model2ViewModel.M2VMPropertyOrFieldOperationInfoAttribute";

    public const string M2VMReplaceGenerationInfoAttribute =
        "WCKYWCKF.Model2ViewModel.M2VMReplaceGenerationInfoAttribute";


    public const string M2VMAttributeContent =
        """
        // <auto-generated/>
        #nullable enable

        namespace WCKYWCKF.Model2ViewModel;

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        internal sealed class M2VMSaveGenerationInfoAttribute : System.Attribute
        {
            public required string SaveFilePath { get; init; }
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        internal sealed class M2VMGenerationInfoAttribute : System.Attribute
        {
            public required System.Type TargetTypeFQType { get; init; }
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        internal sealed class M2VMPropertyOrFieldOperationInfoAttribute : System.Attribute
        {
            public required System.Type TargetTypeFQType { get; init; }
            public required string TargetMemberName { get; init; }
            public required PropertyOrFieldOperationKind TargetOperation { get; init; }
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        internal sealed class M2VMReplaceGenerationInfoAttribute : System.Attribute
        {
            public required System.Type TargetTypeFQType { get; init; }
            public required string TargetMemberName { get; init; }
            public required System.Type ReplaceFQType { get; init; }
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        internal sealed class M2VMUseAutoFieldAttribute : System.Attribute
        {
        }

        [global::System.Flags]
        internal enum PropertyOrFieldOperationKind
        {
            IgnoreProperty = 1,
            IncludePropertyOrField = 1 << 1,
            DoNotReplaceTargetType = 1 << 2,
            TypeIsNullable = 1 << 3,
            TypeIsNotNullable = 1 << 4
        }
        #nullable disable
        """;

    public const string TabStr = "    ";

    public const string GenerateViewModelConvertTemplate =
        $$$"""
           {{{TabStr}}}public {0} ToModel()
           {{{TabStr}}}{{
           {{{TabStr}}}{{{TabStr}}}return new() {{
           {1}
           {{{TabStr}}}{{{TabStr}}}}};
           {{{TabStr}}}}}

           {{{TabStr}}}public static {2} CreateViewModel({3}? model)
           {{{TabStr}}}{{
           {{{TabStr}}}{{{TabStr}}}return new() {{
           {4}
           {{{TabStr}}}{{{TabStr}}}}};
           {{{TabStr}}}}}

           {{{TabStr}}}public static {5} CreateNonNullPropertiesViewModel()
           {{{TabStr}}}{{
           {{{TabStr}}}{{{TabStr}}}return new() {{
           {6}
           {{{TabStr}}}{{{TabStr}}}}};
           {{{TabStr}}}}}
           """;

    public static readonly SymbolDisplayFormat GlobalSymbolDisplayFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);

    public static bool IsSystemType(ITypeSymbol symbol)
    {
        if (symbol is IArrayTypeSymbol arrayTypeSymbol) symbol = arrayTypeSymbol.ElementType;

        return symbol.ContainingNamespace
            .ToDisplayString(GlobalSymbolDisplayFormat)
            .StartsWith("global::System");
    }

    public static bool IsAutoField(IFieldSymbol? memberInfo)
    {
        return memberInfo?.Name.EndsWith("k__BackingField") ?? false;
    }

    public static string GetNewGlobalTypeFullName(M2VMGenerationInfo generationInfo, M2VMTypeInfo typeInfo)
    {
        return $"global::{generationInfo.NameSpace}.{typeInfo.Name.Replace("<", "_").Replace(">", "_")}SGVM";
    }

    public static bool IsDefaultIncluded(M2VMTypeMemberInfo m2VmTypeMemberInfo)
    {
        return m2VmTypeMemberInfo.Accessibility is Accessibility.Public
               && m2VmTypeMemberInfo is
               {
                   GetterAccessibility: Accessibility.Public,
                   SetterAccessibility: Accessibility.Public
               };
    }

    public static string GetTypeofTargetTypeFQType(ITypeSymbol typeSymbol, bool fQType = false)
    {
        return $"typeof({typeSymbol.ToDisplayString(fQType
            ? GlobalSymbolDisplayFormat
            : SymbolDisplayFormat.MinimallyQualifiedFormat).Replace(typeSymbol.IsValueType ? "" : "?", "")})";
    }

    public static string GetTypeofTargetTypeFQType(string typeName)
    {
        return $"typeof({typeName})";
    }

    public static IEnumerable<string> GetTypeofTargetTypeFQType(IEnumerable<ITypeSymbol> typeSymbols)
    {
        return typeSymbols.Select(typeSymbol => GetTypeofTargetTypeFQType(typeSymbol, true));
    }

    public static T? GetNamedArgument<T>(AttributeData attributeData, string name, T? fallback = default)
    {
        return TryGetNamedArgument(attributeData, name, out T? value) ? value : fallback;
    }

    public static bool TryGetNamedArgument<T>(AttributeData attributeData, string name, out T? value)
    {
        foreach (var properties in attributeData.NamedArguments.Where(properties => properties.Key == name))
        {
            value = (T?)properties.Value.Value;

            return true;
        }

        value = default;

        return false;
    }

    public static string GetFieldName(string propertyName)
    {
        return $"_{propertyName[0].ToString().ToLower()}{propertyName[1..]}";
    }
}

[JsonSerializable(typeof(GenerateMode))]
[JsonSerializable(typeof(M2VMConverterInfo))]
[JsonSerializable(typeof(M2VMGenerationInfo))]
[JsonSerializable(typeof(M2VMPropertyOrFieldOperationInfo))]
[JsonSerializable(typeof(M2VMReplaceGenerationInfo))]
[JsonSerializable(typeof(M2VMTypeInfo))]
[JsonSerializable(typeof(M2VMTypeMemberInfo))]
[JsonSerializable(typeof(M2VMViewModelInfo))]
[JsonSerializable(typeof(MetadataAndFQType))]
[JsonSerializable(typeof(PropertyBuildInfo))]
[JsonSerializable(typeof(PropertyOrFieldOperationKind))]
[JsonSerializable(typeof(TypeofInfo))]
[JsonSerializable(typeof(ViewModelBuildInfo))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class M2VMHelperJSC : JsonSerializerContext;