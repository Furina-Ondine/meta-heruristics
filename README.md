# Metaheuristics.NET

Metaheuristics.NET 是面向 .NET 的连续单目标元启发式优化库。你提供目标函数、候选初始化方式和停止条件，库用选定算法搜索更优的 `double` 向量，并可把同一类配置重复运行成可复现实验。

> [!IMPORTANT]
> 项目仍处于早期开发阶段，公共 API 尚未稳定，不建议用于生产环境。

## 当前能做什么

- 求连续 `double` 向量上的标量目标最小值或最大值；
- 使用约束、Clamp/Reflect/RandomReset Repair 和组合停止条件；
- 用内置蝙蝠、PSO、萤火虫或布谷鸟算法执行一次优化；
- 用多个 Case、重复运行、显式 seed 和有界并发执行批量实验；
- 通过强类型接口替换 Objective、Constraint、Initializer、Repair、Stopping Condition 或完整 Optimizer。

当前不支持多目标、二进制或排列表示、远程/集群/GPU 执行，也不提供运行时程序集扫描或字符串插件注册。

## 两个入口

```text
求解一次
Problem + Optimizer + RunOptions
    → OptimizationRunner.Execute
    → Summary + BestPosition

重复实验
ExperimentCase + ExperimentDefinition + ExecutionOptions
    → ExperimentRunner.RunAsync
    → ExperimentResult
```

第一次使用从[用户使用手册](docs/guides/user-guide.md)开始；寻找具体类型时使用 [API Overview](docs/api/overview.md)，并按其中的生成说明构建和打开由 XML 注释生成的 API Reference。

## 运行示例

需要与 [`global.json`](global.json) 兼容的 .NET SDK 10.0.400：

```powershell
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release
```

完整示例同时演示单次求解和两个实验 Case，源码见 [`Program.cs`](examples/Metaheuristics.Examples/Program.cs)。

## 开发与验证

```powershell
dotnet tool restore
dotnet restore Metaheuristics.NET.slnx --property:NuGetAudit=false
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
pwsh ./eng/verify-documentation.ps1
dotnet docfx docfx.json --warningsAsErrors
```

扩展策略或算法前阅读[开发者架构手册](docs/architecture/developer-guide.md)；持续工程规则见 [`ENGINEERING.md`](ENGINEERING.md)，当前实现见[架构概览](docs/architecture/overview.md)，决策理由见 [ADR](docs/decisions/README.md)，新功能变更流程见[功能规格](docs/specs/README.md)。

## 项目来源

本项目源自学位论文实验仓库 [task-schedule](https://github.com/Furina-Ondine/task-schedule)。旧仓库保留为历史实现和实验归档；其 [`fix` 分支](https://github.com/Furina-Ondine/task-schedule/tree/fix)包含部分问题的快速修复。
