# SPEC-0014 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)；Plan：[`plan.md`](./plan.md)；Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-09-17
- 最终结果：`Passed`
- 基线 A：`9b276e5`；历史 H：`56c1f57` 加 `16b66fb` 的精确 `CopyTo` 修订；候选 C：当前工作区

## 需求覆盖与实现

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `UpdateFireflyPositionFromUnitSamples` 与逐移动 Repair 路径 | attractor 顺序、Repair 后距离、零项、特殊值和尾部测试 | 本报告完整操作说明 | Passed |
| FR-002 | `FireflyOptimizer` 每次合格移动的 `Fill(D)` 与 `_randomWalk` 复用 | 状态推进、无移动、复用、取消/异常和隔离测试 | 本报告采样与分配说明 | Passed |
| NFR-001 | 最终融合生产路径 | 正式局部、Vector128、H/A/B/C 与分配基准 | 本报告全部性能表 | Passed |

旧 `UpdateFireflyPosition` runtime 入口和无消费者测试/基准已删除。局部基准把位置读入、吸引项、随机缩放、求和与最终单次写回作为一次操作。

## 局部性能

正式配置：2 launches、8 warmups、20 measured iterations、MemoryDiagnoser；单位 ns。

| D | B 标量缩放+旧向量位置 | C 融合 | 加速比 |
| ---: | ---: | ---: | ---: |
| 32 | 26.44 | 16.11 | 1.64× |
| 128 | 70.00 | 27.30 | 2.56× |

受限 Vector128（Count=2，仅 Vector128=True）：D=32 `29.24/20.71=1.41×`，D=128 `100.85/58.45=1.73×`，通过 `>=1.00×`。

## 完整 run：H/A/B/C

单位 µs；Sphere、Clamp(-5,5)、P=64、seed `20260905`。

| 生命周期 | D/代 | H | A | B | C | H/A | A/B | B/C | A/C | H/C |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 首次 | 32/10 | 1167 | 1172 | 907.0 | 666.1 | 1.00 | 1.29 | 1.36 | 1.76 | 1.75 |
| 复用 | 32/10 | 1164 | 1175 | 859.9 | 654.2 | 0.99 | 1.37 | 1.31 | 1.80 | 1.78 |
| 首次 | 32/100 | 11577 | 11654 | 9002.8 | 6600.5 | 0.99 | 1.29 | 1.36 | 1.77 | 1.75 |
| 复用 | 32/100 | 11561 | 11522 | 8555.2 | 6523.5 | 1.00 | 1.35 | 1.31 | 1.77 | 1.77 |
| 首次 | 128/10 | 3360 | 3382 | 2391.3 | 1634.0 | 0.99 | 1.41 | 1.46 | 2.07 | 2.06 |
| 复用 | 128/10 | 3386 | 3382 | 2450.8 | 1624.2 | 1.00 | 1.38 | 1.51 | 2.08 | 2.08 |
| 首次 | 128/100 | 33643 | 33620 | 23748.3 | 16194.9 | 1.00 | 1.42 | 1.47 | 2.08 | 2.08 |
| 复用 | 128/100 | 33491 | 33576 | 24497.5 | 16216.7 | 1.00 | 1.37 | 1.51 | 2.07 | 2.07 |

所有 A/B、B/C 与 A/C 主要点通过。A 与 C 的首次、复用分配实质相同（首次约 64.09/161.59 KB，10/100 代复用约 20.32/200.3 KB）；实现复用已有 `_randomWalk`，局部内核为 0 B。

## 正确性、残留与工程结论

- 测试覆盖多个严格更优 attractor 的顺序、相等/较差不移动、Repair 后位置的下一次距离、零 random step、尾部、特殊值、固定 seed、复用、取消/异常和 Group 隔离。
- 搜索确认逐维 `NextDouble` 移动采样、跨移动预取、额外 scratch、旧 `UpdateFireflyPosition` 和生产双路径均不存在。
- 正式局部产物位于 `%LOCALAPPDATA%/Temp/MetaheuristicsNetBench0011-01a0ad0a/BenchmarkDotNet.Artifacts/spec0014-formal` 和 `spec0011-0014-vector128`；H/A/B/C 分别位于四个 `MetaheuristicsNet0011{HistoricalH,BaselineA,IntermediateB,CandidateFinal}` 临时源副本的 `BenchmarkDotNet.Artifacts/spec0011-0014-*-e2e`。表中报告 Mean；原始报告保留 Error、StdDev、Median 和分配列。
- Restore、Release Build、222 项测试、生成器测试、格式、文档验证器、DocFX 与 `git diff --check` 的最终结果见完成审计；均为 Passed。

SPEC-0014 的全部 FR/NFR 已满足；固定工作量结果不构成收敛速度声明。
