# WCKYWCKF.Model2ViewModel

## 简介

这是一个源生成器，旨在帮助你在MVVM设计模式中，将复杂且庞大的`Model`配置类或POCO类型生成一个`ViewModel`版本并提供相应的方法来对它们进行互转换。

使用它，你可以节省大量时间，不再需要通过复制粘贴的方式手动创建`ViewModel`和它们之间的赋值，从而有更多时间专注于代码的设计与优化。

此项目绝大多树功能都是为了转换POCO类型或是结构较为简单的类而设计的，当你需要进行转换的类型具有复杂的结构时你需要自行处理而不是将所有工作都丢给此项目。

此项目处于预览阶段，可能还存在一些细微的BUG，但它已能够完成绝大多数工作。

欢迎你的PR，期待你的Star（当帮助到你时）。

项目使用MIT协议。

## 提供的功能
* 一个Nuget包：WCKYWCKF.Model2ViewModel
* 一个可视化编辑器

## 关于源生成器
### 功能
请通过Releases提供的Editor进行查看与使用。文档将在第一个正式版本发布后补齐。

## 关于UI编辑器
旨在增强对复杂Model进行转换配置的体验与能力。

编辑器主要使用如下项目构建：
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia)
- [Ursa.Avalonia](https://github.com/irihitech/Ursa.Avalonia)
- [Mantra](https://www.bilibili.com/video/BV15pfKYbEEQ/?share_source=copy_web&vd_source=d1867713073c379510aa0c911d19663d) [该项目需要订阅]
- [ReactiveUI](https://github.com/reactiveui/ReactiveUI)
- [DynamicData](https://github.com/reactivemarbles/DynamicData)