# T006 成员 `NextRaw()` 与 wrapper/ref 比较

本目录记录用户授权的第二轮内部实现比较。两种方案都只使用同一套 xoshiro256++ 转换、拒绝采样和 53 位 double 映射，未改变公共 API、随机请求顺序或数值语义。

## 比较的两个实现

成员方案在 `RandomSource.NextRaw()` 中读取 `_state0..3` 到局部变量，完成一次转换后写回四个字段；`NextULong()`、所有 Fill 和有界映射都调用这个成员。它的生产实现曾单独存在于本轮工作树，证据中的 `production-*.txt` 是该实现的实际运行时 JIT 输出。

wrapper/ref 方案由 `NextRawFromFields()` 读取和写回字段，再调用唯一的 `NextRaw(ref ...)` 静态转换；Fill 在一次调用中局部化四个状态并循环调用同一静态转换。该方案的正式对照报告见 [`t006-luna-unified/random-source-wrapper-32-128`](../t006-luna-unified/random-source-wrapper-32-128/README.md) 和 [`t006-luna-unified/endpoint-wrapper-32-128`](../t006-luna-unified/endpoint-wrapper-32-128/README.md)。`NextRawFromFields()` 的职责只是状态装载、共享转换调用和状态写回，不是第二份转换算法。

## 环境和门槛

两组报告使用相同的 BenchmarkDotNet 0.15.8 `DefaultJob`、`MemoryDiagnoser`、Apple M4 Arm64、macOS Sequoia 15.7.4、.NET SDK 10.0.400、Runtime 10.0.11。RandomSource 运行 32/128 两个长度的全部 24 个标量/Fill/System.Random 点；每个点的调用级分配均为 `0 B`。端到端使用 `PopulationSize=64`、10 次迭代、Sphere + Clamp(-5,5)、seed `20260905`。

## RandomSource 正式结果

均值单位为 ns；最后一列是成员方案除以 wrapper/ref，低于 1 表示成员更快。Fill 仍全部快于对应 seeded `System.Random` 标量对照，并通过既有门槛，但成员 Fill 每次都读写对象字段，明显慢于局部状态批量推进的 wrapper/ref Fill。

| 长度 | 操作 | 成员 | wrapper/ref | 成员 / wrapper | System.Random 标量 |
| ---: | --- | ---: | ---: | ---: | ---: |
| 32 | Fill ULong | 70.82 | 23.82 | 2.973x | 915.44 |
| 32 | 标量 ULong | 74.19 | 78.41 | 0.946x | 915.44 |
| 32 | Fill unit double | 73.29 | 24.38 | 3.006x | 171.84 |
| 32 | 标量 unit double | 74.01 | 74.89 | 0.988x | 171.84 |
| 32 | Fill bounded double | 76.45 | 27.59 | 2.771x | 178.23 |
| 32 | 标量 bounded double | 77.83 | 79.86 | 0.975x | 178.23 |
| 32 | Fill bounded int | 71.89 | 25.57 | 2.812x | 169.96 |
| 32 | 标量 bounded int | 73.60 | 74.92 | 0.982x | 169.96 |
| 128 | Fill ULong | 301.58 | 92.20 | 3.271x | 3675.39 |
| 128 | 标量 ULong | 308.60 | 314.09 | 0.983x | 3675.39 |
| 128 | Fill unit double | 300.69 | 95.58 | 3.146x | 658.60 |
| 128 | 标量 unit double | 294.43 | 299.99 | 0.981x | 658.60 |
| 128 | Fill bounded double | 319.52 | 111.60 | 2.864x | 698.98 |
| 128 | 标量 bounded double | 320.14 | 320.34 | 0.999x | 698.98 |
| 128 | Fill bounded int | 291.56 | 100.60 | 2.898x | 674.56 |
| 128 | 标量 bounded int | 302.49 | 303.66 | 0.996x | 674.56 |

成员方案的 RandomSource 报告见 [`random-source-member-32-128/results`](./random-source-member-32-128/results/Anastasya.Metaheuristics.Benchmarks.RandomSourceBenchmarks-report-github.md)。wrapper/ref 的完整报告见 [`t006-luna-unified/random-source-wrapper-32-128/results`](../t006-luna-unified/random-source-wrapper-32-128/results/Anastasya.Metaheuristics.Benchmarks.RandomSourceBenchmarks-report-github.md)。

## 端到端正式结果

端到端均值单位为 us；分配与 wrapper/ref 相同。旧生产基线来自 [`t006-rerun-luna/baseline/endpoint-default`](../t006-rerun-luna/baseline/endpoint-default/README.md)，用于既有候选不超过 `1.05x` 的门槛。

| 路径 | 成员 | wrapper/ref | 成员 / wrapper | 旧基线 | 成员 / 基线 | 分配 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Bat 32 | 251.1 | 304.7 | 0.824x | 386.7 | 0.650x | 223.74 KB |
| Bat 128 | 1344.5 | 1386.8 | 0.970x | 2544.4 | 0.528x | 800.52 KB |
| Cuckoo 32 | 57.08 | 56.85 | 1.004x | 59.00 | 0.967x | 44 KB |
| Cuckoo 128 | 171.01 | 169.14 | 1.011x | 164.66 | 1.039x | 140.75 KB |

成员端到端完整报告见 [`endpoint-member-32-128/results`](./endpoint-member-32-128/results/README.md)。四个成员点均通过旧基线 `1.05x` 门槛；wrapper/ref 的完整对照见 [`t006-luna-unified/endpoint-wrapper-32-128/results`](../t006-luna-unified/endpoint-wrapper-32-128/results/README.md)。

## JIT 证据和选择

实际运行时输出使用 .NET 10 Arm64 FullOpts、`COMPlus_TieredCompilation=0` 获取，保存在 [`jit`](./jit/README.md)；没有用源码或耗时替代反汇编。生产成员 `NextULong()` 的调用体把成员转换内联为 76 bytes，生产成员 `Fill(Span<ulong>)` 把转换内联进循环体为 104 bytes；恢复后的生产 wrapper/ref `NextULong()` 也是 76 bytes，而 wrapper/ref `Fill` 在循环外装载/写回状态，输出为 112 bytes。临时诊断类型只用于对照两种内部形状：成员 `Call`/`Fill` 分别是 76/104 bytes、1 个 inlinee；wrapper/ref `Call`/`Fill` 也是 76/104 bytes、2 个 inlinees。标量最终机器码大小相同；wrapper/ref 的静态 helper 没有留下接口、虚调用或 delegate 分派。

最终保留 wrapper/ref 这一份生产转换实现。理由是它在 8 个 RandomSource Fill 主点平均约快 2.8–3.3 倍，而成员只在标量点约快 0.1–5.4%；成员虽然在 Bat 两点更快，Cuckoo 两点与 wrapper/ref 基本持平且略慢，无法抵消 Fill 的稳定回归。成员方案本身通过既有门槛，未因门槛失败而隐藏；选择依据是所有标量、Fill 和端到端路径的合并证据。生产文件随后恢复为 wrapper/ref 唯一实现，未保留成员实现的第二份转换体。
