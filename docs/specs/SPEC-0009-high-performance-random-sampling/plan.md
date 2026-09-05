# SPEC-0009 技术计划

## 元数据

- 状态：`Draft`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：—
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`FR-006`、`FR-007`、`FR-008`、`NFR-001`、`NFR-002`、`NFR-003`、`NFR-004`、`NFR-005`
- 批准人：—
- 批准日期：—

`spec.md` 已于 2026-09-05 由项目作者批准，但尚未进入 Git 提交，因此本 Draft 暂不伪造 Spec 基线 hash。Plan 批准前必须先由用户授权并创建包含 Approved Spec 与 ADR-0020 的文档基线提交，再回填其完整 commit hash。当前生产代码调查基线为 `466115ad90015d5476d890dc5fd93048f8439187`。

## 当前实现调查

- 当前相关类型和入口：`OptimizationRunner.Execute` 接收可选 `int seed = 0`；`OptimizationRunContext` 内部构造 `new Random(seed)` 并公开 `int Seed`/`Random Random`；`OptimizationRunSummary`、Experiment options/group plan/group context/run result/result seed snapshot 均使用 `int`。`ICandidateInitializer.Initialize` 与 `ICandidateRepair.Repair` 直接接收 `System.Random`。
- 当前调用链：Runner 每 run 创建 Context；Optimizer 在 `ResetForRun` 把同一 Context Random 传给 Initializer，并通过 Context 传给 Repair。Bat 在初始化和迭代中消费无界/有界 double；PSO 消费无界/有界 double；Firefly 消费无界 double；Cuckoo 消费 `Next(int)`、无界 double 和私有 Box–Muller。RandomReset Repair 只在有限两侧越界 lane 请求随机数。
- 当前职责所属层：Core Execution 已拥有每 run 随机流生命周期；Core Problems 的 Initializer/Repair 已拥有消费契约；Experiments 拥有共享 repetition seed 排程；Algorithms 拥有随机请求时机和 Cuckoo 专用 Lévy/Box–Muller 逻辑。
- 已存在的相似或重复概念：Bat/Pso 各有一个私有有界 double helper，为相等边界直接返回且不消费随机数；Cuckoo 自行保存 `_hasSpareGaussian`/`_spareGaussian`。后者在本 Spec 中仍有真实生产消费者，直到后续 Cuckoo 批量化 Spec 才删除。
- 当前测试、示例、文档和基准：Core/Algorithms/Experiments 测试大量使用固定 `int` seed；Core Repair 测试直接 `new Random(seed)`；PSO/Firefly 有手写 `System.Random` 期望路径；无单独随机数/分布测试或基准。现有 BenchmarkSwitcher 会传递 filter，已有 Bat、PSO、Firefly、Repair 局部或端到端基准，尚无 Cuckoo 基准。
- 与 Spec 或 ADR 的已知冲突：实现和 ENGINEERING.md 仍表述 `System.Random`/`int` seed；ADR-0009 已被 ADR-0020 替代。ADR-0003 的项目图不变，ADR-0007 的三个运行时包锁步规则不变。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：独立 `Metaheuristics.Random` 程序集 + 公开 `IRandomSource` | 编译器强制底层边界，可外部替换 | 新必选依赖、包版本与接口分派 | 没有脱离 Core 的真实消费者，factory/第三方状态契约超范围 | 否 |
| B：Core 内 public sealed `RandomSource`，直接持有四个 Xoshiro 状态字 | 用户只消费、无接口/虚调用，项目图不变 | Core 承担实现与公共 API；未来替换需新 SDD | 必须阻止算法专用公式进入 Core | 是 |
| C：public sealed facade + internal engine 接口/多实现 | 便于内部切换 | 每次采样分派或 tag switch，增加测试矩阵 | 为无消费者的第二实现预建抽象 | 否 |
| D：保留 `System.Random`，只加 Algorithms 私有批量 helper | 改动小 | 多算法重复分布、不能强类型批量生成 | 随机能力继续分散，无法支撑后续 SIMD | 否 |

### 基础随机实现

1. 在 `src/Metaheuristics.Core/Randomness/RandomSource.cs` 实现 `Anastasya.Metaheuristics.Core.Randomness.RandomSource`。类型 public sealed，只有 internal `RandomSource(ulong seed)`；无公共 factory、接口、基类或后端选择。
2. `RandomSource` 直接保存四个 `ulong` 状态字。构造阶段使用作者 SplitMix64 固定增量和混合常数连续产生四字状态；不使用时间、线程或系统熅。代码来源与已知答案以作者的 [`xoshiro256plusplus.c`](https://prng.di.unimi.it/xoshiro256plusplus.c) 和 [`splitmix64.c`](https://prng.di.unimi.it/splitmix64.c) 为权威参考。
3. 用 `BitOperations.RotateLeft` 实现 `xoshiro256++` 1.0 输出与状态转换。标量成员与 Fill 共享唯一私有状态转换；Fill 把四字复制到局部变量，循环完成后一次写回。允许直接调用私有 helper，不以 JIT 是否内联判定合格。先用基准确定吞吐和分配，再用反汇编解释瓶颈；只有对照实验确认 helper 调用导致未达性能门槛时，才回到 Plan 评估局部展开的收益与重复实现成本，不因残留 call 自动复制转换体。
4. `NextDouble()` 取原始输出的高 53 位并乘 `2^-53`，产生 `[0,1)`。标量与 Fill 可使用不同私有循环，不固化底层 raw-word 消费对应。
5. `NextInt(minimum, maximum)` 与其 Fill 将宽度在 64 位中计算，使用基于乘高位+拒绝阈值的无偏映射，覆盖 `int.MinValue` 到 `int.MaxValue` 的所有非空半开范围；不使用简单 `% width`。测试中的独立参考映射不复用生产 helper。
6. 有界 double 在进入随机状态或写目标前完成参数验证。宽度有限时使用 `minimum + ((maximum - minimum) * u)`；宽度溢出时使用加权端点的防溢出插值。最终结果向下/向上限制到 `[minimum, Math.BitDecrement(maximum)]`，防止舍入越界或命中上界。
7. 非有限 double 端点抛 `ArgumentOutOfRangeException`；有限但 `minimum >= maximum` 的 double 和 int 范围抛 `ArgumentException`。有界 Fill 先验证参数再检查空 Span，因此空目标仍拒绝无效范围，但无状态推进和写入。

### 标准正态选择

1. 在 `src/Metaheuristics.Core/Randomness/StandardNormal.cs` 实现批准的 public static `StandardNormal.Sample(RandomSource)` 与 `Fill(RandomSource, Span<double>)`；两个入口对 null random 抛 `ArgumentNullException`，空 Fill 不消费状态。
2. 默认生产实现为无跨调用 spare 的 Box–Muller：`Sample` 使用 `(0,1]` 径向输入并丢弃第二样本；Fill 在单次调用内成对生成并处理奇数尾部。另在 benchmark/test 私有代码中保留独立参考，不增加第二公共实现。
3. 可评估的优化候选为 Marsaglia–Tsang [Ziggurat 标准正态采样](https://www.jstatsoft.org/v05/i08/)，表和尾部公式来自论文/作者参考，但原始随机 bit 来自 `RandomSource.NextULong`。常量表在编译期固定，首次或每次调用不运行生成和不产生托管分配；测试核对表的单调、边界和面积不变量。
4. Ziggurat 必须通过固定 seed 统计门槛、特殊路径测试、零调用级分配，以及“性能基准与准入”中的批量收益和标量回归门槛，才能替代默认 Box–Muller。收益不足、噪声无法区分或任何正确性门槛失败时，删除候选表/代码并保留 Box–Muller；不保留运行时切换。
5. 固定 seed 统计测试使用 1,000,000 个样本，至少要求 `|mean| <= 0.005`、`|variance - 1| <= 0.01`、中位数绝对值不超过 `0.01`，1%/99% 样本分位分别位于 `[-2.40, -2.25]`/`[2.25, 2.40]`，且 `|x| > 3` 的样本数位于 `[2400, 3000]`。测试数据完全由固定 seed 决定，不重试、不依赖时钟；阈值同时应用于最终候选与 Box–Muller 参考以验证测试本身。

### Core、Algorithms 与 Experiments 迁移

1. `OptimizationRunContext`、`OptimizationRunner.Execute`、`OptimizationRunSummary`、`ICandidateInitializer`、`ICandidateRepair` 和所有 Core Repair 签名一次性改为 `RandomSource`/`ulong`；不保留 `int` 重载、`System.Random` 适配器或双字段。
2. Core 增加只对 `Metaheuristics.Tests` 与 `Metaheuristics.Benchmarks` 的 `InternalsVisibleTo`，使仓库测试/基准能从 seed 构造 `RandomSource`；不增加 public test factory 或第二 PRNG。实现阶段在 Developer Guide 提供外部策略测试示例，并以没有 friend 权限的临时独立项目编译和运行：只引用 Core，调用公共 Runner 执行自定义测试 Optimizer，在 `ResetForRun` 内用 `context.Random` 调用待测 Initializer/Repair，完成合法候选的 Repair/Evaluate；使用停止条件结束 run，仅带出结果快照。相同 seed 的两个独立 run 验证结果重复一致，示例不得使用 internal 构造、反射或把 Context/随机源带出 run。该项目只作验证，不新增运行时项目或公共测试 API。
3. Bat/Pso 保留现有“相等边界直接返回且不消费随机数”分支，非相等时调用 `RandomSource.NextDouble(minimum, maximum)`。按照 FR-007 明确采用上界舍入修正；新增 `[1, Math.BitIncrement(1))` 等相邻端点测试，核对结果排除上界，同时独立验证相等边界不推进状态。两者现有对溢出宽度的 Options 拒绝规则保持，不因新通用 API 放宽算法配置。
4. Cuckoo 的 `Random.Next(maximum)` 机械迁移到 `NextInt(0, maximum)`；私有 Box–Muller、spare 状态和所有随机请求顺序保持，本 Plan 不用新 `StandardNormal` 替换它。
5. RandomReset 保持现有请求条件：只处理有限双侧越界 lane；宽度溢出时继续 Clamp 且不消费随机数；相等有限边界且 lane 越界时继续消费一次无界 `NextDouble()` 后写回该边界。不直接调用会拒绝相等边界的新有界 API。
6. Experiments 将 `BaseSeed`、`Seeds`、`RunGroupPlan.Seeds`、`ExperimentGroupContext.Seeds`、`ExperimentRunResult.Seed` 和 `ExperimentResult.Seeds` 统一改为 `ulong`/`ulong[]`/`IReadOnlyList<ulong>`。`ResolveSeeds` 先复制显式列表；自动派生精确使用 `unchecked(baseSeed + (ulong)repetitionIndex)`，不调用 Randomness helper。
7. 所有示例、XML、API overview、User/Developer Guide、ENGINEERING.md 和架构概览同步 `RandomSource`、`ulong`、封闭所有权与无新项目依赖。不把具体 Xoshiro 类型暴露为用户构造入口。

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| run seed 与随机源创建 | Core Execution + `System.Random` | Core Execution + Core Randomness | Runner 拥有每 run 生命周期。 |
| PRNG 状态和基础分布 | `System.Random`/算法 helper | Core Randomness `RandomSource` | 统一封闭实现和批量能力。 |
| 标准正态 | Cuckoo 私有 Box–Muller | Core Randomness 新增通用 `StandardNormal`；Cuckoo 暂保留旧消费者 | 先建通用能力，后续算法 Spec 再改生产请求顺序。 |
| repetition seed 排程 | Experiments 32 位置换 | Experiments 64 位模加法 | Experiment 只关心稳定编号，不了解 PRNG。 |
| 算法请求时机 | 各 Optimizer | 不变 | 本 Spec 只做能力与签名迁移。 |
| 用户随机后端 | 无 | 无 | 用户只消费 Context 提供的封闭源。 |

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| `ulong seed` | Runner/Experiment 调用边界 | 每 run/每计划一次 | 否 | 所有值合法，不隐式混入环境熅。 |
| Xoshiro 状态 | internal constructor + 独立已知答案测试 | 每源创建/测试 | 构造非热路 | 四字状态正确展开、不是全零。 |
| double 端点 | `NextDouble(min,max)`/Fill 入口 | 每调用一次 | Fill 外层 | 非有限/空/反向范围在状态和目标变化前失败。 |
| int 端点 | `NextInt`/Fill 入口 | 每调用一次 | Fill 外层 | 空/反向范围失败，宽度无符号计算不溢出。 |
| 拒绝映射候选 | `RandomSource` 私有循环 | 按需 | 是 | 无模偏差；状态消费取决于拒绝，不对外承诺 raw-word 数量。 |
| StandardNormal random | 公共入口 | 每调用 | 是 | null 在消费前失败；空 Fill 无状态变化。 |
| 显式 seed 列表 | Experiment `ResolveSeeds` | 每 Experiment 一次 | 否 | 列表必须覆盖最大 repetitions，启动前复制。 |
| Initializer/Repair/Optimizer | 各现有调用边界 | 每旧规则 | 是 | 随机请求条件、Repair/Evaluate 时点和取消不漂移。 |

## API 与行为变化

- 新增：Core `Randomness.RandomSource`、`Randomness.StandardNormal`；`RandomSource` 的八个公共标量/Fill 入口。
- 修改：Context Random、Initializer/Repair 随机参数和全部 run/Experiment seed 类型改为 `RandomSource`/`ulong`。
- 删除：公共运行时签名中的 `System.Random`、公共/plan/result 中的 `int` seed、Bat/Pso 非相等范围的手写线性映射。
- 破坏性变化：现有 Initializer/Repair/自定义 Optimizer/单次 Runner/Experiment 调用方必须重新编译并迁移签名和 seed 常量；固定 seed 的具体搜索轨迹改变。Bat/PSO 非相等有界 double 还会修正旧公式舍入到上界的结果，这项数值变化独立于 PRNG 替换。
- 调用方迁移方式：随机参数类型改为 `RandomSource`；seed 常量使用 `UL` 或可隐式转换的非负整数常量；负 seed 必须由调用方选择明确的 `ulong` 值，库不定义符号映射。
- 明确保持不变的行为：每 run 独立流、Group 隔离、调度无关 seed、Optimizer 复用与异常后丢弃、Repair/Evaluate/取消时点、算法循环和工作区。

## 替代与清理计划

- 被取代的类型、接口和入口：`System.Random` 在 Core/Algorithms 契约中的位置；所有 `int` seed 签名、字段和集合；Experiments 32 位置换派生函数。
- 必须删除的转发层、兼容壳和重复抽象：不增加 `IRandomSource`、public Xoshiro、public seed factory、`System.Random` adapter、`int` overload、双随机字段、多 engine switch 或旧新随机运行时开关。
- 必须删除或改写的旧测试：依赖 `new Random(seed)` 的 Repair/算法期望路径；依赖负 seed 或旧 32 位派生数值的断言；保留并改写定性、Group 隔离和复用契约测试。
- 必须更新的示例和文档：Examples、README/API overview/User Guide/Developer Guide、ENGINEERING.md、architecture overview、XML API 注释、ADR/spec 索引、Verification。
- 全仓库残留搜索方式：`rg "System\\.Random|\\bRandom\\b|int seed|IReadOnlyList<int> Seeds|int\\[\\] Seeds|DeriveSeed\\(int|IRandomSource|Xoshiro256PlusPlus|RandomSourceFactory|NextUInt64|NextInt32|FillUniform" src tests benchmarks examples docs ENGINEERING.md`；逐项区分合法的历史/Benchmark `System.Random` 基线和禁止的生产残留。
- 保留旧结构时的真实消费者、期限和删除条件：Cuckoo 私有 Box–Muller/spare 继续服务未批量化的生产循环，直到后续 Cuckoo 随机布局 Spec 批准替代；Benchmark 私有 `System.Random`/Box–Muller 只作为 NFR-003 历史对照，不被运行时项目引用。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 是 | Randomness 新 API、Context/Runner/Summary、Initializer/Repair 签名 | Core 单元/API/集成测试、JIT、分配 |
| Algorithms | 是 | 四种算法及初始化器接线，Bat/PSO 上界舍入修正；不批量化生产循环 | 固定 seed 复现、相邻端点、调用时机、全部算法测试 |
| Experiments | 是 | seed 全链改 `ulong`，派生简化为模加法 | Group 布局、碰撞、显式列表、取消/失败测试 |
| Examples | 是 | seed 常量与 Initializer 签名 | Release build/运行审查 |
| Tests | 是 | 新增 RandomSource/StandardNormal，迁移旧测试 | 定向与完整 Release test |
| Benchmarks | 是 | 新增随机/正态/Cuckoo 端到端基准，迁移旧基准 | BDN report/MemoryDiagnoser/DisassemblyDiagnoser |
| XML 文档 | 是 | 新 API 及所有随机/seed 签名 | DocFX/公开 API 审查 |
| 用户/API 文档 | 是 | 封闭所有权、批量语义和迁移说明 | 文档验证/残留搜索 |
| ENGINEERING | 是 | `System.Random` 替换为 Core-owned `RandomSource`/`ulong` | 文档验证 |
| ADR | 是 | ADR-0020 替代 ADR-0009；ADR-0003/0007 保持 | ADR 状态/索引/链接 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | API reflection/compile tests；无 friend 权限的外部策略示例编译/运行；公共 ctor/interface/factory 残留搜索 | API/集成/审查 | 只有 public sealed `RandomSource`，外部策略可通过公共 Runner 在 run 内验证。 |
| FR-002 | seed 0/Max/相邻 seed 的 SplitMix64 + Xoshiro 已知答案；10,000 步独立参考差分 | 单元 | 四状态字和输出逐位一致。 |
| FR-003 | 八个 API 的正常/空/极值/非法范围/异常前状态与目标不变；int 参考映射 | 单元/API | 半开范围、无偏、防溢出、无部分写入。 |
| FR-004 | 相同精确调用序列重复测试；不断言 Fill/标量/切分等价 | 单元 | 只固化批准的弱兼容。 |
| FR-005 | StandardNormal API/null/empty/重复序列、统计门槛、Ziggurat 特殊路径 | 单元/统计 | 标准正态正确、无跨调用状态。 |
| FR-006 | Core/Experiment `ulong` API 编译、显式/派生 seed 和 Group 布局 | API/集成 | 全链 64 位，无 `int` 兼容壳。 |
| FR-007 | Bat/PSO 相邻和相等端点；四算法/Repair 定性、随机请求条件、评估与取消 | 单元/集成 | 上界舍入按批准规则修正，请求与生命周期时机不漂移。 |
| FR-008 | 生产源码/工作区残留审查 | 审查 | 无 Bat/Cuckoo Fill 缓冲、请求重排或新 SIMD。 |
| NFR-001 | 并发 Group、连续 run、异常、取消和状态隔离 | 集成 | 每 run 独占、同序列重复一致。 |
| NFR-002 | 已知答案、无偏参考、正态矩/分位/尾部 | 单元/统计 | 状态转换与分布达到预定阈值。 |
| NFR-003 | `RandomSourceBenchmarks`、`StandardNormalBenchmarks`、Bat/Cuckoo 32/128D end-to-end | Benchmark | 比率、置信区间、分配、环境与原始 artifact。 |
| NFR-004 | DisassemblyDiagnoser + 源码审查，关联 NFR-003 实测结果 | JIT/审查 | 无随机源接口/虚/factory/delegate 分派；私有直接调用用于瓶颈分析，不单独判失败。 |
| NFR-005 | public API diff + 禁止词/项目图搜索 | API/构建 | 无超范围分布、PRNG、项目或兼容层。 |

## 性能基准与准入

本节命令只是 Draft Plan 提案。在 Plan 获得用户批准前不运行 BenchmarkDotNet；批准后先在独立临时 worktree 的生产代码基线 `466115ad90015d5476d890dc5fd93048f8439187` 上运行端到端基准，再以相同机器、Runtime、BenchmarkDotNet 配置和参数运行候选。

拟批准命令：

```powershell
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*RandomSourceBenchmarks*"
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*StandardNormalBenchmarks*"
dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*BatRandomMigrationBenchmarks*" "*CuckooRandomMigrationBenchmarks*"
```

基准设计与门槛：

- `RandomSourceBenchmarks` 覆盖长度 0、1、2、7、8、31、32、33、127、128、129；报告 `RandomSource` 标量循环、Fill 与 seeded `System.Random` 对应标量循环。完整 `ulong` 的 System.Random 对照使用两次均匀 32 位 `NextInt64(1L << 32)` 组合；有界 int 用 `Random.Next(min,max)`；double 用 `Random.NextDouble()`/同一线性范围映射。超宽 double 只做正确性测试，不与会溢出的旧公式做性能门槛。
- 长度 32 和 128 下，`ulong`、`[0,1)` double、常用有界 double 与有界 int 的 `RandomSource.Fill` 平均时间均必须低于对应 seeded `System.Random` 标量循环；任一主要点未达到即不得宣称 NFR-003 完成。
- `StandardNormalBenchmarks` 比较生产候选、同一 `RandomSource` 的无跨调用 spare Box–Muller 和当前 Cuckoo `System.Random`+跨调用 spare Box–Muller 参考。除 Fill 长度矩阵外，单独测量 `Sample`，不以 `Fill(1)` 代替标量入口。Ziggurat 的正确性和分配门槛通过后，还必须在两次独立完整 BDN 执行中均满足：32/128 Fill 的同源 Box–Muller 耗时除以候选耗时至少为 `1.15x`，且每个点的候选时间 95% 置信区间上界低于参考下界；`Sample` 平均时间不超过同源 Box–Muller 的 `1.05x`。两次结果均保留，不挑选最佳一次；噪声无法区分或任一门槛失败时保留 Box–Muller。该门槛只决定是否值得维护额外表和尾部逻辑，不构成跨环境性能承诺。
- 所有随机标量和 Fill 方法的 BenchmarkDotNet `Allocated` 必须为 `0 B`。类型初始化和 benchmark setup 分配单独记录，不混入调用级数据。
- Bat/Cuckoo 在 32/128 维、固定 population/迭代/目标下各运行旧/新端到端基准。因 PRNG 变化会改变轨迹，它们不证明算法等价；作为 Core 集成回归门槛，候选在四个主要点的平均时间不得超过同机基线的 `1.05x`。同时报告评估次数、分配和轨迹差异，不将单机数据外推。
- DisassemblyDiagnoser 用于核对 Fill 状态推进内循环没有随机源接口、虚成员、delegate 或 factory 分派，并解释实测瓶颈；不以动态 PGO 偶然去虚化作为封闭路径证据。私有 helper 的直接 call 可以保留，验收依据为正确性、吞吐和分配；仅发现未内联不足以授权复制状态转换。

## 风险和回退

- 最大实现风险：Xoshiro/SplitMix 位操作转写错误；整数拒绝映射在全 `int` 宽度下溢出或有偏；超宽 double 舍入命中上界；Ziggurat 表/尾部误差；机械迁移中改变 RandomReset、Bat/Pso 相等边界或 Cuckoo spare 的消费时机。
- 可能产生的行为漂移：所有固定 seed 数值轨迹预期改变；Bat/PSO 有界 double 按 FR-007 修正舍入上界；非法有界入口新增显式异常；Fill 与标量/切分不等价；新正态组件与 Cuckoo 私有正态暂时产生两条不同轨迹。
- 如何及早发现：先落地独立参考/已知答案测试，再实现标量与 Fill；参数异常使用双实例比较下一输出；逐个迁移 Core、Algorithms、Experiments 并运行定向测试；Ziggurat 在成为生产路径前通过统计和 BDN 门。
- 回退：Ziggurat 失败时删除其生产代码/表并使用批次内 Box–Muller；Fill 优化失败时保留正确公共 API，删除未达门槛的循环展开/局部化候选并返回 Spec 处理 NFR-003，不伪称验证完成。不保留旧新 PRNG 运行时开关。
- 退回 Spec 澄清的条件：需要用户构造/替换随机源；需要第二 PRNG、新程序集、状态导入导出、跨 Fill 切分等价、新分布或改变正态公共形状；必须重排 Bat/Cuckoo 随机请求或改变 Repair/Evaluate 时点；需要保留 `int`/`System.Random` 兼容入口。

## ADR 判断

- 是否触发 ADR：是。
- 判断依据：随机源类型、seed 宽度、Context 所有权、确定性和热路径策略是跨功能长期决策；ADR-0009 明确指定 `System.Random`。
- 新 ADR 或被替代 ADR：[ADR-0020](../../decisions/0020-core-owned-random-source-and-run-execution.md) 已 Accepted 并替代 ADR-0009；ADR-0003/0007 保持 Accepted。

## 批准记录

- 计划批准：—
- 批准日期：—
