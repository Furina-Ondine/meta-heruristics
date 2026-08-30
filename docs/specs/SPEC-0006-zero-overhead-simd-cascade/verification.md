# SPEC-0006 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-08-30
- 最终结果：`Passed`

## 当前状态

生成器已替代纳入范围的 Core Reflect 与 Algorithms PSO/Firefly 手写固定宽度级联；硬件门同样由模板标记统一展开。Release JIT 结构、分配、行为测试、局部基准和端到端基准均已复核。唯一的高方差项是 PSO 32 维内核：两次生成后测量均报告双峰或宽置信区间，且与基线区间重叠；逐方法反汇编与手写基线结构相同，因此不构成统计上可确认的回退。

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

## T004 Core Reflect 生成迁移

- `CandidateRepairs.cs` 保留边界状态、参数派生和标量 `Reflect`；外层 `Repair` 级联及仅因位宽不同的 block、load、mask/lane 修补由 `CandidateRepairs.simd.cs` 单一模板生成。模板的外层级联不再手写 `IsHardwareAccelerated`。
- `dotnet test Metaheuristics.NET.slnx -c Release --no-build`：通过，167/167；`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`：通过，0 warning、0 error。
- Reflect Dry disassembly 执行 40/40。对齐维度的四种边界形状 code size 保持手写基线的 3938 B、4390 B、4390 B、4406 B；其他尾部维度也逐项保持相同 code size。
- 基线与生成候选均导出 550 个方法实例、28 种方法签名。归一化 12 至 16 位运行地址，并消除仅由反汇编输出方法顺序造成的 `Mxx_Lyy` 标签前缀后，按签名和出现次序比较的结构差异为 0；无新增调用、分支、加载/存储或分配。
- 首次命令因受限网络无法获取 NuGet 漏洞数据而以 NU1900 退出，未启动 benchmark；允许网络后以完全相同命令重试成功。完整 timing 仍由 T005 同配置复测。

## T005 生成后零开销与性能门槛

### 同配置命令与执行数量

生成后按 T001 完整命令执行：Reflect 40/40，PSO 局部 11/11、端到端 2/2，Firefly 局部 11/11、端到端 2/2；三项 Dry 反汇编分别执行 Reflect 40/40、PSO 11/11、Firefly 11/11。所有命令退出码为 0，产物位于被忽略的 `BenchmarkDotNet.Artifacts/spec-0006-generated/`。为复核 PSO 32 维局部测量的宽误差，另行以相同 Medium job 完整重跑 11/11 个 PSO 维度：

```powershell
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter '*.PsoCandidateUpdateBenchmarks.VectorOpsCandidateUpdate' --job Medium --artifacts BenchmarkDotNet.Artifacts/spec-0006-generated-rerun/pso-kernel
```

复跑退出码为 0，运行 8 分 11 秒；BenchmarkDotNet 将其中部分样本标记为双峰分布。

### JIT 与分配

- PSO 11/11 Dry：每一维 code size 均为 2004 B；将 12 至 16 位运行地址归一化后，与手写基线反汇编完全相同（双方 173800 字符）。
- Firefly 11/11 Dry：每一维 code size 均为 2753 B；地址归一化后与手写基线完全相同（双方 225665 字符）。
- Reflect 40/40 Dry：基线和生成版本均有 550 个方法实例、28 种签名；按签名与出现次序归一化运行地址和反汇编序号后，结构差异为 0。对齐边界形状 code size 保持 3938、4390、4390、4406 B。
- 所有局部基准仍为 0 B；PSO lifecycle 为 456 B，Firefly lifecycle 为 20.45 KB，均与基线相同。未发现生成器运行时调用、delegate、接口分派或新增分配。

### 时间结果

比率为 `基线耗时 / 生成后耗时`，仅描述本机、本 Runtime 和本次输入，不外推为通用承诺。

| 路径 | 维度 | 基线 | 生成后 | 比率 | 分配 |
| --- | ---: | ---: | ---: | ---: | ---: |
| PSO kernel | 32 | 22.295 ns ± 1.6489 | 24.721 ns ± 0.1348 | 0.902 | 0 B → 0 B |
| PSO kernel | 128 | 37.340 ns ± 0.0443 | 37.240 ns ± 0.1802 | 1.003 | 0 B → 0 B |
| PSO lifecycle | 32 | 42.06 us ± 0.141 | 42.21 us ± 0.117 | 0.996 | 456 B → 456 B |
| PSO lifecycle | 128 | 110.58 us ± 1.202 | 107.50 us ± 0.525 | 1.029 | 456 B → 456 B |
| Firefly kernel | 32 | 17.24 ns ± 0.102 | 16.33 ns ± 0.371 | 1.056 | 0 B → 0 B |
| Firefly kernel | 128 | 31.51 ns ± 0.131 | 30.54 ns ± 0.317 | 1.032 | 0 B → 0 B |
| Firefly lifecycle | 32 | 4.170 ms ± 0.0119 | 4.135 ms ± 0.0203 | 1.008 | 20.45 KB → 20.45 KB |
| Firefly lifecycle | 128 | 14.866 ms ± 0.0149 | 14.946 ms ± 0.0298 | 0.995 | 20.45 KB → 20.45 KB |

PSO 32 维内核首次生成后测量虽为 24.721 ns，但基线误差为 1.6489 ns，且完整复跑得到 21.730 ns ± 2.0730（median 24.579 ns），被 BenchmarkDotNet 标记为双峰。复跑与基线均覆盖相同硬件、Runtime、job 和参数，区间重叠；结合该路径 JIT 完全相同，结论为环境/跨运行测量波动，而非生成路径引入的可确认回退。Reflect 保持 T001 所述的 `InvocationCount=1` 诊断限制，仍以 JIT、分配和行为证据为主。

## T006 完整验证与残留审查

- `dotnet restore Metaheuristics.NET.slnx`：通过。
- `dotnet build Metaheuristics.NET.slnx -c Release --no-restore`：通过，0 warning、0 error。
- `dotnet test Metaheuristics.NET.slnx -c Release --no-build`：通过，167/167。
- `pwsh -NoProfile -File ./eng/test-documentation-verifier.ps1`：通过。
- `pwsh -NoProfile -File ./eng/verify-documentation.ps1`：通过。
- `git diff --check`：通过。生产模板残留搜索确认外层路径只使用 `__SimdExpandHardwareAcceleratedWidths`；没有 `if (__Vector.IsHardwareAccelerated)` 手写硬件门，也没有第二套 512/256/128 级联。
- `dotnet format Metaheuristics.NET.slnx --no-restore --verify-no-changes` 仍会报告仓库既有文件的 CRLF 与 import-order 格式问题（例如 `PsoOptimizer.cs`），与本次变更无关；为保护既有工作区，本任务没有批量重写这些文件。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | 受限增量生成器、Core/Algorithms 模板 | 生成器 23/23、三路径 JIT | [`spec.md`](./spec.md) | Passed |
| FR-002 | 消费程序集内完全展开方法 | Release build、三路径 JIT、引用审查 | [`spec.md`](./spec.md) | Passed |
| FR-003 | 模板保留公式、Reflect 数值与标量参考 | 完整测试 167/167 | [`spec.md`](./spec.md) | Passed |
| FR-004 | 显式元素类型与能力 metadata | 生成器正/负例 | [`spec.md`](./spec.md) | Passed |
| FR-005 | 稳定 hint、相对 `#line`、确定输出 | 生成器确定性/增量测试、完整验证 | [`spec.md`](./spec.md) | Passed |
| FR-006 | Core/Algorithms 旧机械展开已删除 | 残留审查、完整测试 | [`spec.md`](./spec.md) | Passed |
| NFR-001 | Core/Algorithms 完全展开输出 | Reflect/PSO/Firefly JIT 等价、分配一致 | [`spec.md`](./spec.md) | Passed |
| NFR-002 | 既有公式、Reflect 数值与标量尾部 | 完整行为测试 167/167 | [`spec.md`](./spec.md) | Passed |
| NFR-003 | 同配置局部/端到端 BenchmarkDotNet | 表中比率、PSO 复跑与分配 | [`spec.md`](./spec.md) | Passed |
| NFR-004 | 受限语法节点替换，无运行时 DSL | 生成器 23/23、Release build、项目引用审查 | [`spec.md`](./spec.md) | Passed |
