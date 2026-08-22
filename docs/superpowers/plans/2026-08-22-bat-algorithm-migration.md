# 蝙蝠算法第一波迁移实施计划

## 范围

把旧论文仓库 `fix` 分支的蝙蝠算法迁移为当前 Core 的 `BatOptimizer`，完成配置、工作区、回归测试、示例和 API 文档；不迁移其他算法，不引入批量评估。

## 步骤

1. 审计旧 `master`、`fix` 和逻辑审计文档，确认 Bat 专属修复。
2. 固定 `BatOptimizerOptions`、问题边界和自定义初始化语义。
3. 实现双缓冲种群、独立历史最优、逐 run Reset 和单代 Advance。
4. 接入 Core 的随机流、评估、方向、约束、修复和取消。
5. 覆盖确定性、并发隔离、工作区复用、旧错误及 Sphere 回归。
6. 用正式 Bat 算法替换临时随机搜索示例，更新 API、架构、路线图和 ADR 索引。
7. 执行格式检查、Release restore/build/test、示例和差异检查。

## 验证

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet format Metaheuristics.NET.slnx --verify-no-changes --no-restore
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release --no-build
git diff --check
```
