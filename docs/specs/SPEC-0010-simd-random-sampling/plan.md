# SPEC-0010 技术计划（随机源自身性能口径）

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)，2026-09-08 自适应 Vector 修订稿（Approved）
- Spec 基线提交：`56c1f57340e74060b938fe2e431ff77b948328da`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`FR-006`、`NFR-001`、`NFR-002`
- 批准人：项目作者
- 批准日期：2026-09-12

2026-09-12 项目作者指示“继续实现”，并给出本轮边界：实现前先考虑如何压低内存访问与计算次数；性能只比较随机源自身，且要覆盖 128/256/512 三种向量宽度；`verification` 用直白语言写；中间脚本与临时文件全部删除，只留报告需要的部分；本轮不做标量与向量随机源在元启发算法里的对比。本计划按该指示制定并作为实施依据。

## 当前实现调查

- `src/Metaheuristics.Core/Randomness/RandomSource.cs` 目前只有一个四字 `ulong` 状态；单值入口与全部 `Fill` 共用它，`Fill` 在循环外局部化四字状态。
- `src/Metaheuristics.Core/Randomness/StandardNormal.cs` 的 `Sample` 与 `Fill` 都走标量 `NextPair`。
- `RandomSourceTests` 固定了单值已知答案、边界、异常与运行隔离；这些行为属于 SPEC-0009，必须逐位保留。
- 基准项目已经以 `BenchmarkSwitcher` 发现基准，可直接承载本轮对比；不需要额外的临时项目或分析脚本。
- 本机为 AMD Ryzen 7 9800X3D、Windows 11、.NET 10.0.11，`Vector.IsHardwareAccelerated=True`，AVX-512 可用。默认 `Vector<ulong>.Count=4`。

## 本轮范围

1. 依 Spec 实现双状态随机源与两个向量采样入口，并让四个 `Fill` 与 `StandardNormal.Fill` 走批量状态。
2. 按下列“内存与计算”设计实现热路径。
3. 只用基准比较随机源自身：基线 A（SPEC-0009 单状态实现）、候选 B、标量参考 R，覆盖 128/256/512 位向量宽度。
4. 不做算法层端到端对比；SPEC-0011 至 SPEC-0014 的运行时代码本轮不改。

## 内存与计算设计

目标是让每个样本的**状态内存访问接近零、映射运算次数最小**，而不是简单地“用了向量”。

| 决策 | 目的 |
| --- | --- |
| 单值与批量各四个状态；批量状态为 `Vector<ulong>` 字段 | 两套状态互不推进；单值序列与 SPEC-0009 逐位相同 |
| `NextRaw(ref,ref,ref,ref)` 只做一轮转换，调用方在进入/退出时各触碰字段一次 | 循环内状态常驻寄存器；状态内存流量由每轮一次降到每调用一次 |
| 完整轮用 `StoreUnsafe` 直接写入目标 span，4 轮展开 | 每轮一次连续写；减少循环控制指令；不留中间数组 |
| 不满一轮的尾块先整轮推进，再经调用内栈缓冲一次写入 | 避免对寄存器中的向量逐 lane 取值造成的重复栈往返；仍满足“整轮消费、轮尾丢弃” |
| 有界 double 的区间换算在调用前一次性构造成向量，循环内只做仿射与上下界修正 | 每轮样本只做 3 次向量运算；端点语义与标量公式一致 |
| 有界整数沿用乘高位 + 拒绝阈值，且按 lane 顺序检查、拒绝就取下一个 | 与 SPEC-0009 相同的无偏映射与相同的原始字消费顺序 |
| 正态每块只取一次 `NextDoubleVector`，半径/角度/`Log`/`SinCos`/`Sqrt` 都在向量上完成 | 分布公式不进 PRNG；尾块仍走向量数学 |
| 正态输入重排用调用内栈缓冲，`Vector<T>` 没有 lane 重排 API | 只让重排走标量，超越函数保持向量化 |
| 构造按 Spec 执行 L 次 Jump，成本单独测量 | 初始化是规范要求的行为，不用懒初始化掩盖 |

明确不做：不使用 `Vector128/256/512` 或 ISA 专属 API，不引入 `BatchCursor`/`BeginBatch`，不做跨调用原始字缓存，不为某一种宽度写专用后端。

## 验证设计

### 宽度与实现

- 宽度：同一二进制分别以 `DOTNET_MaxVectorTBitWidth=128`、`256`、`512` 运行，对应 `Vector<ulong>.Count` 为 2/4/8。`DOTNET_PreferredVectorBitWidth` 只能在上限内下调，单独设 512 无效，因此统一用上限设置并在报告里记录实测 `Count`。
- A：SPEC-0009 单状态实现（提交 `56c1f57`）在基准程序集内的副本；B：本次候选；R：与 B 语义相同、但每 lane 用标量状态推进的参考实现。
- 每个场景一个基准类，`Baseline=true` 固定在本场景的对照上，因此 BenchmarkDotNet 的 `Ratio` 列就是候选相对该对照的倍数（小于 1 表示候选更快）。

### 自动化测试

| 需求 | 测试 |
| --- | --- |
| FR-001 | 五种 seed 下 lane 状态与独立参考一致；lane 互不相同且非全零；批量调用不推进单值状态 |
| FR-002 | 两个向量入口的结果与互不推进；单位向量等于同轮原始值的 53 位映射 |
| FR-003 | 长度 0/1/2/3/L±1/2L±1/31/32/33/127/128/129/1024 的排放与后续状态；有界整数拒绝顺序；宽度 1 区间仍消费原始字 |
| FR-004 | 正态尾块、相邻 lane 配对、与逐 lane 参考的容差比较、恰好消费 `ceil(N/L)` 轮 |
| FR-005 | 单值已知答案、半开区间、异常类型与失败后两套状态不变 |
| FR-006 | 三个宽度的全量测试；`DOTNET_EnableHWIntrinsic=0` 软件路径测试；反汇编证据 |
| NFR-002 | 固定种子统计与相关性检查；不通过不换种子重试 |

### 性能口径与门槛（事前固定）

测量方式：Release、BenchmarkDotNet 0.15.8、同一进程内同参数对照、`MemoryDiagnoser`、固定 seed。长度 32 与 128；有界区间 `[-5,5)`。每次操作分配必须为 0 B。

| 层级 | 门槛 |
| --- | --- |
| 单值入口（`NextULong`/`NextDouble`/`NextInt`/`StandardNormal.Sample`） | 相对 A 不低于 0.95，且不做向量化改造 |
| 批量 raw/unit `Fill` | 宽度 4 与 8：相对 A 至少 1.10；宽度 2：相对 A 不低于 0.90 |
| 有界 double/int、`StandardNormal.Fill` | 各宽度相对 A 不低于 0.90 |
| 两个向量 API | 相对标量参考 R 至少 1.05 |
| 构造 | 只报告时间与字节，不设吞吐门槛 |

宽度 2 的门槛低于宽度 4/8 是本机硬件的结构限制：128 位宽度下 64 位整数向量运算的吞吐并不比标量优，而 `Vector<T>` 的循环左移需要“两次移位 + 或”，标量侧只有一条 `rol`。该差异在实施前记录，用于解释而不是掩盖结果。

## 连带影响与风险

| 区域 | 影响 | 处理 |
| --- | --- | --- |
| Core Randomness | 双状态、批量路由、正态向量数学 | 独立参考与隔离测试 |
| Algorithms / Experiments | 批量消费轨迹随宽度改变；代码不变 | 全量测试与固定 seed 复现 |
| Tests | 新增批量参考与契约测试 | 三个宽度与软件路径各跑一次 |
| Benchmarks | 新增对照与 JIT 诊断文件 | 报告只保留 `-report-github.md`/`-asm.md` 与命令记录 |
| 文档 | Spec、Plan、Tasks、Verification、ADR | 与实现同步 |

主要风险：宽度 2 无加速、构造随宽度变贵、批量序列不再与单值流等价。三者都在 Spec 的边界内，报告需直白说明。

## 验证命令

```sh
dotnet build Metaheuristics.NET.slnx -c Release
dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_EnableHWIntrinsic=0 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=128 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
DOTNET_MaxVectorTBitWidth=512 dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build
dotnet format Metaheuristics.NET.slnx --verify-no-changes --no-restore
DOTNET_MaxVectorTBitWidth=<128|256|512> dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Rs*Benchmarks*"
```

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-09-12
- 批准内容：按 2026-09-12 指示实施已批准的 SPEC-0010，并以本计划的随机源自身口径、三宽度矩阵与门槛验收。

## 2026-09-12 测量结果（计划执行后回填，不修改上面的门槛）

- 256 位与 512 位：批量原始/单位填充 1.43×–3.64×、有界 double 1.96×–4.00×、有界 int 1.04×–1.10×、正态 1.08×–1.35×，单值入口 ±3%，全部门槛通过。
- 128 位：批量原始/单位填充 0.72×–0.79×、正态 0.60×，低于本文“宽度 2 不低于 0.90”的门槛；有界 double/int 与单值入口通过。原因与反汇编证据见 [`verification.md`](./verification.md)。
- 因此 NFR-001 记为 `Failed`，Spec 停在 `Implementing`。要改变 128 位结论必须先修订 Spec/ADR-0024（例如允许该宽度使用标量 lane 推进），该决定留给项目作者。

## 2026-09-13 ISA 旋转指令授权

项目作者要求 `RotateLeft` 加入 ISA 探测：宽度匹配且指令集支持时使用硬件旋转指令，不支持的平台回退到“两次移位 + 或”，并接受“`if` 条件是 JIT 常量、未选中分支被消除”的零开销前提。实现落在 AVX-512F（512 位 `vprolq`）与 AVX10.1（128/256 位）；ARM64 的 NEON/SVE/SVE2 没有 64 位 lane 的通用旋转指令，继续走回退。该授权放宽了 FR-006/ADR-0024 原先“不使用 Vector128/256/512 或 ISA 专属 API”的边界：只允许 ISA 门控的固定宽度旋转指令，其余固定宽度与 ISA 专属用法仍然禁止，且必须保留通用回退。
