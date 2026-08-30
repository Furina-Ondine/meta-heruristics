# SPEC-0006 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-08-30（进行中）
- 最终结果：`Pending`

## 当前状态

Spec 与 Plan 已批准，实施进行中。T001 已记录当前手写生产路径基线；生成后必须使用相同配置复测，并以 JIT 结构、分配和统计结果共同判定零开销。

## T001 手写 SIMD 基线

### 环境

- Windows 11 10.0.26200，AMD Ryzen 7 9800X3D
- .NET SDK 10.0.400，.NET 10.0.11，BenchmarkDotNet 0.15.8
- X64 RyuJIT x86-64-v4；`Vector512`、`Vector256`、`Vector128` 均启用，`double` lane 数分别为 8、4、2
- MediumRun：15 次目标迭代、2 次 launch、10 次 warmup；所有生成后比较必须保持相同 job、filter 与参数

### 完整命令

以下命令均从仓库根目录执行，退出码均为 0，且实际执行数量与预期一致：

```powershell
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.RepairBenchmarks.Reflect' --job Short --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/reflect
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.PsoCandidateUpdateBenchmarks.VectorOpsCandidateUpdate' --job Medium --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/pso-kernel
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.PsoAdvanceBenchmarks.VectorOpsAdvanceLifecycle' --job Medium --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/pso-e2e
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.FireflyMoveBenchmarks.VectorOpsMove' --job Medium --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/firefly-kernel
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.FireflyAdvanceBenchmarks.VectorOpsAdvanceLifecycle' --job Medium --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/firefly-e2e
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.RepairBenchmarks.Reflect' --job Dry --disasm --disasmDepth 3 --disasmFilter '*ReflectCandidateRepair*' --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/jit-reflect
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.PsoCandidateUpdateBenchmarks.VectorOpsCandidateUpdate' --job Dry --disasm --disasmDepth 3 --disasmFilter '*VectorOps*' --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/jit-pso
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.FireflyMoveBenchmarks.VectorOpsMove' --job Dry --disasm --disasmDepth 3 --disasmFilter '*VectorOps*' --artifacts BenchmarkDotNet.Artifacts/spec-0006-baseline/jit-firefly
```

### 结果摘要

| 路径 | 维度 | Mean | Error | StdDev | Allocated |
| --- | ---: | ---: | ---: | ---: | ---: |
| PSO kernel | 32 | 22.295 ns | 1.6489 ns | 2.4680 ns | 0 B |
| PSO kernel | 128 | 37.340 ns | 0.0443 ns | 0.0606 ns | 0 B |
| PSO lifecycle | 32 | 42.06 us | 0.141 us | 0.202 us | 456 B |
| PSO lifecycle | 128 | 110.58 us | 1.202 us | 1.724 us | 456 B |
| Firefly kernel | 32 | 17.24 ns | 0.102 ns | 0.146 ns | 0 B |
| Firefly kernel | 128 | 31.51 ns | 0.131 ns | 0.196 ns | 0 B |
| Firefly lifecycle | 32 | 4.170 ms | 0.0119 ms | 0.0162 ms | 20.45 KB |
| Firefly lifecycle | 128 | 14.866 ms | 0.0149 ms | 0.0214 ms | 20.45 KB |

Reflect 共执行 40/40 个参数组合且均为 0 B；代表值为 32 维 scalar/scalar 1000.0 ns、vector/vector 1733.3 ns，128 维分别为 3033.3 ns 与 3466.7 ns。该 benchmark 固定 `InvocationCount=1`，迭代时间过短且误差很大，因此只作诊断，不单独作为回退结论；生成后仍保持相同配置，主要依赖 JIT 结构、分配及行为证据。

Dry disassembly 分别执行 Reflect 40/40、PSO 11/11、Firefly 11/11。PSO 与 Firefly 的基线 code size 分别为 2004 B、2753 B；Reflect 因边界形状不同为 3938 B、4390 B 或 4406 B。产物位于被忽略的 `BenchmarkDotNet.Artifacts/spec-0006-baseline/`，不提交仓库。

## T002 生成器基础设施

- `dotnet test tests/Metaheuristics.Simd.Generators.Tests/Metaheuristics.Simd.Generators.Tests.csproj -c Release --no-restore`：通过，23/23。
- `dotnet build Metaheuristics.NET.slnx -c Release --no-restore`：通过，0 warning、0 error。
- 正例覆盖 `double`/`float`/`int` 三种显式能力组合、512→256→128 展开顺序、由生成器插入的三层 `IsHardwareAccelerated` 门、方法级三宽度展开、指定尾部长度的动态编译执行、逐字节确定输出、不同路径 hint name 与第二轮 `Cached` 增量步骤。
- 负例覆盖类型/能力不匹配、缺失/重复/错误形状 expansion block、未知占位符、整数能力使用浮点运算、跨模板重复目标和 AdditionalFile 诊断位置。

## T003 Algorithms 生成迁移

- `VectorOps.cs` 仅保留 partial 容器，`ComputePsoVelocity`、`DistanceSquared` 与 `UpdateFireflyPosition` 的单一权威位于 AdditionalFiles 模板；模板通过 `__SimdExpandHardwareAcceleratedWidths` 声明块体，不再手写任一宽度的硬件门。
- `dotnet test Metaheuristics.NET.slnx -c Release --no-build` 在 T003 候选上通过，既有行为测试 144/144；生成器测试随后扩充为 23/23，完整当前测试为 167/167。
- Firefly Dry disassembly 执行 11/11，生成候选 code size 为 2753 B；PSO Dry disassembly 执行 11/11，生成候选 code size 为 2004 B，均与 T001 手写基线一致。
- 将反汇编中的 12 至 16 位十六进制地址归一化后，Firefly 生成候选与基线文本相同（两侧归一化长度均为 225445），PSO 也相同（两侧均为 173690）；未出现额外调用或控制流。完整 timing 仍由 T005 同配置复测。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | 受限增量生成器、Algorithms 模板 | 生成器 23/23、Algorithms JIT | [`spec.md`](./spec.md) | Partial |
| FR-002 | Pending | Pending | [`spec.md`](./spec.md) | Pending |
| FR-003 | Pending | Pending | [`spec.md`](./spec.md) | Pending |
| FR-004 | 显式元素类型与能力 metadata | 生成器正/负例 | [`spec.md`](./spec.md) | Pass |
| FR-005 | 稳定 hint、相对 `#line`、确定输出 | 生成器确定性/增量测试 | [`spec.md`](./spec.md) | Partial |
| FR-006 | Pending | Pending | [`spec.md`](./spec.md) | Pending |
| NFR-001 | Algorithms 完全展开输出 | PSO/Firefly JIT 等价 | [`spec.md`](./spec.md) | Partial |
| NFR-002 | Algorithms 既有公式与标量尾部 | 完整行为测试 167/167 | [`spec.md`](./spec.md) | Partial |
| NFR-003 | Pending | Pending | [`spec.md`](./spec.md) | Pending |
| NFR-004 | 受限语法节点替换，无运行时 DSL | 生成器 23/23、Release build | [`spec.md`](./spec.md) | Partial |
