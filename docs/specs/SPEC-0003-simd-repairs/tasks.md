# SPEC-0003 实施任务

## 执行规则

- 只能实施 Approved Spec 和 Approved Plan 中已有的行为。
- 发现遗漏、冲突、重复概念或新架构选择时停止，退回 Spec/Plan。
- 完成一项需求时同时完成对应测试和必要文档。
- 不得因为现有公共 API 而保留 Plan 已要求删除的抽象。
- 一个时间只能有一项任务处于 `InProgress`。

## T001：建立 Tensor 依赖和 Repair 数值测试

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`NFR-001`
- 依赖：无
- 影响区域：集中包版本管理、Core、Core 测试。
- 实施内容：将 `System.Numerics.Tensors` 10.0.11 仅引用到 Core；创建独立标量参考和 Clamp/Reflect 四种边界形状、指定对齐/尾部长度、特殊值、溢出及 1 ULP 兼容测试；实现 Clamp 和 Reflect 的批准分派与回退。
- 明确不做：不改 RandomReset、DoNothing、公开 API 或 Algorithms。
- 完成条件：Release build 通过，新增与现有 Core 测试均通过，正常 Repair 路径无调用级临时数组。
- 验证命令：`dotnet test Metaheuristics.NET.slnx -c Release`。
- 验证结果：2026-08-28 通过；Release build 和 103 项测试通过。

## T002：建立 Repair 与 Bat 性能证据

- 状态：`Completed`
- 覆盖需求：`FR-005`、`NFR-002`
- 依赖：`T001`
- 影响区域：Benchmarks、规格验证报告。
- 实施内容：添加 Repair 标量参考/实际实现微基准，以及 Clamp/Reflect 的 Bat 端到端组合基准；以 MemoryDiagnoser 记录结果。
- 明确不做：不宣称所有算法、硬件或维度均加速，不修改现有 Bat 工作区复用基准的含义。
- 完成条件：Release BenchmarkDotNet 可运行并输出所需维度、形状和分配信息。
- 验证命令：`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter *Repair*`。
- 验证结果：2026-08-28 通过；Repair 全参数矩阵 DryRun 及 Bat ShortRun 均生成结果与分配数据。

## T003：完成验证和规格追踪

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`、`NFR-002`
- 依赖：`T002`
- 影响区域：Specs、架构概览（若实现状态需要报告）。
- 实施内容：执行 restore、Release build、完整测试、格式/残留搜索与基准；填写 verification.md，复核 API/职责不变，并将 Spec/Tasks 状态推进到完成。
- 明确不做：不在无数据时写入性能倍数或扩大范围。
- 完成条件：所有需求具有通过/失败证据，无未解决问题。
- 验证命令：`dotnet restore Metaheuristics.NET.slnx; dotnet build Metaheuristics.NET.slnx -c Release --no-restore; dotnet test Metaheuristics.NET.slnx -c Release --no-build`。
- 验证结果：2026-08-28 完成；restore、Release build、测试、DocFX、残留搜索与基准均已执行，格式检查的无关既有 CRLF 问题记录于 Verification。
