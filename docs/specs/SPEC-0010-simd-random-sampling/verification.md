# SPEC-0010 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-09-13
- 最终结果：`Failed`

## 结论

1. **实现完成**：`RandomSource` 现在有两套互不推进的状态——四个 `ulong` 单值状态字，加四个“一轮 `Vector<T>.Count` 个样本”的批量状态向量。新增 `NextULongVector`/`NextDoubleVector`；四个 `Fill` 重载和 `StandardNormal.Fill` 改走批量状态；单值入口的序列和行为与之前完全一致。
2. **正确性通过**：170 个测试在 128 位、256 位、512 位与“关闭硬件内建”四种配置下全部通过；批量输出与测试工程里独立的逐 lane 参考实现逐位一致；所有采样调用 0 B 分配。
3. **性能**：批量填充比旧实现快 1.1–5.3 倍（向量越宽收益越大），有界 double 快 1.4–5.0 倍，单值入口基本持平。
4. **唯一未达标**：128 位下的正态填充比旧实现慢约 40%。它在 256/512 位是快的（1.1–1.3 倍），但 128 位不行——原因见最后一节。
5. 构造随机源要为每个 lane 做一次 Jump，所以比旧实现贵很多（128/256/512 位分别约 471/903/1790 ns）。这是 Spec 规定的播种规则，不是缺陷，但短 run 需要留意。

## 名词对照（报告里的简称对应哪个 API）

| 报告里的写法 | 对应 API | 写出来的东西 |
| --- | --- | --- |
| 原始填充、原始 `Fill(n)` | `RandomSource.Fill(Span<ulong>)` | n 个完整 64 位范围的均匀原始值（生成器直接输出） |
| 单位 double 填充、单位填充 | `RandomSource.Fill(Span<double>)` | n 个 `[0,1)` 的 double，即 `(raw >> 11) * 2^-53` |
| 有界 double 填充 | `RandomSource.Fill(Span<double>, minimum, maximum)` | n 个 `[min,max)` 的 double，含上界舍入修正 |
| 有界 int 填充 | `RandomSource.Fill(Span<int>, minimum, maximum)` | n 个 `[min,max)` 的无偏整数（乘高位 + 拒绝采样） |
| 正态填充 | `StandardNormal.Fill(random, Span<double>)` | n 个标准正态样本（Box–Muller） |
| 向量 API：原始 / 单位 | `NextULongVector()` / `NextDoubleVector()` | 一个向量宽度的“一轮”样本 |
| 单值入口 | `NextULong()`、`NextDouble()`、`NextInt()`、`StandardNormal.Sample()` | 只推进标量状态的逐次采样 |

## 环境与读法

| 项目 | 值 |
| --- | --- |
| 机器 | AMD Ryzen 7 9800X3D 4.70 GHz，Windows 11 25H2，.NET 10.0.11（X64 RyuJIT `x86-64-v4`） |
| 基准 | BenchmarkDotNet 0.15.8，`MemoryDiagnoser`，5 次 warmup、12 次 measurement，固定种子 `0x0123456789ABCDEF` |
| 宽度设置 | `DOTNET_MaxVectorTBitWidth=128/256/512`，实测 `Vector<ulong>.Count` = 2/4/8（基准程序启动时会打印） |

- **旧实现**：SPEC-0009 的单状态实现（提交 `56c1f57` 的副本），只推进一组四字状态。它是所有倍数的分母。
- **倍数**：`2.00×` = 现在比旧实现快一倍；`0.60×` = 现在比旧实现慢 40%。每个数字都取自同一次运行的同一进程。
- **长度 32/128**：一次 `Fill` 写出的样本个数。
- 所有采样场景的分配都是 0 B/op，因此表中不再单列；只有构造会分配（128/256/512 位分别 112/176/304 B，正好是批量状态本身）。

## 性能结果（2026-09-13 一套矩阵，三种宽度）

| 场景 | 128 位（2 lane） | 256 位（4 lane） | 512 位（8 lane） |
| --- | ---: | ---: | ---: |
| 原始 `Fill(32)` | 1.15× | 2.08× | 3.85× |
| 原始 `Fill(128)` | 1.14× | 2.22× | 5.26× |
| 单位 double `Fill(32)` | 1.12× | 1.89× | 2.94× |
| 单位 double `Fill(128)` | 1.06× | 2.08× | 4.00× |
| 有界 double `Fill(32)` | 1.43× | 2.63× | 4.55× |
| 有界 double `Fill(128)` | 1.41× | 2.86× | 5.00× |
| 有界 int `Fill(32)` | 1.01× | 1.03× | 0.97× |
| 有界 int `Fill(128)` | 1.03× | 1.03× | 1.05× |
| 正态 `Fill(32)` | **0.61×** | 1.10× | 1.10× |
| 正态 `Fill(128)` | **0.60×** | 1.14× | 1.33× |
| 单值 `NextULong`/`NextDouble`/`NextInt`/`Sample` | 0.94–1.02× | 0.96–1.02× | 0.99–1.04× |
| 向量 API `NextULongVector` / `NextDoubleVector` | 3.70× / 4.35–4.55× | 4.35× / 5.00× | 4.55× / 4.76–5.00× |

两点读表说明：

- 有界 int 三种宽度都约 1.0×：它的瓶颈是“每个 lane 都要做一次 64 位乘高位 + 拒绝判断 + 写 int”，这部分工作量随 lane 数线性增长，向量化只加速了状态推进。
- 向量 API 那一行的对照不是旧实现（旧实现没有同签名 API），而是与候选语义相同的标量 lane 参考实现。
- 跨列比较请留余量：同一份旧实现代码在不同 run 之间会落在约 ±20% 的两个频率档位；表内每个数字都来自同一 run 内的对照，各自有效，但“512 位比 256 位快多少”这类结论会被档位差放大。

## 正确性、随机质量与 SIMD 证据

- **独立参考**：测试工程里有一份逐 lane 标量参考（自己实现 SplitMix64、Jump、状态转换、乘高位拒绝与区间映射），与生产代码不共享路径。
- **覆盖**：五种 seed 的初始化与 512 轮输出；长度 0/1/2/3/`L±1`/`2L±1`/31/32/33/127/128/129/1024；有界 int 的拒绝顺序（含宽度 1 与 2 的幂区间）；有界 double 与标量公式逐位一致；单值/批量双向隔离；空目标与非法区间不消费状态；正态尾块、配对、相对误差 ≤1e-12 与恰好 `ceil(N/L)` 轮。
- **随机质量**：固定 seed、每 lane 2^20 轮，检查每 bit 的 1 计数、每字节桶计数（7σ 预算）与相邻值相关性；正态另有 100 万样本统计。失败不重试、不换种子。
- **SIMD 证据**（FR-006）：批量状态推进全程在向量寄存器里完成，循环左移在支持的平台上用硬件旋转指令。512 位宽度的循环体（`RsJitDiagnosticsBenchmarks.RawFill`，本轮实测反汇编摘录）：

  ```asm
  M00_L01:
         vpaddq    zmm0,zmm6,zmm9      ; s0 + s3
         vprolq    zmm0,zmm0,17        ; rotl(x, 23)
         vpxord    zmm2,zmm6,zmm8      ; s2 ^= s0
         vpxord    zmm3,zmm7,zmm9      ; s3 ^= s1
         vpaddq    zmm0,zmm6,zmm0      ; result = rotl(...) + s0
         vprolq    zmm3,zmm3,2D        ; rotl(x, 45)
  ```

  `RotateLeft` 按宽度与指令集选择：512 位用 `Avx512F`，256/128 位用 `Avx512F.VL` 或 `Avx10v1`，其余平台（含 ARM64 的 NEON/SVE/SVE2）回退到“两次移位 + 或”。反汇编文件不入库，可用文末命令随时再生。

## 工程验证

- `dotnet build Metaheuristics.NET.slnx -c Release`：0 警告 0 错误。
- `dotnet test tests/Metaheuristics.Tests`：默认、`DOTNET_MaxVectorTBitWidth=128`、`=512`、`DOTNET_EnableHWIntrinsic=0` 四种配置各 170/170 通过；软件路径实测 `Vector<ulong>.Count=2`、`IsHardwareAccelerated=False`。
- `dotnet format --verify-no-changes`：通过。
- `eng/test-documentation-verifier.ps1`：通过；`eng/verify-documentation.ps1` 只剩既有缺口（`SPEC-0008` 空目录，`SPEC-0011`–`SPEC-0014` 缺 Plan/Tasks/Verification），SPEC-0010 自身无错误。
- DocFX：0 警告 0 错误。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `RandomSource` 双状态与 Jump 播种 | `RandomSourceBatchTests.VectorSamplesMatchIndependentJumpSeededLanes` | spec、plan、ADR-0024 | Passed |
| FR-002 | `NextULongVector`/`NextDoubleVector` 与路由 | `UnitVectorMatchesRawVectorMapping`、隔离测试 | spec、plan、XML 注释 | Passed |
| FR-003 | 四个 `Fill` 重载的排放与拒绝 | `RawFillMatchesReferenceEmissionForAllShapeBoundaries`、`BoundedIntegerFillMatchesReferenceRejectionOrder` | spec、plan | Passed |
| FR-004 | `StandardNormal.Fill` 向量数学 | `VectorFillMatchesScalarBoxMullerReferenceAndConsumesWholeRounds`、100 万样本统计 | spec、plan | Passed |
| FR-005 | 单值数值/异常/确定性边界 | `RandomSourceTests`、`EmptyAndInvalidOperationsPreserveStateAndTargets` | spec、plan | Passed |
| FR-006 | 向量内核与 SIMD 证据 | 三宽度测试、软件路径测试、上面的反汇编摘录 | spec、plan、ADR-0024 | Passed |
| NFR-001 | 三宽度性能矩阵 | 本报告的性能表 | spec、plan | Failed |
| NFR-002 | 固定种子随机质量 | `BatchStreamsPassFixedSeedStatisticalChecks`、`FixedSeedMillionSampleStatisticsMeetTheApprovedThresholds` | spec、plan | Passed |

NFR-001 记 `Failed` 的唯一原因是 128 位正态填充（0.60×），Plan 对它写的要求是“不低于 0.90×”。

## 未达标项与待决定

128 位正态为什么慢：`StandardNormal.Fill` 按 Spec 的“相邻 lane 配对”实现，于是每个 pair 的 `log`/`sqrt` 在两个 lane 上各算一遍，`SinCos` 也算出 cos/sin 各两份（4 个值只用 2 个）。256/512 位时这点浪费被向量宽度摊平（仍有 1.1–1.3 倍），128 位时向量超越函数本身又不比标量 `Math` 快，浪费的一半就暴露成 0.60×。

两条路（需要项目作者决定）：

1. 按 SIMD 重写正态：把两个 unit 向量打包后一次算完（消灭重复计算），用 ISA 门控的 lane 重排指令；估计 512 位从 7.1 ns/输出降到约 3.9 ns/输出，128 位也能回到 1.0× 附近。
2. 接受现状：128 位（只有 2 lane 的环境）不使用批量正态，或接受它比标量慢。

另有一个不涉及性能的决定：`Fill(Span<int>, int, int)` 目前没有任何生产调用点（全仓库只有基准与测试用它），而它是唯一 SIMD 无收益的路径；建议删掉，将来 SPEC-0012 做批量索引抽样时再按 SIMD 方式重新设计。

## 证据与复现

证据只保留 BenchmarkDotNet 的 `-report-github.md` 汇总表：`evidence/width-128/`、`evidence/width-256/`、`evidence/width-512/`，每个目录 14 个文件（13 个对照场景类 + 1 个 JIT 诊断类）。没有脚本、原始日志或反汇编文件。

```sh
dotnet build Metaheuristics.NET.slnx -c Release
dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=128 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=512 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_EnableHWIntrinsic=0 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build

# 性能矩阵：三个宽度各跑一次（--artifacts 可指向临时目录，只保留需要的报告文件）
DOTNET_MaxVectorTBitWidth=128 dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Rs*Benchmarks*"

# 上面摘录的反汇编（只跑诊断类，不需要保留文件）
DOTNET_MaxVectorTBitWidth=512 dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*RsJitDiagnostics*" --job Short
```
