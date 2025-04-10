using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record M2VMGenerationInfo
{
    public M2VMGenerationInfo()
    {
    }

    [SetsRequiredMembers]
    public M2VMGenerationInfo(ITypeSymbol typeSymbol) : this(typeSymbol, CancellationToken.None)
    {
    }

    [SetsRequiredMembers]
    public M2VMGenerationInfo(ITypeSymbol typeSymbol, CancellationToken token)
    {
        NameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        ClassName = typeSymbol.Name;
        GenerateMode = getGenerateMode();
        var attributes = typeSymbol.GetAttributes();
        Queue<ITypeSymbol> sourceTypes = [];
        TypeToGenerateSource = attributes
            .Where(attributeData =>
            {
                token.ThrowIfCancellationRequested();
                return IsTargetAttribute(attributeData, M2VMHelper.M2VMGenerationInfoAttributeStr);
            })
            .Select(attributeData =>
            {
                token.ThrowIfCancellationRequested();
                var argument = M2VMHelper.GetNamedArgument<ITypeSymbol>(attributeData,
                    nameof(M2VMPropertyOrFieldOperationBase.TargetTypeFQType));
                if (argument is null) throw new NullReferenceException(nameof(argument));
                sourceTypes.Enqueue(argument);
                return new M2VMTypeInfo(argument);
            })
            .DistinctBy(x =>
            {
                token.ThrowIfCancellationRequested();
                return x.GlobalTypeFullName;
            })
            .ToList();
        Operations = attributes
            .Where(attributeData =>
            {
                token.ThrowIfCancellationRequested();
                return IsTargetAttribute(attributeData, M2VMHelper.M2VMPropertyOrFieldOperationInfoAttributeStr);
            })
            .Select(attributeData =>
            {
                token.ThrowIfCancellationRequested();
                return new M2VMPropertyOrFieldOperationInfo(attributeData);
            })
            .ToList();
        Replaces = attributes
            .Where(attributeData =>
            {
                token.ThrowIfCancellationRequested();
                return IsTargetAttribute(attributeData, M2VMHelper.M2VMReplaceGenerationInfoAttribute);
            })
            .Select(attributeData =>
            {
                token.ThrowIfCancellationRequested();
                return new M2VMReplaceGenerationInfo(attributeData);
            })
            .ToList();
        UseAutoField = attributes.FirstOrDefault(attributeData =>
        {
            token.ThrowIfCancellationRequested();
            return IsTargetAttribute(attributeData, M2VMHelper.M2VMUseAutoFieldAttributeStr);
        }) is not null;
        var attributeData = attributes.FirstOrDefault(attributeData =>
        {
            token.ThrowIfCancellationRequested();
            return IsTargetAttribute(attributeData, M2VMHelper.M2VMSaveGenerationInfoAttributeStr);
        });
        if (attributeData is not null)
            SaveFilePath = M2VMHelper.GetNamedArgument<string>(attributeData, nameof(SaveFilePath));
        //获得所有需要生成的类型
        TypeToGenerateAll = new List<M2VMTypeInfo>(TypeToGenerateSource);
        HashSet<string> includedInGeneratedType = new(TypeToGenerateSource.Select(x =>
        {
            token.ThrowIfCancellationRequested();
            return x.GlobalTypeFullName;
        }));
        while (sourceTypes.TryDequeue(out var result))
        {
            token.ThrowIfCancellationRequested();
            foreach (var symbol in result.GetMembers()
                         .Where(x =>
                         {
                             token.ThrowIfCancellationRequested();
                             return x is IFieldSymbol or IPropertySymbol;
                         })
                         .Select(x =>
                         {
                             token.ThrowIfCancellationRequested();
                             return (x as IFieldSymbol)?.Type ?? (x as IPropertySymbol)?.Type;
                         })
                         .Where(x =>
                         {
                             token.ThrowIfCancellationRequested();
                             return x != null && !IsSystemType(x) && !IsIncludedInGeneratedType(x);
                         }))
            {
                token.ThrowIfCancellationRequested();
                TypeToGenerateAll.Add(new M2VMTypeInfo(symbol!));
                sourceTypes.Enqueue(symbol!);
            }

            continue;

            bool IsIncludedInGeneratedType(ITypeSymbol symbol)
            {
                return !includedInGeneratedType.Add(symbol.ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat));
            }

            bool IsSystemType(ITypeSymbol symbol)
            {
                if (symbol is IArrayTypeSymbol arrayTypeSymbol)
                {
                    symbol = arrayTypeSymbol.ElementType;
                }

                return symbol.ContainingNamespace
                    .ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat)
                    .StartsWith("global::System");
            }
        }

        return;

        static bool IsTargetAttribute(AttributeData attributeData, string targetName)
        {
            return attributeData.AttributeClass?.ToDisplayString() == targetName;
        }


        GenerateMode getGenerateMode()
        {
            var typeName = typeSymbol.BaseType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            // typeName = typeName?[(typeName.LastIndexOf('.') + 1)..];
            return typeName switch
            {
                "ReactiveObject" => GenerateMode.RxUI,
                "ObservableObject" => GenerateMode.CommunityMvvm,
                _ => GenerateMode.Default
            };
        }
    }

    public required string ClassName { get; init; }
    public required string NameSpace { get; init; }
    public GenerateMode GenerateMode { get; init; }
    public required List<M2VMTypeInfo> TypeToGenerateSource { get; init; }
    public required List<M2VMTypeInfo> TypeToGenerateAll { get; init; }
    public required List<M2VMPropertyOrFieldOperationInfo> Operations { get; init; }
    public required List<M2VMReplaceGenerationInfo> Replaces { get; init; }
    public required bool UseAutoField { get; init; }
    [JsonIgnore] public string? SaveFilePath { get; init; }

    public List<ViewModelBuildInfo> CreateViewModelBuildInfos(out HashSet<M2VMConverterInfo> converterInfos)
    {
        HashSet<M2VMConverterInfo> m2VmConverterInfos = [];
        var viewModelBuildInfos = new List<ViewModelBuildInfo>();
        foreach (var m2VmTypeInfo in TypeToGenerateAll)
        {
            var properties = m2VmTypeInfo.MemberInfos
                .Where(ShouldIgnoreMember)
                .Select(x =>
                {
                    var shouldReplaceMemberType = ShouldReplaceMemberType(x, out var newGlobalFqType);
                    if (shouldReplaceMemberType is null)
                    {
                        m2VmConverterInfos.Add(new M2VMConverterInfo
                        {
                            InputType = x.GlobalTypeFullName,
                            OutputType = newGlobalFqType
                        });
                        m2VmConverterInfos.Add(new M2VMConverterInfo
                        {
                            InputType = newGlobalFqType,
                            OutputType = x.GlobalTypeFullName
                        });
                    }

                    return new PropertyBuildInfo
                    {
                        GlobalSourceFQType = x.GlobalTypeFullName,
                        GlobalPropertyFQType = newGlobalFqType,
                        PropertyName = x.MemberName,
                        IsNullable = ShouldPropertyTypeBeNullable(x),
                        IeTypeReplace = shouldReplaceMemberType is null
                    };
                })
                .ToList();
            viewModelBuildInfos.Add(new ViewModelBuildInfo
            {
                GlobalSourceFQType = m2VmTypeInfo.GlobalTypeFullName,
                ViewModelName = $"{m2VmTypeInfo.Name}SGVM",
                Namespace = m2VmTypeInfo.NameSpace,
                Properties = properties
            });
        }

        converterInfos = m2VmConverterInfos;
        return viewModelBuildInfos;
    }

    public M2VMPropertyOrFieldOperationInfo? GetPropertyOrFieldOperationInfo(M2VMTypeMemberInfo m2VmTypeMemberInfo)
    {
        return Operations.Find(x => x.TargetTypeFQType == m2VmTypeMemberInfo.GlobalTypeFullName
                                    && x.TargetMemberName == m2VmTypeMemberInfo.MemberName);
    }

    public M2VMReplaceGenerationInfo? GetReplaceGenerationInfo(M2VMTypeMemberInfo m2VmTypeMemberInfo)
    {
        return Replaces.Find(x => x.TargetTypeFQType == m2VmTypeMemberInfo.GlobalTypeFullName
                                  && x.TargetMemberName == m2VmTypeMemberInfo.MemberName);
    }

    public bool ShouldPropertyTypeBeNullable(M2VMTypeMemberInfo m2VmTypeMemberInfo)
    {
        var operationKind = GetPropertyOrFieldOperationInfo(m2VmTypeMemberInfo)?.TargetOperation;
        if (operationKind is not null)
        {
            if ((operationKind & PropertyOrFieldOperationKind.TypeIsNullable) > 0) return true;
            if ((operationKind & PropertyOrFieldOperationKind.TypeIsNotNullable) > 0) return false;
        }

        return !m2VmTypeMemberInfo.IsValue;
    }

    public bool ShouldIgnoreMember(M2VMTypeMemberInfo m2VmTypeMemberInfo)
    {
        //是默认应该包含的成员且没有被标记IgnoreProperty的返回true
        //是不默认包含的成员但被标记了IncludePropertyOrField的返回true
        var operationInfo = GetPropertyOrFieldOperationInfo(m2VmTypeMemberInfo);
        var isDefaultIncluded = M2VMHelper.IsDefaultIncluded(m2VmTypeMemberInfo);
        if (operationInfo is null) return isDefaultIncluded;
        if ((operationInfo.TargetOperation & PropertyOrFieldOperationKind.IgnoreProperty) > 0
            && isDefaultIncluded) return false;
        if ((operationInfo.TargetOperation & PropertyOrFieldOperationKind.IncludePropertyOrField) > 0
            && !isDefaultIncluded) return true;
        return isDefaultIncluded;
    }

    public bool? ShouldReplaceMemberType(M2VMTypeMemberInfo m2VmTypeMemberInfo, out string newGlobalFQType)
    {
        newGlobalFQType = m2VmTypeMemberInfo.GlobalTypeFullName;
        var operationInfo = GetPropertyOrFieldOperationInfo(m2VmTypeMemberInfo);
        var isSystemType = m2VmTypeMemberInfo.GlobalTypeFullName.StartsWith("global::System");

        if (operationInfo is not null
            && (operationInfo.TargetOperation & PropertyOrFieldOperationKind.DoNotReplaceTargetType) > 0)
            return false;
        var replaceGenerationInfo = GetReplaceGenerationInfo(m2VmTypeMemberInfo);
        if (replaceGenerationInfo is not null)
        {
            newGlobalFQType = replaceGenerationInfo.ReplaceFQType;
            return null;
        }

        if (isSystemType) return false;
        var info = TypeToGenerateAll.Find(x => x.GlobalTypeFullName == m2VmTypeMemberInfo.GlobalTypeFullName);
        if (info is not null) newGlobalFQType = M2VMHelper.GetNewGlobalTypeFullName(this, info);

        return true;
    }
}