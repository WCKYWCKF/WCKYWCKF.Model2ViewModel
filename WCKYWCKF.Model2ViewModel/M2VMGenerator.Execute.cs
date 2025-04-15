using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using WCKYWCKF.RxUI.Model2ViewModel.Model;
using static System.String;

namespace WCKYWCKF.RxUI.Model2ViewModel;

public partial class M2VMGenerator
{
    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<M2VMGenerationInfo> generateViewModelInfos)
    {
        foreach (var generateViewModelInfo in generateViewModelInfos.Where(x => !IsNullOrWhiteSpace(x.SaveFilePath)))
        {
#pragma warning disable RS1035
            using var fileStream = File.Create(generateViewModelInfo.SaveFilePath!);
#pragma warning restore RS1035
            JsonSerializer.Serialize(fileStream, generateViewModelInfo, M2VMHelperJSC.Default.M2VMGenerationInfo);
        }

        foreach (var generateViewModelInfo in generateViewModelInfos)
        {
            var viewModelBuildInfos = generateViewModelInfo.CreateViewModelBuildInfos(out var converterInfos);
            foreach (var viewModelBuildInfo in viewModelBuildInfos)
                context.AddSource(
                    $"{viewModelBuildInfo.Namespace}{viewModelBuildInfo.ViewModelName}.g.cs",
                    viewModelBuildInfo.CreateViewModelCode(
                        generateViewModelInfo.GenerateMode,
                        generateViewModelInfo.UseAutoField));
        }
    }
//         return;
//         var builtViewModels = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
//
//         foreach (var info in generateViewModelInfos)
//         {
//             var viewModelBuildInfos = CreateViewModelBuildInfo(builtViewModels, info);
//
//             foreach (var buildInfo in viewModelBuildInfos.Distinct(EqualityComparer<ViewModelBuildInfo>.Create(
//                          (viewModelBuildInfo, buildInfo) =>
//                              viewModelBuildInfo?.ViewModelName == buildInfo?.ViewModelName,
//                          x => x.ViewModelName.GetHashCode())))
//             {
//                 var fileStr = Format(GenerateViewModelsTemplate,
//                     $"""
//                      {info.GenerateMode switch {
//                          GenerateMode.RxUI => "using ReactiveUI;",
//                          GenerateMode.CommunityMvvm => "using CommunityToolkit.Mvvm.ComponentModel;",
//                          _ => """
//                               using System.ComponentModel;
//                               using System.Runtime.CompilerServices;
//                               """
//                      }}
//
//                      namespace {buildInfo.Namespace}
//                      """,
//                     GetClassName(buildInfo.ViewModelName),
//                     info.GenerateMode switch
//                     {
//                         GenerateMode.RxUI => "global::ReactiveUI.ReactiveObject",
//                         GenerateMode.CommunityMvvm => "global::CommunityToolkit.Mvvm.ComponentModel.ObservableObject",
//                         _ => "global::System.ComponentModel.INotifyPropertyChanged"
//                     },
//                     CreateProperties(info.GenerateMode switch
//                     {
//                         GenerateMode.RxUI => (x, _, _) =>
//                             $"{TabStr}{TabStr}{TabStr}this.RaiseAndSetIfChanged(ref {x}, value);",
//                         GenerateMode.CommunityMvvm => (x, y, z) =>
//                             $$$"""
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}if (!global::System.Collections.Generic.EqualityComparer<{{{y.Replace("?", "")}}}?>.Default.Equals({{{x}}}, value))
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}{
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    var t = {{{x}}};
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    On{{{z}}}Changing(value);
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    On{{{z}}}Changing(t, value);
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    OnPropertyChanging();
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    {{{x}}} = value;
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    On{{{z}}}Changed(value);
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    On{{{z}}}Changed(t, value);
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}    OnPropertyChanged();
//                                {{{TabStr}}}{{{TabStr}}}{{{TabStr}}}}
//                                """,
//                         _ => (x, _, _) => $"{TabStr}{TabStr}{TabStr}SetField(ref {x}, value);"
//                     }) + "\r\n" +
//                     info.GenerateMode switch
//                     {
//                         GenerateMode.RxUI => "",
//                         GenerateMode.CommunityMvvm => CreatePropertiesForCommunityMvvm(),
//                         _ => """
//                              public event PropertyChangedEventHandler? PropertyChanged;
//
//                              protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
//                              {
//                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//                              }
//
//                              protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
//                              {
//                                  if (EqualityComparer<T>.Default.Equals(field, value)) return false;
//                                  field = value;
//                                  OnPropertyChanged(propertyName);
//                                  return true;
//                              }
//                              """
//                     } + "\r\n" + GenerateConvertMethods(buildInfo));
//
//
//                 context.AddSource(buildInfo.ViewModelName.Replace("?", ""), fileStr);
//                 continue;
//
//                 string CreatePropertiesForCommunityMvvm()
//                 {
//                     StringBuilder builder = new();
//                     var first = buildInfo.Propertys.FirstOrDefault();
//                     if (first is null) return Empty;
//                     builder.Append(Create(first));
//                     builder.AppendLine("");
//                     foreach (var propertyBuildInfo in buildInfo.Propertys.Skip(1))
//                     {
//                         builder.AppendLine("");
//                         builder.AppendLine(Create(propertyBuildInfo));
//                     }
//
//                     return builder.ToString();
//
//                     string Create(PropertyBuildInfo propertyBuildInfo)
//                     {
//                         return $"""
//                                 {TabStr}partial void On{propertyBuildInfo.PropertyName}Changing({propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", "")}? value);
//                                 {TabStr}partial void On{propertyBuildInfo.PropertyName}Changing({propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", "")}? oldValue, {propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", "")}? newValue);
//                                 {TabStr}partial void On{propertyBuildInfo.PropertyName}Changed({propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", "")}? value);
//                                 {TabStr}partial void On{propertyBuildInfo.PropertyName}Changed({propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", "")}? oldValue, {propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", "")}? newValue);
//                                 """;
//                     }
//                 }
//
//                 string CreateProperties(Func<string, string, string, string> setMethodStr)
//                 {
//                     StringBuilder builder = new();
//                     var first = buildInfo.Propertys.FirstOrDefault();
//                     if (first is null) return Empty;
//                     builder.Append(CreateProperty(first));
//                     builder.AppendLine("");
//                     foreach (var propertyBuildInfo in buildInfo.Propertys.Skip(1))
//                     {
//                         builder.AppendLine("");
//                         builder.AppendLine(CreateProperty(propertyBuildInfo));
//                     }
//
//                     return builder.ToString();
//
//                     string CreateProperty(PropertyBuildInfo propertyBuildInfo)
//                     {
//                         var field =
//                             $"_{propertyBuildInfo.PropertyName[0].ToString().ToLower()}{propertyBuildInfo.PropertyName[1..]}";
//                         return Format(GenerateViewModelPropertyTemplate,
//                             propertyBuildInfo.GlobalSymbolDisplayFormat + (propertyBuildInfo.IsValue ? Empty : "?"),
//                             field + (propertyBuildInfo.IsList ? " = new()" : Empty),
//                             propertyBuildInfo.GlobalSymbolDisplayFormat + (propertyBuildInfo.IsValue ? Empty : "?"),
//                             propertyBuildInfo.PropertyName,
//                             field,
//                             setMethodStr(field, propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("?", ""),
//                                 propertyBuildInfo.PropertyName));
//                     }
//                 }
//             }
//         }
//     }
//
//     private static string GenerateConvertMethods(ViewModelBuildInfo viewModelBuildInfo)
//     {
//         string template = $"{TabStr}{TabStr}{TabStr}{{0}} = {{1}},";
//         StringBuilder builderForToModel = new();
//         var modelClassName = viewModelBuildInfo.ViewModelName.EndsWith("SGVM")
//             ? viewModelBuildInfo.ViewModelName[..^4]
//             : viewModelBuildInfo.ViewModelName;
//         foreach (var propertyBuildInfo in viewModelBuildInfo.Propertys)
//         {
//             builderForToModel.AppendLine(Format(template,
//                 propertyBuildInfo.PropertyName,
//                 GetRightCode()
//             ));
//             continue;
//
//             string GetRightCode()
//             {
//                 if (propertyBuildInfo.IsList)
//                 {
//                     return
//                         $"this?.{propertyBuildInfo.PropertyName} is not null ? new System.Collections.Generic.List<{propertyBuildInfo.SourceTypeFullName}>(this.{propertyBuildInfo.PropertyName}{(propertyBuildInfo.GlobalSymbolDisplayFormat.EndsWith("SGVM>") is false ? Empty : ".Select(x => x.ToModel())")}) : new ()";
//                 }
//
//                 return $"this.{(propertyBuildInfo.GlobalSymbolDisplayFormat.EndsWith("SGVM")
//                     ? $"{propertyBuildInfo.PropertyName}?.ToModel()"
//                     : propertyBuildInfo.PropertyName)}";
//             }
//         }
//
//         StringBuilder builderForToVM = new();
//         foreach (var propertyBuildInfo in viewModelBuildInfo.Propertys)
//         {
//             builderForToVM.AppendLine(Format(template,
//                 propertyBuildInfo.PropertyName,
//                 GetRightCode()));
//             continue;
//
//             string GetRightCode()
//             {
//                 if (propertyBuildInfo.IsList)
//                 {
//                     return
//                         $"model?.{propertyBuildInfo.PropertyName} is not null ? new System.Collections.ObjectModel.ObservableCollection<{propertyBuildInfo.TargetTypeFullName}>(model.{propertyBuildInfo.PropertyName}{(propertyBuildInfo.GlobalSymbolDisplayFormat.EndsWith("SGVM>") is false ? Empty : $".Select(x =>  {propertyBuildInfo.TargetTypeFullName}.CreateViewModel(x))")}) : new ()";
//                 }
//
//                 return propertyBuildInfo.GlobalSymbolDisplayFormat.EndsWith("SGVM")
//                     ? $"global::{propertyBuildInfo.GlobalSymbolDisplayFormat}.CreateViewModel(model?.{propertyBuildInfo.PropertyName})"
//                     : $"model?.{propertyBuildInfo.PropertyName} is null ? default : model.{propertyBuildInfo.PropertyName}";
//             }
//         }
//
//         StringBuilder builderForNonNullProperties = new();
//         foreach (var propertyBuildInfo in viewModelBuildInfo.Propertys
//                      .Where(x => x.IsList is false
//                                  && x.IsValue is false
//                                  && x.GlobalSymbolDisplayFormat is not "System.String"
//                                  && x.GlobalSymbolDisplayFormat.EndsWith("[]") is false))
//         {
//             builderForNonNullProperties.AppendLine(Format(template,
//                 propertyBuildInfo.PropertyName,
//                 GetRightCode()));
//             continue;
//
//             string GetRightCode()
//             {
//                 if (propertyBuildInfo.IsList)
//                 {
//                     return
//                         $"new {propertyBuildInfo.GlobalSymbolDisplayFormat.Replace("System.Collections.ObjectModel.ObservableCollection", "System.Collections.Generic.List")}()";
//                 }
//
//                 return propertyBuildInfo.GlobalSymbolDisplayFormat.EndsWith("SGVM")
//                     ? $"global::{propertyBuildInfo.GlobalSymbolDisplayFormat}.CreateNonNullPropertiesViewModel()"
//                     : $"new ()";
//             }
//         }
//
//         return Format(GenerateViewModelConvertTemplate,
//             viewModelBuildInfo.GlobalSourceFQType,
//             builderForToModel,
//             viewModelBuildInfo.ViewModelName,
//             viewModelBuildInfo.GlobalSourceFQType,
//             builderForToVM,
//             viewModelBuildInfo.ViewModelName,
//             builderForNonNullProperties);
//     }
}