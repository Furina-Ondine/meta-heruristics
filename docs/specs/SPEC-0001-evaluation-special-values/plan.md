# SPEC-0001 技术计划

## 元数据

- 状态：`Draft`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：—
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`
- 批准人：—
- 批准日期：—

## 当前状态

Spec 仍处于 `Clarifying`。在特殊值的统计、目标阈值和约束溢出语义获得批准前，不调查或选择实现方案，以免 Plan 反向替用户决定公共行为。

## 已知影响面

- Core：`ContinuousProblem`、`Evaluation`、`ConstraintEvaluation`、`EvaluationComparer`、`StoppingConditions`。
- Experiments：`ExperimentStatisticsCalculator` 与 `NumericStatistics`。
- Tests：Core 比较/评估/停止测试和 Experiment 统计测试。
- 文档：替代 ADR、XML 注释、API Reference、User/Developer Guide 中的特殊值契约。

## 下一门禁

待 Spec 批准后，补全当前调用链调查、至少两个实现方案、信任与验证设计、删除清单、连带影响矩阵以及逐需求验证设计，再请求 Plan 批准。该计划不得在此阶段授权 `FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005` 或 `NFR-001` 的代码改动。
