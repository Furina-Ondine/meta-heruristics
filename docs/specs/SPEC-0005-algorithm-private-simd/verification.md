# SPEC-0005 验证报告

## 元数据

- Spec：[spec.md](./spec.md)
- Plan：[plan.md](./plan.md)
- Tasks：[tasks.md](./tasks.md)
- 验证日期：2026-08-29
- 最终结果：`Passed`
- Spec 状态：`Implemented`。T006 固定宽度级联的 PSO 测试、内核和端到端测量已完成；T007 Firefly 重试也已通过测试、内核、端到端和边界门槛。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `PsoOptimizer`、`VectorOps`、Algorithms 项目引用 | `TensorPrimitivesPsoTests`、`VectorOpsTests`、`PsoOptimizerTests`；当前 144/144 通过 | `spec.md`、`plan.md`、本报告 | Passed |
| FR-002 | Firefly 的候选距离和逐维更新 | `FireflyOptimizerTests`、`VectorOpsTests`；Firefly 内核 33/33、端到端 6/6 | `spec.md`、`plan.md`、本报告 | Passed |
| FR-003 | `PsoBenchmarks`、`FireflyBenchmarks` | PSO 内核 33/33、端到端 4/4；Firefly 内核 33/33、端到端 6/6 | `plan.md`、本报告 | Passed |
| FR-004 | 不修改 `BatOptimizer`、`CuckooOptimizer` | 源码热点审查 | `spec.md`、`plan.md`、本报告 | Passed |
| NFR-001 | PSO/Firefly 保持原随机抽取、Repair 与状态机 | 既有契约与长度回退测试 144/144 通过；Firefly 固定 seed 复现、严格 attractor 顺序和每次 Repair 测试通过 | `spec.md`、`plan.md`、本报告 | Passed |
| NFR-002 | 目标数组和 `VectorOps` 复用；没有新增热路径数组 | PSO 与 Firefly 的内核均为 0 B；Firefly 两个端到端维度的候选分配不高于标量基线 | `spec.md`、`plan.md`、本报告 | Passed |
| NFR-003 | `VectorOps` 为 Algorithms 内部纯 Span 算术；仅使用通用固定宽度向量 | 项目引用审查和 ISA 专属 API 残留搜索通过 | `spec.md`、`plan.md`、本报告 | Passed |

## 工程验证

| 命令 | 结果 |
| --- | --- |
| `dotnet restore Metaheuristics.NET.slnx` | 通过；使用集中版本管理的依赖完成还原。 |
| `dotnet build Metaheuristics.NET.slnx -c Release --no-restore` | 通过；0 警告、0 错误。 |
| `dotnet test Metaheuristics.NET.slnx -c Release --no-restore` | 通过；144/144，0 失败、0 跳过（已覆盖 Firefly 固定宽度级联诊断长度）。 |
| `pwsh -NoProfile -File eng/test-documentation-verifier.ps1` | 通过；文档验证器自测通过。 |
| `pwsh -NoProfile -File eng/verify-documentation.ps1` | 通过；所有受检文档和 fixture 通过。 |
| `git diff --check` | 通过；无空白错误。 |
| `dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*PsoCandidateUpdateBenchmarks*' --job Medium` | 通过；33/33，24 分 56 秒。 |
| `dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*PsoAdvanceBenchmarks*' --job Medium` | 通过；4/4，2 分 39 秒。 |
| `dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*FireflyMoveBenchmarks*' --job Medium` | 通过；33/33，约 22 分 30 秒；产生内核 CSV/Markdown 报告。 |
| `dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*FireflyAdvanceBenchmarks*' --job Medium` | 通过；6/6，约 3 分 51 秒；产生端到端 CSV/Markdown 报告。 |

## PSO 阶段

以下旧数值是 T006 之前的历史基线，不可用于接受当前固定宽度级联实现。当前代码和补充诊断输入已写入，且已通过 144 项测试。

首次授权的候选更新内核命令在 BenchmarkDotNet 启动前的构建阶段退出：`PsoBenchmarks.cs` 的两个 `SimdConfigurations` 参数源触发 CA1822（可标记为 `static`）。已将它们改为静态参数源；该修复尚未重新构建或测量，因此没有新的机器配置、耗时、误差、分配或结果文件。

修复后重试时，BenchmarkDotNet 自动生成项目因无法访问 `https://api.nuget.org/v3/index.json` 获取漏洞审计数据而未完成还原；尽管外层命令返回 0，实际执行 benchmark 数为 0，摘要为 `NA`，不构成性能证据。网络权限申请受当前 Codex 用量限制未获准，故本轮停止。

网络恢复后的下一次重试发现 `Program.cs` 当前硬编码 `BenchmarkRunner.Run<PsoAdvanceBenchmarks>()`，命令行的候选内核过滤器因而被忽略。进程已立即中止：只有 32 维标量端到端路径完成，候选路径在实际迭代中中断；两者均不作为性能结论。项目作者随后手动恢复 `BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args)`；此后候选更新内核完整运行成功。

### 固定宽度级联候选更新内核（T006）

命令：`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*PsoCandidateUpdateBenchmarks*' --job Medium`。BenchmarkDotNet 0.15.8 在 Windows 11 25H2、AMD Ryzen 7 9800X3D、.NET 10.0.11、x64 RyuJIT `x86-64-v4` 上完成 33/33 项（约 24 分 56 秒）。运行时报告 `Vector512`/`Vector256`/`Vector128` 均可用，`double` lane 数分别为 8/4/2；三条候选路径分配均为 0 B。

| 维度 | 标量 | TensorPrimitives 组合 | VectorOps 级联 | 标量 / VectorOps |
| ---: | ---: | ---: | ---: | ---: |
| 2 | 3.960 ns | 19.288 ns | 8.725 ns | 0.45x |
| 7 | 11.923 ns | 29.364 ns | 12.319 ns | 0.97x |
| 8 | 13.782 ns | 65.901 ns | 12.800 ns | 1.08x |
| 15 | 25.759 ns | 39.536 ns | 16.243 ns | 1.59x |
| 16 | 27.460 ns | 36.731 ns | 16.268 ns | 1.69x |
| 31 | 52.951 ns | 51.701 ns | 21.193 ns | 2.50x |
| 32 | 55.457 ns | 45.281 ns | 20.119 ns | 2.78x |
| 33 | 56.257 ns | 57.838 ns | 18.820 ns | 3.03x |
| 127 | 218.394 ns | 84.777 ns | 37.063 ns | 5.89x |
| 128 | 220.237 ns | 87.505 ns | 37.015 ns | 5.95x |
| 129 | 220.966 ns | 80.889 ns | 35.338 ns | 6.25x |

15 维的 `VectorOps` 结果确认 512 位硬件下的 8 + 4 + 2 + 1 长度级联没有退回为大段标量尾部。原始导出（运行时生成，未纳入版本库）：`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.PsoCandidateUpdateBenchmarks-report.csv`、`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.PsoCandidateUpdateBenchmarks-report-github.md`。

### 固定宽度级联完整 `Advance`（T006）

命令：`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*PsoAdvanceBenchmarks*' --job Medium`。同一主机与运行时完成 4/4 项（约 2 分 39 秒）。

| 维度 | 标量 | VectorOps 级联 | 标量 / VectorOps | 标量分配 | VectorOps 分配 |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 32 | 62.02 μs | 41.81 μs | 1.48x | 456 B | 456 B |
| 128 | 539.04 μs | 107.85 μs | 5.00x | 456 B | 456 B |

固定宽度级联在 32、128 维的内核和端到端四项比较均快于同机标量基线，且没有新增托管分配，满足 PSO 的 FR-003 阶段门槛。原始导出（运行时生成，未纳入版本库）：`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.PsoAdvanceBenchmarks-report.csv`、`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.PsoAdvanceBenchmarks-report-github.md`。

Algorithms 直接引用集中管理的 `System.Numerics.Tensors` `10.0.11`。先测量了直接 `TensorPrimitives` 组合；32 维完整生命周期为 62.603 us，对照标量 61.671 us，未能通过，因此没有将该组合作为生产路径。随后只为实际 PSO 速度融合公式使用无状态 `internal VectorOps.ComputePsoVelocity`，Clamp 和位置更新仍调用 `TensorPrimitives.Clamp` 与 `TensorPrimitives.Add`。

BenchmarkDotNet `0.15.8`、Windows 11 `10.0.26200.9168`、.NET SDK `10.0.400`、.NET Runtime `10.0.11`、x64 RyuJIT `x86-64-v4`；种群为 64，完整生命周期执行 10 次迭代，使用既有 Clamp Repair 与 Sphere 目标。`加速`为标量均值除以候选均值。所有 PSO 内核路径分配 0 B，完整生命周期标量与候选均为 456 B。

| 负载 | 维度 | 标量 | 生产候选 | 加速 |
| --- | ---: | ---: | ---: | ---: |
| 候选更新内核 | 32 | 54.205 ns | 23.755 ns | 2.28x |
| 候选更新内核 | 128 | 217.425 ns | 55.257 ns | 3.93x |
| 完整 `Advance` 生命周期 | 32 | 63.852 us | 46.225 us | 1.38x |
| 完整 `Advance` 生命周期 | 128 | 541.394 us | 116.383 us | 4.65x |

T006 之前的四项主要比较均优于各自标量基线。当前实现改为 512→256→128→标量级联，并新增 15/16 维诊断输入；`PsoOptimizerTests.AdvancePreservesRandomDrawOrderAndComputesTheExpectedVectorUpdate`、`TensorPrimitivesPsoTests` 和 `VectorOpsTests` 已在最终 Release 测试中重新运行。

## Firefly 首次尝试（T004，历史记录）

PSO 通过后，首次按原方案测量了 Firefly 的直接 `TensorPrimitives` 组合和融合向量候选。内核在两个主要维度均有收益，但端到端 32 维不达标，因而依照当时的 FR-003/NFR-002 退出条件删除了 Firefly 候选生产路径。这是本次 T007 重试之前的历史证据。

| 候选 | 负载 | 维度 | 标量 | 候选 | 加速 |
| --- | --- | ---: | ---: | ---: | ---: |
| 直接 TensorPrimitives | 单次移动内核 | 32 | 41.512 ns | 40.515 ns | 1.02x |
| 直接 TensorPrimitives | 单次移动内核 | 128 | 145.066 ns | 72.974 ns | 1.99x |
| 融合向量原型 | 单次移动内核 | 32 | 41.512 ns | 29.677 ns | 1.40x |
| 融合向量原型 | 单次移动内核 | 128 | 145.066 ns | 59.301 ns | 2.45x |
| 直接 TensorPrimitives | 完整 `Advance` 生命周期 | 32 | 4.322 ms | 4.372 ms | 0.99x |
| 直接 TensorPrimitives | 完整 `Advance` 生命周期 | 128 | 16.000 ms | 14.380 ms | 1.11x |
| 融合向量原型 | 完整 `Advance` 生命周期 | 32 | 4.290 ms | 4.515 ms | 0.95x |
| 融合向量原型 | 完整 `Advance` 生命周期 | 128 | 17.515 ms | 15.908 ms | 1.10x |

两个端到端候选与各自标量的 `Allocated` 均相同（32 维 20.45 KB，128 维约 20.45 KB），但 32 维耗时失败已经足以否决该次尝试。

## Firefly 重试（T007，已完成）

本次按 ADR-0016 的通用边界重新实现 Firefly 候选。生产路径使用 Algorithms 私有 `VectorOps`：距离平方采用 512→256→128→标量归约，位置更新采用相同级联；随机步长仍先按原 attractor/dimension 顺序写入 Optimizer 私有工作区，更新完成后立即调用既有 Repair。基准夹具同时提供标量、优化后的 `TensorPrimitives`（`Subtract`、`Dot`、`MultiplyAdd`、`Add`）和 VectorOps 三条完整生命周期路径。

### Firefly 固定宽度级联候选更新内核

命令：`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*FireflyMoveBenchmarks*' --job Medium`。BenchmarkDotNet 0.15.8 在 Windows 11 25H2、AMD Ryzen 7 9800X3D、.NET 10.0.11、x64 RyuJIT `x86-64-v4` 上完成 33/33 项（约 22 分 30 秒）。运行时报告 `Vector512`/`Vector256`/`Vector128` 均可用，`double` lane 数分别为 8/4/2；三条内核路径分配均为 0 B。

下表的“标量 / VectorOps”是标量均值除以 VectorOps 均值；小维度只作为诊断，不作为生产准入要求。

| 维度 | 标量 | TensorPrimitives 组合 | VectorOps 级联 | 标量 / VectorOps |
| ---: | ---: | ---: | ---: | ---: |
| 2 | 13.81 ns | 20.40 ns | 15.57 ns | 0.89x |
| 7 | 15.98 ns | 28.85 ns | 17.13 ns | 0.93x |
| 8 | 16.60 ns | 28.69 ns | 18.77 ns | 0.88x |
| 15 | 25.91 ns | 40.72 ns | 19.58 ns | 1.32x |
| 16 | 25.91 ns | 34.90 ns | 15.42 ns | 1.68x |
| 31 | 43.01 ns | 42.39 ns | 16.86 ns | 2.55x |
| 32 | 43.90 ns | 40.66 ns | 16.07 ns | 2.73x |
| 33 | 43.02 ns | 44.03 ns | 17.26 ns | 2.49x |
| 127 | 148.46 ns | 60.93 ns | 31.11 ns | 4.77x |
| 128 | 145.10 ns | 69.04 ns | 28.93 ns | 5.02x |
| 129 | 148.03 ns | 65.48 ns | 35.71 ns | 4.15x |

目标维度 32、128 的 VectorOps 内核均快于标量基线；15、31、33、127、129 等非整倍数诊断也走完整固定宽度级联和标量尾部。原始导出（运行时生成，未纳入版本库）：`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.FireflyMoveBenchmarks-report.csv`、`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.FireflyMoveBenchmarks-report-github.md`。

### Firefly 固定宽度级联完整 `Advance`

命令：`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*FireflyAdvanceBenchmarks*' --job Medium`。同一主机与运行时完成 6/6 项（约 3 分 51 秒）；种群为 64，完整生命周期执行 10 次迭代，使用既有 Clamp Repair 与 Sphere 目标。下表的“标量 / 候选”是标量均值除以候选均值，`Allocated` 为每次 `Advance` 负载的托管分配。

| 维度 | 标量 | TensorPrimitives 组合 | VectorOps 级联 | 标量 / TensorPrimitives | 标量 / VectorOps | 标量分配 | VectorOps 分配 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 32 | 4.453 ms | 4.848 ms | 4.222 ms | 0.92x | 1.05x | 20.45 KB | 20.45 KB |
| 128 | 16.492 ms | 15.876 ms | 14.947 ms | 1.04x | 1.10x | 20.46 KB | 20.45 KB |

VectorOps 在 32、128 维的端到端比较均快于标量基线，且没有新增托管分配，满足 Firefly 的 FR-003/NFR-002 阶段门槛；因此保留 VectorOps 生产路径，不保留 TensorPrimitives 候选路径。原始导出（运行时生成，未纳入版本库）：`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.FireflyAdvanceBenchmarks-report.csv`、`BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.FireflyAdvanceBenchmarks-report-github.md`。

T007 的 Release 构建、144/144 测试、内核 33/33 和端到端 6/6 均完成。`FireflyOptimizerTests` 覆盖严格更优 attractor、逐 attractor/逐维随机调用顺序、每次 Repair、固定 seed 复现、并发实例隔离和工作区复用；`VectorOpsTests` 覆盖 2、7、8、15、16、31、32、33、127、128、129 长度、原位更新以及 NaN/Infinity/负零分类。VectorOps 的生产调用点保持在纯 Span 算术边界内，不承担随机、Repair、评估或比较职责。

## Bat / Cuckoo 诊断

- Bat：`GenerateCandidate` 每维都抽取频率随机值，并有由候选脉冲率决定的状态分支；分支两侧还写入 Position、Loudness 与 PulseRate，不能仅替换线性算术而保持随机/状态语义。未迁移。
- Cuckoo：`GenerateLevyCandidate` 每维执行高斯采样、`Math.Pow` 与额外随机 guidance；高斯采样本身依赖 `Log`、`Sqrt`、`Sin`、`Cos` 和缓存的 spare 值。遗弃候选虽有线性差向量，但其随机扰动和挑选流程仍为主导耦合。未迁移。

任何 Bat/Cuckoo 生产 SIMD 改动都需要新的 Spec，明确随机数、数值和端到端性能门槛。

## 边界审查

- `Metaheuristics.Algorithms.csproj` 仅新增无版本号的 `System.Numerics.Tensors` 直接引用；版本仍由 `Directory.Packages.props` 统一锁定为 `10.0.11`。
- `VectorOps` 是 `internal static`，当前含 PSO 与 Firefly 的无状态 Span 算术；没有随机、Repair、比较、验证、缓存或公开运行时分派。
- 已确认 `src/Metaheuristics.Algorithms` 只使用通用固定宽度向量；`rg` 未发现 ISA 专属 `System.Runtime.Intrinsics.X86` 或 `.Arm` API，且未向 Core 增加依赖或批量评估 API。
- 没有跨 OS、硬件架构、Runtime 或 JIT 设置的数值稳定性承诺；固定 seed 的同一目标环境测试是验收边界。

## 固定宽度级联验收结论

项目作者已选择通用固定宽度向量级联，Plan 的实施补充记录了该决定，消除了原先的 Spec/Plan 冲突。每一层只在当前剩余 Span 足够且硬件可用时执行；15 个 double 在 512 位环境的路径为 8 + 4 + 2 + 1。PSO 和 Firefly 的 15/16 维及其他诊断长度均已完成测试和基准测量；长度处理规则与性能准入均通过。
