# SPEC-0010 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-09-12
- 最终结果：`Failed`

## 先说结论

双状态随机源、两个向量采样入口和向量正态填充都已实现，正确性测试在 128、256、512 三种向量宽度和软件路径下全部通过。性能结论分三档：

- **512 位（每轮 8 个样本）**：批量填充比原实现快 2.6–3.7 倍，有界 double 快约 4 倍，正态快 1.1–1.35 倍。
- **256 位（每轮 4 个样本）**：批量填充快 1.4–1.5 倍，有界 double 快约 2 倍，正态快 8–12%。
- **128 位（每轮 2 个样本）**：2026-09-12 的原始矩阵里批量原始/单位填充比原实现慢 27–39%，正态慢 66%；在 2026-09-13 接入 ISA 旋转指令后，原始填充变为快 18–20%、单位填充快 9%，**只有正态仍慢 39%**。正态是当前唯一未达门槛的场景，因此本报告结论仍是 `Failed`。

单值随机入口（`NextULong`、`NextDouble`、`NextInt`、`StandardNormal.Sample`）在所有宽度下与原实现一致，差异在 ±3% 以内。构造一个随机源需要为每个 lane 做一次 Jump，成本随宽度线性上升：128 位约 455 ns、256 位约 903 ns、512 位约 1796 ns（原实现约 2 ns，不构造 lane 状态）。

## 怎么读下面的表

- **A**：改动前的实现（SPEC-0009 单状态版本，提交 `56c1f57` 的副本）。
- **B**：本次候选，即现在 `src` 里的实现。
- **R**：只给基准用的标量参考——播种和排放规则与 B 相同，但每个 lane 用标量状态推进。
- 表里的数字是**候选比对照快几倍**（`对照耗时 ÷ 候选耗时`）：2.00× 表示候选快一倍，0.75× 表示候选慢 25%。
- 比值与分配都取自同一次 BenchmarkDotNet 运行内的同参数对照（BenchmarkDotNet 的 `Ratio` 列），因此不受两次运行之间的频率漂移影响。长度 32/128 指一次 `Fill` 写出的元素个数；有界区间是 `[-5, 5)`。

## 环境

| 项目 | 值 |
| --- | --- |
| CPU / OS | AMD Ryzen 7 9800X3D 4.70 GHz / Windows 11 25H2（10.0.26200） |
| 运行时 | .NET 10.0.11，X64 RyuJIT `x86-64-v4` |
| 基准 | BenchmarkDotNet 0.15.8，`MemoryDiagnoser`，5 次 warmup、12 次 measurement |
| 宽度设置 | `DOTNET_MaxVectorTBitWidth=128/256/512`；`DOTNET_PreferredVectorBitWidth` 只能在上限内下调，单独设 512 无效 |
| 实测宽度 | 128 → `Vector<ulong>.Count=2`；256 → 4；512 → 8（基准程序启动时打印） |
| 固定种子 | `0x0123456789ABCDEF` |

软件路径另测一次：`DOTNET_EnableHWIntrinsic=0` 时 `Vector<ulong>.Count=2`、`Vector.IsHardwareAccelerated=False`，因此该配置的测试跑的是 128 位形状的矢量软件实现，用于 FR-006 的“无硬件加速”验收。

## 三宽度性能结果

### 批量填充（候选 B 相对改动前 A）

下表是 2026-09-12 的整体矩阵，尚未包含 2026-09-13 的 ISA 旋转指令改动；当前状态见下文“2026-09-13 追加”。每列原始报告见 `evidence/width-128/`、`evidence/width-256/`、`evidence/width-512/`（文件名前缀 `Anastasya.Metaheuristics.Benchmarks.Rs*Benchmarks-report-github.md`）。

| 场景 | 128 位 | 256 位 | 512 位 |
| --- | ---: | ---: | ---: |
| 原始 `Fill(32)` | 0.79× | 1.41× | 2.94× |
| 原始 `Fill(128)` | 0.75× | 1.49× | 3.64× |
| 单位 double `Fill(32)` | 0.74× | 1.47× | 2.63× |
| 单位 double `Fill(128)` | 0.72× | 1.43× | 2.79× |
| 有界 double `Fill(32)` | 1.06× | 2.00× | 3.70× |
| 有界 double `Fill(128)` | 1.02× | 1.96× | 4.00× |
| 有界 int `Fill(32)` | 1.10× | 1.09× | 1.10× |
| 有界 int `Fill(128)` | 1.08× | 1.04× | 1.10× |
| 正态 `Fill(32)` | 0.60× | 1.12× | 1.35× |
| 正态 `Fill(128)` | 0.60× | 1.08× | 1.10× |

原始/单位填充在 128 位下没有达到 Plan 的门槛（要求不低于 0.90×），其余各点都通过与“不慢于原实现”的要求。

### 向量 API 与单值入口

| 场景 | 128 位 | 256 位 | 512 位 |
| --- | ---: | ---: | ---: |
| `NextULongVector`（相对 R） | 3.2× | 3.7× | 4.5× |
| `NextDoubleVector`（相对 R） | 3.1× | 4.2× | 4.8× |
| `NextULong`（相对 A） | 1.00× | 0.99× | 1.00× |
| `NextDouble`（相对 A） | 0.99× | 0.92–1.01× | 0.99× |
| `NextInt(-5,5)`（相对 A） | 1.00× | 0.97–1.12× | 0.99× |
| `StandardNormal.Sample`（相对 A） | 1.00× | 1.03× | 1.03× |

单值入口改动前后是同一段代码，表中的小差异属于测量波动：同一批场景在不同宽度下方向不一致（0.92×–1.12×），且都在 ±12% 以内。向量 API 的对照对象是标量参考 R，不是改动前的实现——改动前没有同签名的向量入口。

### 构造与分配

| 场景 | A | B（128 位） | B（256 位） | B（512 位） |
| --- | ---: | ---: | ---: | ---: |
| 构造 + 首次采样 | 2.0 ns / 0 B | 455 ns / 112 B | 903 ns / 176 B | 1796 ns / 304 B |
| 构造 + `Fill(32)` | 19–24 ns / 0 B | 478 ns / 112 B | 909 ns / 176 B | 1786 ns / 304 B |

多出的字节就是批量状态本身：`32 + 32×lane 数` 加上对象头。构造里每个 lane 要做一次 Jump（4 个多项式 × 64 步），这是 Spec 要求的播种规则，没有用懒初始化把它藏到首次采样后面。所有采样调用本身是 0 B/op。

## 正确性与随机质量

- 测试工程里有一份独立的逐 lane 标量参考实现（自己的 SplitMix64、Jump、状态转换、乘高位拒绝和区间映射），与生产代码不共享代码路径。
- 覆盖：五种 seed 的 lane 初始化与 512 轮输出；长度 0/1/2/3/`L±1`/`2L±1`/31/32/33/127/128/129/1024 的排放、轮尾丢弃与后续状态；有界 int 的拒绝顺序（含宽度 1 与 2 的幂区间）；有界 double 的标量公式逐位一致；单值/批量互不推进的双向隔离；空目标与非法区间不消费状态；正态尾块、相邻 lane 配对、相对误差 ≤1e-12 以及恰好消费 `ceil(N/L)` 轮。
- 随机质量：固定 seed、每 lane 2^20 轮，检查每 bit 的 1 计数、每字节桶计数（7σ 预算）与相邻值的 Pearson 相关性；正态沿用 100 万样本的均值/方差/分位/尾部门槛。失败不重试、不换 seed。

## JIT 证据（这轮性能改动从哪里来）

### 2026-09-13 追加：ISA 旋转指令（AVX-512F / AVX-512VL `vprolq`，三种宽度全接入）

按项目作者指示，`RotateLeft` 增加了 ISA 探测：宽度匹配且支持时走 `Avx512F.RotateLeft`（512 位）或 `Avx512F.VL.RotateLeft`／`Avx10v1.RotateLeft`（256 位与 128 位），否则回退到“两次移位 + 或”。宽度转换使用 `Vector<T>.AsVector512/256/128()` 与 `Vector512/256/128<T>.AsVector()`；与上一版 `Unsafe.BitCast` 的逐指令反汇编比较完全一致（144 行、唯一差异是 `call` 里的运行时地址），因此两种转换方式没有性能差异。

复测（同一档位、同一次运行内对照）：

| 场景 | 改动前候选 | 改动后候选 | 同档位基线 | 相对基线 |
| --- | ---: | ---: | ---: | ---: |
| 原始 `Fill(32)`，128 位 | 25.00 ns | 17.050 ns | 20.520 ns | 1.20× |
| 原始 `Fill(128)`，128 位 | 97.15 ns | 64.480 ns | 76.130 ns | 1.18× |
| 单位 double `Fill(128)`，128 位 | 108.83 ns | 73.260 ns | 79.860 ns | 1.09× |
| 原始 `Fill(32)`，256 位 | 14.10 ns | 8.999 ns | 19.091 ns | 2.12× |
| 原始 `Fill(128)`，256 位 | 49.06 ns | 32.834 ns | 73.212 ns | 2.23× |
| 单位 double `Fill(128)`，256 位 | 54.72 ns | 36.950 ns | 78.020 ns | 2.11× |
| 原始 `Fill(32)`，512 位 | 7.969 ns | 6.633 ns | 23.979 ns | 3.62× |
| 原始 `Fill(128)`，512 位 | 25.887 ns | 17.946 ns | 94.585 ns | 5.27× |
| 单位 double `Fill(128)`，512 位 | 28.209 ns | 19.971 ns | 84.379 ns | 4.22× |

每轮成本：128 位原始填充 1.52 → 1.01 ns，256 位 1.53 → 1.03 ns，512 位 1.62 → 1.12 ns。四套配置的 170 个测试全部通过。

128 位唯一仍未达门槛的是正态填充：候选 1682.3 ns、改动前 1031.3 ns（0.61×），瓶颈是 `Vector.Log`/`Vector.SinCos` 的软件实现与每对重复的超越函数计算，不随旋转指令改善。

本节数字来自局部复测；`width-128/`、`width-256/` 与 `width-512/` 目录里其余场景（有界 double/int、正态、单值、构造）仍是 2026-09-12 整体矩阵的快照，将在下一次整体矩阵运行中刷新。原始报告见 `evidence/isa-rotate/`。

反汇编显示批量转换体把四个状态向量一直放在寄存器里（512 位下为 `zmm0`–`zmm3`），循环里每轮只有一次 64 字节写入，状态字段只在进入和退出时各访问一次。这解决了两处实测出来的内存/计算浪费：

1. **有界 int 填充的重复落栈**：早先版本在 lane 循环内部每次都把整条向量写回栈再按偏移读取（512 位下每轮 8 次 64 字节写）。现在整轮只落栈一次，再按 lane 顺序读。
2. **循环内的 64 位除法**：拒绝阈值 `2^64 mod range` 原来在 lane 循环里重算，反汇编中能看到 `div`；现在每次调用只算一次。这两处合计把 512 位下的有界 int 填充从**慢 2.0 倍**改成**快 1.10 倍**。

反汇编同时确认：原始状态推进使用向量加法/异或/移位/或；`StandardNormal.Fill` 的 `Log`、`SquareRoot`、`SinCos` 都在向量上执行。三个宽度的完整反汇编见各自证据目录下的 `Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks-asm.md`。

## 工程验证

- Release 构建：`Metaheuristics.NET.slnx` 0 警告 0 错误。
- 测试：`Metaheuristics.Tests` 在默认宽度、`DOTNET_MaxVectorTBitWidth=128`、`=512` 与 `DOTNET_EnableHWIntrinsic=0` 四种配置下各 170/170 通过。
- 格式：`dotnet format --verify-no-changes`。
- 文档：`eng/test-documentation-verifier.ps1` 与 `eng/verify-documentation.ps1`（既有缺口单独报告）。
- 基准证据：三个宽度的 `-report-github.md` 与 `-asm.md`，见下文清单。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `RandomSource` 双状态与 Jump 播种 | `RandomSourceBatchTests.VectorSamplesMatchIndependentJumpSeededLanes` | spec、plan、ADR-0024 | Passed |
| FR-002 | `NextULongVector`/`NextDoubleVector` 与路由 | `UnitVectorMatchesRawVectorMapping`、隔离测试 | spec、plan、XML 注释 | Passed |
| FR-003 | 四个 `Fill` 重载的排放与拒绝 | `RawFillMatchesReferenceEmissionForAllShapeBoundaries`、`BoundedIntegerFillMatchesReferenceRejectionOrder` | spec、plan | Passed |
| FR-004 | `StandardNormal.Fill` 向量数学 | `VectorFillMatchesScalarBoxMullerReferenceAndConsumesWholeRounds`、100 万样本统计 | spec、plan | Passed |
| FR-005 | 单值数值/异常/确定性边界 | `RandomSourceTests` 既有用例、`EmptyAndInvalidOperationsPreserveStateAndTargets` | spec、plan | Passed |
| FR-006 | 四 ref 向量内核与 SIMD 证据 | 三宽度测试、软件路径测试、`-asm.md` | spec、plan、ADR-0024 | Passed |
| NFR-001 | 冻结候选与三宽度基准 | 本报告的 128/256/512 矩阵 | spec、plan | Failed |
| NFR-002 | 固定种子随机质量 | `BatchStreamsPassFixedSeedStatisticalChecks`、`FixedSeedMillionSampleStatisticsMeetTheApprovedThresholds` | spec、plan | Passed |

NFR-001 为 `Failed`：128 位宽度下的批量原始/单位填充是 0.72×–0.79×，低于 Plan 事先写下的 0.90× 门槛。

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| 单组四字状态被全部入口共享 | 单值与批量状态分离 | `rg "NextRawFromFields\|_state0"` 只在单值路径与测试参考出现 | Passed |
| `BatchCursor`/`BeginBatch`/`EndBatch` | 不引入 | 全仓库无匹配 | Passed |
| 固定宽度 `Vector128/256/512` 与 ISA 专属 API | 生产代码不引入 | `src/Metaheuristics.Core` 无匹配 | Passed |
| 中间脚本、临时工程与原始日志 | 删除，只留报告需要的证据 | 基准只保留 `-report-github.md` 与 `-asm.md` | Passed |

## 架构一致性

- 策略职责保持独立：随机能力仍由 Core 的封闭 `RandomSource`/`StandardNormal` 提供，算法层不改。
- 未新增无消费者抽象：没有接口、工厂、后端配置或游标类型。
- 职责位置正确：状态与分布留在 Core Randomness，基准参考只在基准程序集，参考实现只在测试程序集。
- 未引入兼容层：单值入口沿用 SPEC-0009 的行为，批量入口按 Spec 的新语义。

## 未解决问题

128 位宽度下批量路径慢于原实现，这是本轮唯一未达门槛的点，原因是结构性的：

- 原始状态推进在 `Vector<T>` 上必须写成“两次移位 + 一次或”来模拟循环左移（三条指令），而标量侧是一条 `rol`；本机 128 位整数向量运算的每周期吞吐也不高于标量整数运算，于是每轮 2 个样本没有换来吞吐收益。
- 正态的 `Log`/`SinCos` 向量实现在只有 2 个 lane 时要贵于两次标量 `Math` 调用。
- Spec 的 FR-006 要求批量内核只接受四个 `ref Vector<ulong>` 并使用向量运算，因此不能用标量 lane 专用路径绕过；要改变这一点需要先修订 Spec（以及 ADR-0024）并重新批准。

可选处理方式（需要项目作者决定）：接受当前结论并在文档里标注 128 位不划算；或者先修订 Spec/ADR，允许对 128 位宽度使用标量 lane 推进后再实施并复测。

## 证据清单与复现

每个宽度一份目录：`evidence/width-{128,256,512}/`，其中包含本次基准全部场景的 `-report-github.md`（BenchmarkDotNet 原始汇总表）与 `-asm.md`（反汇编诊断）。

```sh
dotnet build Metaheuristics.NET.slnx -c Release
dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=128 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=512 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_EnableHWIntrinsic=0 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=512 dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Rs*Benchmarks*"
```
