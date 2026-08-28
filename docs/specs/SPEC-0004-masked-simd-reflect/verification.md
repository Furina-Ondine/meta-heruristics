# SPEC-0004 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-08-28
- 最终结果：`Passed`

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `CandidateRepairs.ReflectCandidateRepair` 的 512/256/128 位块内核 | `CandidateRepairsTensorTests` 混合 lane 差分；`CanUseTensorPath` 残留搜索 | SPEC-0004 | Passed |
| FR-002 | 块内 Clamp/Reflect 掩码与大余数逐 lane 标量修补 | 有限、混合、特殊、端点、溢出与大偏移差分测试 | SPEC-0003/0004 | Passed |
| FR-003 | 创建时 width/period 缓存；无调用级缓冲区 | MemoryDiagnoser、构造/长度既有测试 | SPEC-0004 | Passed |
| FR-004 | `Vector512 → Vector256 → Vector128 → 单元素尾部` 调度 | 指定 2/7/8/31/32/33/127/128/129/1024 长度测试和基准 | SPEC-0004 | Passed |
| FR-005 | `RepairBenchmarks.LegacyTensorReflect` 与 `Reflect` | 80 项 ShortRun 同机对比 | 本报告 | Passed |
| NFR-001 | Core 私有实现与既有运行调用链 | Release build、完整测试、API/随机性审查 | architecture overview | Passed |
| NFR-002 | 仅 `CandidateRepairs` 使用 Intrinsics | 项目/残留搜索审查 | SPEC-0004 | Passed |

## 性能证据

运行环境：Windows 11、AMD Ryzen 7 9800X3D、.NET SDK 10.0.400、.NET Runtime 10.0.11、x64 RyuJIT x86-64-v4、BenchmarkDotNet 0.15.8、ShortRun（3 次测量迭代）。原始报告位于本机忽略的 `BenchmarkDotNet.Artifacts/results/Anastasya.Metaheuristics.Benchmarks.RepairBenchmarks-report-github.md`。

`LegacyTensorReflect` 是提交 `21804bd` 的整段安全预扫描/Tensor 管线的私有 benchmark 复现。`Reflect` 是本规格的掩码内核。主要门槛均通过：

| 边界形状 | 长度 | 旧分派平均值 | 掩码 SIMD 平均值 | 比率 |
| --- | ---: | ---: | ---: | ---: |
| 标量/标量 | 32 | 3,633.3 ns | 1,500.0 ns | 0.41x |
| 向量/向量 | 32 | 2,433.3 ns | 1,033.3 ns | 0.47x |
| 标量/标量 | 128 | 6,533.3 ns | 3,366.7 ns | 0.57x |
| 向量/向量 | 128 | 4,966.7 ns | 3,500.0 ns | 0.76x |

全四种边界形状和所有既定长度共 80 项比较均已运行；MemoryDiagnoser 均报告无 Repair 调用级托管分配。长度 7 在本机同样快于旧分派，但 ShortRun 仅作为诊断，不将单机数据外推为所有硬件或输入的性能承诺。

## 数值发现与处理

手写向量 `remainder = offset - period * Truncate(offset / period)` 对极大有限 offset 会因中间舍入偏离标量 `%`：测试中 `-1e100` 在 `[-10, 10]` 的参考结果为 `6`，未经修补的向量结果为 `-10`。内核以 SIMD 掩码识别绝对商大于 2 的可反射 lane，并只对这些 lane 调用私有标量参考；其它 lane 继续留在同一向量块的 SIMD 路径。该处理保留“不整段回退”的目标，并使测试中的特殊/极大输入逐位或在已批准的 1 ULP 范围内兼容。

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| `CanUseTensorPath` 整段预扫描 | 删除 | `src`、`tests`、`benchmarks` 中无运行时代码残留 | Passed |
| 整段 Tensor Reflect 管线 | 删除 | 仅 benchmark 的 `LegacyTensorReflect` 保留为性能基线 | Passed |
| 公开 SIMD 抽象/开关 | 不新增 | 未发现 `SimdRepair` 或新增 `VectorOps`；Intrinsic 仅在 Core 私有实现 | Passed |

## 架构一致性

- 策略职责是否保持独立：是。Core 继续唯一拥有边界、Repair、Tensor 与 Intrinsic 实现；Algorithms 未变。
- 是否新增重复验证：否。创建时与调用时的既有边界/长度验证不变；lane 掩码仅实施 Reflect 的数值分支。
- 是否存在无消费者抽象：否。三种向量宽度方法仅服务私有 Reflect 内核；旧路径基线只在 benchmark 使用。
- 职责是否位于批准的项目层：是。未新增项目或包依赖。
- 是否出现未经批准的兼容层：否。

## 工程验证

- Restore：Passed — `dotnet restore Metaheuristics.NET.slnx`。
- Release Build：Passed — `dotnet build Metaheuristics.NET.slnx -c Release --no-restore`，0 warning / 0 error。
- Tests：Passed — `dotnet test Metaheuristics.NET.slnx -c Release --no-build`，105 passed。
- Format：Passed（本次文件）— 对 Core、测试和 benchmark 文件执行 `dotnet format ... --verify-no-changes --no-restore`。
- 文档链接与规格检查：Passed — `git diff --check` 通过；Spec/Plan/Tasks/Verification 链接齐全。
- DocFX：Passed — `dotnet tool run docfx docfx.json`，0 warning / 0 error。
- Benchmark 或分配分析：Passed — 80 项 Reflect ShortRun 对比与 MemoryDiagnoser，结果如上。

## 未解决问题

- 无。
