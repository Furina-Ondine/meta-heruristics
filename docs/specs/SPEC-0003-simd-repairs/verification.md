# SPEC-0003 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-08-28
- 最终结果：`Passed`

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `Directory.Packages.props`、`Metaheuristics.Core.csproj` | Release restore/build | ADR-0013、architecture overview | Passed |
| FR-002 | `CandidateRepairs.ClampCandidateRepair` | `CandidateRepairsTensorTests.ClampMatchesTheScalarReferenceForAllBoundaryShapesAndLengths` | 既有 CandidateRepairs XML | Passed |
| FR-003 | `CandidateRepairs.ReflectCandidateRepair` | `CandidateRepairsTensorTests.ReflectMatchesTheScalarReferenceWithinOneUlpForFiniteInputs`、`ReflectUsesTheScalarSemanticsForSpecialValuesEndpointsAndOverflowingRanges` | architecture overview | Passed |
| FR-004 | 未修改 `RandomResetCandidateRepair` 与 `DoNothingCandidateRepair` | 既有 `ContinuousProblemTests.BuiltInRepairsFollowTheirDocumentedSemantics` | Spec 非目标 | Passed |
| FR-005 | `RepairBenchmarks`、`BatRepairBenchmarks` | Repair 全形状/长度 DryRun，Bat ShortRun + MemoryDiagnoser | 本报告 | Passed |
| NFR-001 | Core Repair 实现与差分测试 | Release build；103 项完整测试 | Spec 数值兼容性 | Passed |
| NFR-002 | 无调用级数组分配的 Tensor 路径；基准 | MemoryDiagnoser；实现审查 | Spec 性能边界 | Passed |

## 性能证据

运行环境：Windows 11、AMD Ryzen 7 9800X3D、.NET 10.0.11、x64 RyuJIT x86-64-v4；BenchmarkDotNet 0.15.8。原始报告位于本机忽略的 `BenchmarkDotNet.Artifacts/results/`。

- Repair DryRun 覆盖标量/向量四种端点形状及长度 2、7、8、31、32、33、127、128、129、1024，确认所有参数组合可发现、构建和运行，并由 MemoryDiagnoser 报告无 Repair 调用级托管分配。
- Bat ShortRun 使用复用工作区、种群 64、5 次迭代。Tensor Clamp 相对标量参考在维度 32 为 0.92x（约快 8%）、维度 128 为 0.93x（约快 7%）；Tensor Reflect 分别为 0.98x（约快 2%）与 0.96x（约快 4%）。两条路径的分配均为 456 B/457 B，来自 Runner，不是 Repair 临时缓冲。
- 这些数据只支持该运行时、机器和 Bat 配置下的结果；不外推为所有 Optimizer、硬件、维度或边界形状的性能承诺。

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| Clamp 标量热循环 | 由 TensorPrimitives 四形状分派取代 | `CandidateRepairs` 不再含 Clamp Repair 的逐元素循环；标量 `Clamp` 函数仍被 Reflect/RandomReset 回退正确使用。 | Passed |
| 安全有限 Reflect 标量热循环 | Tensor 管线取代，保留特殊值参考路径 | `ReflectCandidateRepair` 先执行安全预扫描；不安全整次调用仍进入原始标量 `Reflect`。 | Passed |
| 公共 SIMD/VectorOps 抽象 | 不新增 | 定向 `rg` 未发现本变更新增的 `SimdRepair`、`VectorOps` 或 Algorithms Tensor 引用。历史文档中的 `VectorOps` 表述不属于运行时代码。 | Passed |

## 架构一致性

- 策略职责是否保持独立：是。Core 继续唯一拥有边界、Repair 与 Tensor 依赖；Algorithms 未改变。
- 是否新增重复验证：否。端点和形状仍在构造时验证；调用时只保留既有长度检查及 Reflect 的内部安全分派。
- 是否存在无消费者抽象：否。width/period 是 Reflect 私有创建时缓存；没有新公开类型或开关。
- 职责是否位于批准的项目层：是。仅 Core 直接引用 `System.Numerics.Tensors`。
- 是否出现未经批准的兼容层：否。

## 工程验证

- Restore：Passed — `dotnet restore Metaheuristics.NET.slnx`。
- Release Build：Passed — `dotnet build Metaheuristics.NET.slnx -c Release --no-restore`，0 warning / 0 error。
- Tests：Passed — `dotnet test Metaheuristics.NET.slnx -c Release --no-build`，103 passed。
- Format：Passed（本次文件）— `CandidateRepairsTensorTests.cs` 已由 `dotnet format` 格式化并以 `--verify-no-changes` 验证。全仓 `dotnet format ... --verify-no-changes` 未通过，原因是本变更以外既有 `CuckooOptimizerOptions.cs`、`ExperimentRunner.cs` 与多个既有测试文件的 CRLF 行尾；依照保护用户修改的规则未改写它们。
- 文档链接与规格检查：Passed — `git diff --check` 通过；Spec/Plan/Tasks/Verification 链接齐全。
- DocFX：Passed — `dotnet tool run docfx docfx.json`，0 warning / 0 error。
- Benchmark 或分配分析：Passed — Repair DryRun 与 Bat ShortRun 报告如上。

## 未解决问题

- 无。本变更范围内的实现、测试、文档和性能证据均已完成。
