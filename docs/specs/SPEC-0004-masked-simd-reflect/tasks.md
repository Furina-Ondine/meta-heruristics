# SPEC-0004 实施任务

## 执行规则

- 只能实施 Approved Spec 与 Approved Plan 中已有的行为。
- 发现遗漏、数值冲突、重复概念或新的架构选择时停止，退回 Spec 或 Plan。
- 完成一个任务时同步完成其测试和必要文档。
- 不得以性能开关、共享缓冲区或放宽数值规则绕过 FR-005。
- 同一时间只能有一项任务处于 `InProgress`。

## T001：建立混合 lane 契约与旧实现性能基线

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`
- 依赖：无
- 影响区域：Core 测试、Repair 基准。
- 实施内容：扩展独立标量差分测试，覆盖同一 512/256/128 候选块中的正常、NaN、无穷、无界端点、端点和溢出 lane，以及所有指定长度和边界形状；在基准项目添加私有 `LegacyTensorReflect` 基线，完整复现提交 `21804bd` 的 Reflect 分派，供同机比较。
- 明确不做：不修改 Core Reflect 的生产实现、不改变 API 或算法、不以基准辅助类型作为运行时抽象。
- 完成条件：新测试可在旧实现上通过；基准可在四种边界形状和全部指定长度运行，且 MemoryDiagnoser 可见分配。
- 验证命令：`dotnet test Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter *RepairBenchmarks* --job Dry`。
- 验证结果：2026-08-28 通过；`CandidateRepairsTensorTests` 4 项测试通过，BenchmarkDotNet DryRun 成功发现 `LegacyTensorReflect` 基线。一次过宽筛选启动无关矩阵后已终止其专属进程，未将结果作为性能证据。

## T002：实现自适应掩码 Reflect 内核

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`NFR-001`、`NFR-002`
- 依赖：`T001`
- 影响区域：Core 私有 `CandidateRepairs.ReflectCandidateRepair`、Core 测试。
- 实施内容：删除整段 `CanUseTensorPath` 与 Tensor Reflect 管线；实现私有 512/256/128 位块内核、原值/Clamp/Reflect 候选、掩码优先级和逐级宽度循环；保留既有标量参考作为无硬件 SIMD、最终单元素尾部及逐 lane 精确修补。
- 明确不做：不改 Clamp、RandomReset、DoNothing、公开接口、项目依赖、状态所有权或 Algorithms。
- 完成条件：T001 差分测试通过；不存在整段安全扫描或全段回退；正常 Repair 无调用级托管分配。
- 验证命令：`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test Metaheuristics.NET.slnx -c Release --no-build`。
- 验证结果：2026-08-28 通过；Release build 0 warning/0 error，完整测试 105 项通过。大有限偏移差分测试发现直接向量余数会失真，已由超过 2 个 period 的逐 lane 标量修补处理，不恢复整段回退。

## T003：执行性能门槛与完成验证

- 状态：`Completed`
- 覆盖需求：`FR-005`、`NFR-001`、`NFR-002`
- 依赖：`T002`
- 影响区域：Benchmarks、Specs、architecture overview。
- 实施内容：运行 `LegacyTensorReflect` 与新内核的同机 BenchmarkDotNet 对比；确认标量/标量与向量/向量的 32/128 四个主要比较均改善。满足时执行 Release restore/build/test、格式、残留、DocFX 并填写 Verification、更新架构概览；不满足时删除候选内核并恢复 `21804bd` 的生产实现，记录未达门槛的证据。
- 明确不做：不把单机数据宣传为所有硬件的性能结论；不在门槛失败时保留较慢实现或开关。
- 完成条件：所有要求具有验证证据；Spec/Tasks 状态与实际结果一致。
- 验证命令：`dotnet restore Metaheuristics.NET.slnx`; `dotnet build Metaheuristics.NET.slnx -c Release --no-restore`; `dotnet test Metaheuristics.NET.slnx -c Release --no-build`; `dotnet tool run docfx docfx.json`; `dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter *RepairBenchmarks*`。
- 验证结果：2026-08-28 通过；80 项 Reflect ShortRun 对比中四个主要门槛均改善，完整 Release restore/build/test、DocFX、格式和残留检查均通过。详情见 `verification.md`。
