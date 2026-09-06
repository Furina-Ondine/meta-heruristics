# SPEC-0009 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-09-06
- 最终结果：`Passed`

## 需求覆盖

实现、性能复测及独立收尾审查已完成；下表关联实现与已通过的验证。

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | RandomSource；Context | RandomSourceTests.PublicShapeIsClosed；T005 外部消费者验证 | spec.md / plan.md | Passed |
| FR-002 | RandomSource | KnownSeedsMatchXoshiroReferenceOutputs；LongSequenceMatchesIndependentReference | spec.md / plan.md | Passed |
| FR-003 | RandomSource 八个入口 | RandomSourceTests：整数参考、半开范围、极值、无效输入、空目标 | spec.md / plan.md | Passed |
| FR-004 | RandomSource | ExactCallSequenceRepeatsDeterministically | spec.md / plan.md | Passed |
| FR-005 | StandardNormal | StandardNormalTests：null/empty、复现、百万样本统计、零分配 | spec.md / plan.md | Passed |
| FR-006 | Core Execution；Experiments | OptimizationRunnerTests；ExperimentRunnerTests，含 ulong 回绕 | spec.md / plan.md | Passed |
| FR-007 | Bat/Pso/Cuckoo；CandidateRepairs | 四算法测试；Bat/PSO 相邻及相等端点；Repair 测试 | spec.md / plan.md | Passed |
| FR-008 | Algorithms | 生产差异审查：未新增批量缓冲、SIMD 或随机请求重排 | spec.md / plan.md | Passed |
| NFR-001 | Context；ExperimentRunner | ExperimentRunnerTests；四算法复用与隔离测试 | spec.md / plan.md | Passed |
| NFR-002 | RandomSource；StandardNormal | 独立参考差分、整数映射及百万样本统计 | spec.md / plan.md | Passed |
| NFR-003 | 随机与端到端基准 | 下述 BDN 报告：四类 Fill、四个端到端点及零分配 | spec.md / plan.md | Passed |
| NFR-004 | RandomSource | 成员比较 JIT 证据及唯一转换源码审查 | spec.md / plan.md | Passed |
| NFR-005 | 公共 API 与项目引用 | PublicShapeIsClosed；生产残留及依赖审查 | spec.md / plan.md | Passed |

## 性能报告（仅性能类修改）

后续按用户要求补测直接更新字段的成员 `NextRaw()`，完整结果与 JIT 证据见[成员方案比较](./evidence/t006-luna-member-20260906/README.md)。该方案也通过原门槛，标量与当前方案接近，Bat 32/128 为 `251.1/1344.5 us`，Cuckoo 为 `57.08/171.01 us`；但四类 Fill 的八个主要点耗时为当前方案的 `2.77–3.27` 倍。最终保留共享静态转换及标量装载/写回函数。FullOpts 诊断中两种标量转换均内联，机器码均为 76 bytes；批量差异是成员版在循环内读写字段，而最终版在循环外装载和写回状态。这不是额外函数调用本身带来的加速；诊断关闭了分层编译，不冒充默认 BDN 执行时的完整 JIT 因果证明。

Terra 历史方案的四类 Fill 在长度 32/128 均通过原门槛，采样分配为 0 B；这些历史证据继续保留，但最终实现采用本轮统一状态转换方案。最终 wrapper/ref 方案在相同机器、Runtime、BDN 0.15.8、DefaultJob 和 MemoryDiagnoser 下完成全部 32/128 维标量、Fill 与 seeded `System.Random` 对照，原始证据位于 [基准报告](./evidence/t006-luna-unified/random-source-wrapper-32-128/README.md)。四类 Fill（ulong/unit double/有界 double/有界 int）均值为 32D `23.82/24.38/27.59/25.57 ns`、128D `92.20/95.58/111.60/100.60 ns`，对应 `System.Random` 标量为 32D `921.24/180.99/183.12/170.71 ns`、128D `3679.44/661.82/671.23/683.00 ns`；主要点均更快，全部采样调用 `0 B`。同一报告的最终标量均值为 32D `78.41/74.89/79.86/74.92 ns`、128D `314.09/299.99/320.34/303.66 ns`，也均为 `0 B`。直接字段 ref 候选的对照报告位于 [基准报告](./evidence/t006-luna-unified/random-source-32-128/README.md)；它在标量和端到端路径不如 wrapper/ref，因此未保留。

端到端最终结果：wrapper/ref 方案的 Bat 32/128 为 `304.7/1386.8 us`，Cuckoo 为 `56.85/169.14 us`，原始报告位于 [基准报告](./evidence/t006-luna-unified/endpoint-wrapper-32-128/README.md)。相对 [T001 生产基线](./evidence/t001-baseline/Anastasya.Metaheuristics.Benchmarks.BatRandomMigrationBenchmarks-report-github.md) Bat `319.6/2526.4 us`、Cuckoo `58.02/166.28 us`，基线耗时/候选耗时分别为 Bat `1.049x/1.822x`、Cuckoo `1.021x/0.983x`，四点均满足候选不超过基线 `1.05x`。端到端分配为 Bat `223.74/800.52 KB`、Cuckoo `44/140.75 KB`。环境：BDN 0.15.8、.NET 10.0.11、Apple M4 Arm64、macOS 15.7.4；不得外推为跨环境承诺。

标准正态正式报告位于 [基准报告](./evidence/t006-luna-unified/standard-normal-wrapper-32-128/README.md)；12 个 32/128D Sample/Fill、独立 Box–Muller 和 Cuckoo spare 对照均为 `0 B`。生产 Sample 为 `11.859/11.867 ns`，Fill 为 `246.751/977.268 ns`；未引入 Ziggurat 候选，不适用其额外两轮准入门槛。

证据目录：[基准报告](./evidence/t006-rerun-luna/baseline/endpoint-default/README.md)、[基准报告](./evidence/t006-terra/bat-endpoint-default-final-53bit/README.md)、[基准报告](./evidence/t006-terra/endpoint-default-final-53bit/README.md)。T001 和失败候选历史结果继续保留。性能数值通过不代替方案一致性与工程验收。

## 删除与残留检查

RandomSource/ulong 生产迁移已落地；生产源码未发现 System.Random、int seed、IRandomSource、RandomSourceFactory 或 Random.Shared 残留。算法差异仅涉及批准的随机入口迁移，项目运行时依赖方向保持不变。历史和基准中的 System.Random 对照允许保留。

## 架构一致性

已解决。`RandomSource.cs` 现在只有一个 `NextRaw(ref,ref,ref,ref)` 状态转换体和一个 `NextBounded(range,ref,ref,ref,ref)` 有界映射体；`NextRawFromFields` 与 `NextBoundedFromFields` 仅负责标量状态的局部装载、共享 helper 调用和写回，不含重复的 Xoshiro 或乘高位算法。Fill 与标量共同调用同一实现，公共 API、随机请求顺序和 53 位 double 语义未改变。源码审查确认随机路径无接口、虚调用、delegate 或 factory 分派。

## 工程验证

- Restore：Passed；`dotnet restore --disable-parallel -p:BuildInParallel=false -m:1 -nr:false`。
- Release Build：Passed；`dotnet build -c Release --no-restore -m:1 -nr:false -p:UseSharedCompilation=false`，0 警告、0 错误。
- Tests：Passed；`dotnet test -c Release --no-build`，完整 182 项通过、0 失败、0 跳过。
- Format：Passed；修复 PsoBenchmarks.cs 和 RandomSourceTests.cs 导入顺序后，`dotnet format --verify-no-changes --no-restore` 返回 0。新增随机测试 XML 注释已改为中文。
- 文档链接与规格检查：Passed；`pwsh -NoProfile -File eng/verify-documentation.ps1`。
- DocFX：Passed；`dotnet docfx docfx.json --warningsAsErrors` 返回 0，0 警告、0 错误。保留 Roslyn 5.9.0；按用户要求，DocFX metadata 设置 `BuildingDocfx=true`，Algorithms 仅在此文档提取模式下跳过私有 SIMD 生成器项目引用，正常 Release 构建仍加载生成器并通过。
- Benchmark 或分配分析：Passed；wrapper/ref 方案的原始 BDN CSV/HTML/Markdown 位于 [基准报告](./evidence/t006-luna-unified/random-source-wrapper-32-128/README.md)、[基准报告](./evidence/t006-luna-unified/standard-normal-wrapper-32-128/README.md) 和 [基准报告](./evidence/t006-luna-unified/endpoint-wrapper-32-128/README.md)。直接字段 ref 候选报告位于 [基准报告](./evidence/t006-luna-unified/random-source-32-128/README.md) 和 [基准报告](./evidence/t006-luna-unified/endpoint-32-128/README.md)，用于方案选择。

## 未解决问题

无阻塞项。性能证据仅适用于报告中的机器、运行时和输入；固定种子的搜索轨迹随本次随机源迁移改变，不承诺与旧版本逐值一致。
