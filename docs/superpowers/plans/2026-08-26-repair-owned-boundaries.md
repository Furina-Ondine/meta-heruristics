# Repair 拥有候选位置边界实施计划

## 目标

落实 [ADR-0012](../../decisions/0012-repair-owned-candidate-boundaries.md) 与[设计规格](../specs/2026-08-26-repair-owned-boundaries-design.md)：算法不读取边界，所有 Position 在初始化和修改后交给 Repair，Core 不再验证位置。

## 任务 1：重构 Core 位置契约

- 让 `ContinuousProblem` 只保留维度和问题定义；删除 `Bounds`、`ValidatePosition` 与评估前位置检查。
- 将 `ICandidateInitializer` 改为位置与随机流参数。
- 将 `ICandidateRepair` 改为位置与随机流参数，并使 `OptimizationRunContext.Repair` 始终调用它。
- 实现持有边界副本的 Clamp、Reflect、RandomReset 与 DoNothing Repair；Problem 缺省使用 Clamp。

## 任务 2：迁移 Bat 算法

- 要求 `BatOptimizer` 接收位置初始化器。
- 删除 Bat 对 Problem 边界的读取和内置截断。
- 在 Position 初始化和候选更新后调用 Context Repair；保持速度、频率、响度、脉冲率、随机调用顺序与双缓冲布局不变。

## 任务 3：迁移调用点与测试

- 为示例和实验工厂提供显式位置初始化器。
- 更新现有测试的初始化器实现。
- 增加内置 Repair、无界/无穷/NaN、默认 Clamp、DoNothing、Repair 调用时机、边界未暴露和固定 seed 的测试。

## 任务 4：同步契约文档

- 更新工程规范、架构概览、Core/Algorithms API、用户手册和开发者架构手册。
- 更新公开 XML 文档，特别说明 DoNothing 风险、NaN 与无界维度的语义和位置责任。

## 验证

```powershell
dotnet restore Metaheuristics.NET.slnx --property:NuGetAudit=false
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet format Metaheuristics.NET.slnx --verify-no-changes --no-restore
git diff --check
```
