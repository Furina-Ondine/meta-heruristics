# API 可读性与文档重整实施计划

## 目标

落实 [API 可读性与文档重整设计](../specs/2026-08-24-api-and-documentation-redesign-design.md)：按职责拆分实现，删除单行 `OptimizationRunner.Run`，并交付用户手册、开发者架构手册和完整公共 API 文档。

## 任务 1：整理 Core 执行和问题模型

- 将问题值对象、问题契约、候选策略、执行契约、停止、轨迹和结果拆为单一职责文件。
- 保留 `Evaluation`、`ConstraintEvaluation`、`OptimizationRunner.Execute` 的名称和语义。
- 删除只包装 `Execute` 的 `OptimizationRunner.Run` 与 `OptimizationResult`；让调用方在位置有效窗口内从 Optimizer 复制结果。
- 不改变位置验证、约束汇总、比较、停止检查或轨迹采样逻辑。

## 任务 2：整理算法和实验执行实现

- 抽取 Bat 的私有工作区状态，保持双缓冲数组、随机调用顺序和算法选择逻辑不变。
- 从 `ExperimentRunner` 抽取稳定计划、执行状态、结果构造和 Group 执行的内部职责。
- 保持固定 Worker、有界并发、seed 派生、异常后重建与部分取消的行为。

## 任务 3：迁移调用点与公共文档

- 将 Examples 和测试从 `OptimizationRunner.Run` 迁移至 `Execute` 与明确的位置复制。
- 删除过时 `OptimizationResult` 引用，并更新 API 参考。
- 完善公开类型的 XML 文档，强调所有权、线程安全、取消与异常后复用限制。

## 任务 4：新增人类阅读优先的文档

- 新增用户手册：最小单次执行、实验、扩展点、结果所有权、并发和可复现性。
- 新增开发者架构手册：项目职责、依赖、生命周期、状态、并发、失败恢复、扩展与验证。
- 调整 README 和现有 API 页的导航关系。

## 任务 5：验证

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet format Metaheuristics.NET.slnx --verify-no-changes
git diff --check
```

现有固定 seed、数值安全、并发隔离、失败恢复和调度行为测试必须继续通过。性能路径仅做结构移动，不更改算法循环、数组布局或调度策略。
