# SPEC-0010 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-09-14
- 最终结果：`Passed`

## 结论

1. **实现完成**：`RandomSource` 有两套互不推进的状态——四个 `ulong` 单值状态字，加四个“一轮 `Vector<T>.Count` 个样本”的批量状态向量。新增 `NextULongVector`/`NextDoubleVector`；三个 `Fill` 重载与 `StandardNormal.Fill` 只推进批量状态；单值入口的序列和行为与改动前完全一致。
2. **正确性通过**：169 个测试在 128 位、256 位、512 位与“关闭硬件内建”四种配置下全部通过；批量输出与测试工程里独立的逐 lane 参考实现逐位一致；所有采样调用 0 B 分配。
3. **性能全部达标**：批量原始/单位填充比旧实现快 **1.1–4.0 倍**，有界 double 快 **1.35–4.6 倍**，正态填充快 **2.25–7.4 倍**，三个宽度都随向量宽度增加而提高；单值入口保持持平。
4. **本轮的两处接口调整**（项目作者 2026-09-13 决定）：
   - 正态改成“两个单位向量配对”（一个给半径输入、一个给角度输入），消除了此前每个 pair 重复计算 `log`/`sqrt`/`sin`/`cos` 的浪费；写回进一步改为**向量粒度**（每块先写 L 个 cos 结果、再写 L 个 sin 结果），写回只剩两条向量存储；
   - 删除 `Fill(Span<int>, int, int)`：没有生产消费者，且是唯一向量化无收益的路径；无偏整数映射继续由 `NextInt` 承担。
5. 构造随机源仍要为每个 lane 做一次 Jump，所以比旧实现贵（128/256/512 位约 452/901/1801 ns），这是 Spec 规定的播种成本；单值入口的分配仍为 0。

## 名词对照（报告里的简称对应哪个 API）

| 报告里的写法 | 对应 API | 写出来的东西 |
| --- | --- | --- |
| 原始填充、原始 `Fill(n)` | `RandomSource.Fill(Span<ulong>)` | n 个完整 64 位范围的均匀原始值（生成器直接输出） |
| 单位 double 填充、单位填充 | `RandomSource.Fill(Span<double>)` | n 个 `[0,1)` 的 double，即 `(raw >> 11) * 2^-53` |
| 有界 double 填充 | `RandomSource.Fill(Span<double>, minimum, maximum)` | n 个 `[min,max)` 的 double，含上界舍入修正 |
| 正态填充 | `StandardNormal.Fill(random, Span<double>)` | n 个标准正态样本（Box–Muller，向量数学） |
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
- **长度 32/128**：一次 `Fill` 写出的样本个数。所有采样场景的分配都是 0 B/op，因此表中不单列。
- 跨列比较请留余量：同一份旧实现代码在不同 run 之间会落在约 ±20% 的两个频率档位；表内每个数字都来自同一 run 内的对照，各自有效。

## 性能结果（2026-09-14 一套矩阵，三种宽度）

| 场景 | 128 位（2 lane） | 256 位（4 lane） | 512 位（8 lane） |
| --- | ---: | ---: | ---: |
| 原始 `Fill(32)` | 1.37× | 2.13× | 3.23× |
| 原始 `Fill(128)` | 1.45× | 2.22× | 4.00× |
| 单位 double `Fill(32)` | 1.09× | 1.85× | 2.94× |
| 单位 double `Fill(128)` | 1.09× | 2.13× | 4.00× |
| 有界 double `Fill(32)` | 1.35× | 2.78× | 4.55× |
| 有界 double `Fill(128)` | 1.35× | 2.70× | 4.00× |
| 正态 `Fill(32)` | 2.33× | 4.43× | 7.41× |
| 正态 `Fill(128)` | 2.25× | 4.71× | 7.37× |
| 单值 `NextULong`/`NextDouble`/`NextInt`/`Sample` | 0.99–1.04× | 0.98–1.03× | 0.99–1.03× |
| 向量 API `NextULongVector` / `NextDoubleVector` | 3.85–4.00× / 5.00× | 4.35× / 5.00–5.26× | 4.35–4.55× / 4.76–5.00× |
| 构造（只报绝对值，不做倍数） | 452 ns | 901 ns | 1801 ns |

读表要点：

- 正态是本轮提升最大的项：旧实现每对样本把 `log`/`sqrt` 算两遍、`SinCos` 也算双份；改成两个向量配对后每个 lane 都参与运算，写回也从逐值交错改成向量粒度（每块两条向量存储），所以 128 位从“慢 40%”变成“快 2.25 倍”，512 位到 7.4 倍。正态这两行是写回布局定稿后单独复测的结果（同一协议、同一天），其余场景来自 2026-09-14 的三宽度矩阵。
- 单值入口那一行与改动前是同一段代码，差异都在测量波动范围内（±4%）。
- 向量 API 那一行的对照不是旧实现（旧实现没有同签名 API），而是与候选语义相同的标量 lane 参考实现。

## 正确性、随机质量与 SIMD 证据

- **独立参考**：测试工程里有一份逐 lane 标量参考（自己实现 SplitMix64、Jump、状态转换、乘高位拒绝与区间映射），与生产代码不共享路径。
- **覆盖**：五种 seed 的初始化与 512 轮输出；原始/单位填充的长度 0/1/2/3/`L±1`/`2L±1`/31/32/33/127/128/129/1024；单值/批量双向隔离；空目标与非法区间不消费状态；有界 double 与标量公式逐位一致；正态的双向量配对、尾部、`2*ceil(N/(2L))` 块消费量与相对误差 ≤1e-12。
- **随机质量**：固定 seed、每 lane 2^20 轮，检查每 bit 的 1 计数、每字节桶计数（7σ 预算）与相邻值相关性；正态另有 100 万样本统计。失败不重试、不换种子。
- **SIMD 证据**（FR-006）：批量状态推进全程在向量寄存器里完成，循环左移在支持的平台上用硬件旋转指令。512 位宽度的循环体（本轮实测反汇编摘录）：

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
- `dotnet test tests/Metaheuristics.Tests`：默认、`DOTNET_MaxVectorTBitWidth=128`、`=512`、`DOTNET_EnableHWIntrinsic=0` 四种配置各 169/169 通过；软件路径实测 `Vector<ulong>.Count=2`、`IsHardwareAccelerated=False`。
- `dotnet format --verify-no-changes`：通过。
- `eng/test-documentation-verifier.ps1`：通过；`eng/verify-documentation.ps1` 只剩既有缺口（`SPEC-0008` 空目录，`SPEC-0011`–`SPEC-0014` 缺 Plan/Tasks/Verification），SPEC-0010 自身无错误。
- DocFX：0 警告 0 错误。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `RandomSource` 双状态与 Jump 播种 | `RandomSourceBatchTests.VectorSamplesMatchIndependentJumpSeededLanes` | spec、plan、ADR-0024 | Passed |
| FR-002 | `NextULongVector`/`NextDoubleVector` 与路由 | `UnitVectorMatchesRawVectorMapping`、隔离测试 | spec、plan、XML 注释 | Passed |
| FR-003 | `Fill` 重载的排放、消费轮数与 API 收敛 | `RawFillMatchesReferenceEmissionForAllShapeBoundaries`、`PublicShapeIsClosed` | spec、plan | Passed |
| FR-004 | `StandardNormal.Fill` 的双向量配对与向量数学 | `VectorFillMatchesScalarBoxMullerReferenceAndConsumesWholeRounds`、100 万样本统计 | spec、plan | Passed |
| FR-005 | 单值数值/异常/确定性边界 | `RandomSourceTests`、`EmptyAndInvalidOperationsPreserveStateAndTargets` | spec、plan | Passed |
| FR-006 | 向量内核与 SIMD 证据 | 三宽度测试、软件路径测试、上面的反汇编摘录 | spec、plan、ADR-0024 | Passed |
| NFR-001 | 三宽度性能矩阵 | 本报告的性能表 | spec、plan | Passed |
| NFR-002 | 固定种子随机质量 | `BatchStreamsPassFixedSeedStatisticalChecks`、`FixedSeedMillionSampleStatisticsMeetTheApprovedThresholds` | spec、plan | Passed |

NFR-001 的所有门槛（单值 ≥0.95×、批量 raw/unit ≥1.10× 或宽度 2 ≥0.90×、有界 double 与正态 ≥0.90×、向量 API ≥1.05×、采样 0 B/op）在三个宽度上全部满足。

## 证据与复现

证据只保留 BenchmarkDotNet 的 `-report-github.md` 汇总表：`evidence/width-128/`、`evidence/width-256/`、`evidence/width-512/`，每个目录 13 个文件（12 个对照场景类 + 1 个 JIT 诊断类）。没有脚本、原始日志或反汇编文件。

```sh
dotnet build Metaheuristics.NET.slnx -c Release
dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=128 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=512 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_EnableHWIntrinsic=0 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build

# 性能矩阵：三个宽度各跑一次
DOTNET_MaxVectorTBitWidth=128 dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Rs*Benchmarks*"

# 上面摘录的反汇编（只跑诊断类，不需要保留文件）
DOTNET_MaxVectorTBitWidth=512 dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*RsJitDiagnostics*" --job Short
```
