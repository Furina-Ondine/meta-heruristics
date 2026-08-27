# SPEC-0001 实施任务

## 执行规则

- 只实施已批准 Spec 与 Plan 中的行为；发现新的公共表示或职责变化时停止并退回批准门。
- 一个时间只能有一项任务处于 `InProgress`，每项任务同时完成测试、注释和对应证据。
- 不保留有限值旧语义的兼容属性、包装类型或配置开关。

## T001: 固化长期数值决策

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`
- 依赖：无
- 影响区域：ADR、ENGINEERING、Spec 元数据。
- 实施内容：新增 ADR-0015，定义目标与约束扩展数值域、统计缺失语义及无兼容壳决策；同步工程契约和索引。
- 明确不做：不修改运行时代码。
- 完成条件：ADR 状态与索引一致，Spec 指向新 ADR，文档门禁通过。
- 验证命令：`pwsh ./eng/verify-documentation.ps1`
- 验证结果：ADR-0015、索引、ENGINEERING 与 Spec 链接已更新；文档门禁通过。

## T002: 实现 Core 特殊值契约

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`
- 依赖：`T001`
- 影响区域：Core 的 Evaluation、ConstraintEvaluation、ContinuousProblem、EvaluationComparer、StoppingConditions 与 Core 测试。
- 实施内容：先添加失败测试，再以常数次标量分类实现 NaN 拒绝、Infinity 接受/排序、约束汇总和目标阈值语义。
- 明确不做：不扫描候选位置，不在算法层重复验证，不修改 Repair。
- 完成条件：Core 需求矩阵全部通过，热路径没有新增维度相关循环或分配。
- 验证命令：`dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj --configuration Release -- --filter-namespace Anastasya.Metaheuristics.Tests.Core`
- 验证结果：新增 29 个特殊值契约用例；Core 命名空间共 49 项测试通过，Release Build 零警告零错误。

## T003: 实现 Experiment 可空统计

- 状态：`InProgress`
- 覆盖需求：`FR-005`、`NFR-001`
- 依赖：`T002`
- 影响区域：ExperimentStatistics、ExperimentRunnerTests 及仓库内 API 消费者。
- 实施内容：先添加 Infinity 组合和极端有限值失败测试，再迁移三个 nullable 属性并实现显式分类、缩放 Mean/StandardDeviation 与安全 Median。
- 明确不做：不新增 Infinity 计数、不过滤样本、不新增公共统计类型。
- 完成条件：任何统计字段不产生 NaN；有限常规结果不变；计算保持一次物化、一次排序、两次线性遍历。
- 验证命令：`dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj --configuration Release -- --filter-namespace Anastasya.Metaheuristics.Tests.Experiments`
- 验证结果：尚未执行。

## T004: 迁移示例与文档契约

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`
- 依赖：`T003`
- 影响区域：Example、XML 注释、User Guide、Developer Guide、架构概览、API Reference。
- 实施内容：显式输出 undefined 统计，删除陈旧有限性表述，记录用户与扩展实现者需要掌握的特殊值规则。
- 明确不做：不在 API Overview 复制成员参数和异常契约。
- 完成条件：示例编译，DocFX 零警告，手写文档职责无重复。
- 验证命令：`dotnet build Metaheuristics.NET.slnx --configuration Release`；`dotnet docfx docfx.json --warningsAsErrors`
- 验证结果：尚未执行。

## T005: 完成追踪与工程验证

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`
- 依赖：`T004`
- 影响区域：Verification、全仓库。
- 实施内容：执行残留搜索、完整构建测试、格式、文档验证和 DocFX；记录每项需求证据并推进 Spec 状态。
- 明确不做：不以 Verification 阶段引入新行为。
- 完成条件：所有需求有实现、测试和文档证据，工作区无意外变更，Spec 进入 Implemented。
- 验证命令：按 Verification 的完整命令集合执行。
- 验证结果：尚未执行。
