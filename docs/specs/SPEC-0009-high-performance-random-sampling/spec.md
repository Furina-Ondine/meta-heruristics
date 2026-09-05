# SPEC-0009：高性能随机源与批量分布采样

## 元数据

- 编号：`SPEC-0009`
- 状态：`Approved`
- 创建日期：2026-09-03
- 批准人：项目作者
- 批准日期：2026-09-05
- 替代：现有运行时公共契约中由 `System.Random` 承担的随机源与 `int` seed 行为；长期决策由 ADR-0020 记录。
- 被替代：无
- 相关 ADR：[ADR-0003](../../decisions/0003-project-and-package-boundaries.md)、[ADR-0014](../../decisions/0014-spec-driven-change-governance.md)、[ADR-0020](../../decisions/0020-core-owned-random-source-and-run-execution.md)；本变更保持现有项目边界，ADR-0020 替代 ADR-0009 并记录 Randomness 职责、随机源所有权与 64 位 seed 迁移。

## 问题与动机

当前 Core 为每个 run 从显式 `int` seed 创建独立 `System.Random`，并通过 `OptimizationRunContext`、`ICandidateInitializer` 与 `ICandidateRepair` 把同一随机流传给运行时组件。该模型保证 RunGroup 隔离和调度无关的 seed，但只向算法提供逐次随机调用，无法直接向调用方拥有的 Span 批量生成均匀整数、均匀浮点或标准正态样本。

Bat 的初始化与候选更新每维多次调用均匀随机数；Cuckoo 的 Lévy 候选每维消费标准正态与均匀样本，并由算法私有代码实现 Box–Muller 状态。逐元素随机调用限制了随机数生成本身的吞吐，也使算法难以先批量准备随机样本、再以现有算法私有 SIMD 路径处理规则算术。现有 SPEC-0005 明确没有修改 Bat/Cuckoo 的随机源或生产循环，因此该能力不能作为既有 SIMD 工作的隐含扩展。

需要在 `Metaheuristics.Core` 中新增边界清晰的 Randomness 子系统，提供由 Core 构造并密封的公共 `RandomSource`、内部 `xoshiro256++` 状态实现、标量与批量基础均匀采样，以及独立的标准正态采样组件。第一阶段同时把 Core、Experiments 和现有策略签名从 `System.Random`/`int` seed 一次性迁移到 `RandomSource`/`ulong` seed，但不重排 Bat、Cuckoo 或其他算法的随机请求阶段；算法批量化和随机轨迹选择由后续独立 Spec 决定。

## 目标

- 在 `Metaheuristics.Core` 内新增独立目录和命名空间的 Randomness 子系统，不新增运行时项目、程序集或 NuGet 包。
- 提供公共密封 `RandomSource`；它只能由 Core 内部从 run seed 构造，内部固定使用 `xoshiro256++`，不提供公共随机源接口、继承点或实现选择器。
- 使用 `ulong` 作为随机源、Runner、Experiment 计划和结果中的统一 seed 类型，并删除旧 `int` seed 公共契约。
- 提供完整范围 `ulong`、`[0, 1)` 均匀 `double`、有界均匀 `double`、无偏有界 `int` 的标量与 `Span<T>` 批量入口。
- 以独立于 `RandomSource` 的公共 sampler/distribution 组件提供标准正态标量和批量采样；不把非均匀分布算法并入随机状态转换。
- 维持每 run 一个独立随机源、RunGroup 隔离、调度无关 seed 和无全局随机流的现有职责。
- 以已知输出、分布测试、分配分析、JIT 审查和 BenchmarkDotNet 证明内置实现的正确性与高性能，不在随机热路径引入接口或虚调用。

## 非目标

- 不在本 Spec 中批量化、SIMD 化或重排 Bat、Cuckoo、PSO、Firefly 的生产随机请求和更新循环。
- 不保证迁移前 `System.Random(seed)` 与迁移后 `RandomSource` 内部 `xoshiro256++` 的固定 seed 轨迹一致。
- 不保证跨 OS、CPU 架构、Runtime、JIT 设置或包版本产生相同序列。
- 不保证一次 Fill 等价于连续标量调用，也不保证不同 Fill 切分产生相同序列或最终状态。
- 不提供 byte Fill、`Jump`、`LongJump`、任意跳转、子流/多流拆分、状态导入导出、序列化或随机访问。
- 不提供全局或线程共享随机源、时间隐式播种、密码学安全随机数、远程/GPU 后端或并行共享实例。
- 不提供 `float`、`Half`、泛型数值、参数化正态、Lévy/stable、Gamma、Poisson、Beta 或其他分布目录。
- 不把 Cuckoo 的 Mantegna Lévy 公式、尺度衰减、稳定项或 guidance 移入 Core Randomness。
- 不提供 `IRandomSource`、公共 `Xoshiro256PlusPlus`、随机源 factory、适配器、后端注册中心或字符串选择机制。
- 不预建备用 PRNG、测试 PRNG、内部 engine 接口或多实现切换层；真实需求出现后通过新 Spec/ADR 重新评估。

## 架构契合

Randomness 是 `Metaheuristics.Core` 内的底层子系统，只拥有随机状态、基础均匀采样和通用非均匀分布采样。Core Execution 以每个 run 的 `ulong` seed 创建独立 `RandomSource`，并继续拥有 Context、取消、Repair 与 Evaluate 生命周期。Algorithms 通过 Core 的公共 Context 和 `RandomSource` 消费随机能力，但算法决定何时、按什么控制流请求样本；Randomness 不认识候选、边界、种群、迭代或 Optimization。

仓库项目依赖图保持 ADR-0003 的现状：

```text
Metaheuristics.Algorithms  ──→ Metaheuristics.Core
Metaheuristics.Experiments ──→ Metaheuristics.Core
```

本变更不新增 `csproj`、运行时程序集、NuGet 包或发布版本关系。只有出现不应依赖 Core 的真实消费者、不同目标平台/重量依赖、独立版本节奏或已证明的构建/部署成本时，才通过新 Spec/ADR 重新评估提取 `Metaheuristics.Random` 程序集。

`RandomSource` 是公共密封的随机消费能力，但其构造函数为 internal。调用方可通过 `OptimizationRunContext.Random` 读取并调用它，但不能自行创建、继承、实现或替换随机源。类型内部只存在一个 `xoshiro256++` 状态实现；不为未来 PRNG 预建接口、工厂、注册或切换层。密封具体类型的标量和批量路径不依赖接口去虚化。

封闭构造同时限制外部策略的独立测试：外部 Initializer/Repair 作者必须通过 Runner 创建 run，在自定义 Optimizer 的运行回调内使用 Context 提供的随机源验证策略，并只把结果快照带出 run。文档和验收必须提供仅依赖公共 API 的可编译测试示例；仓库内 friend assembly 的构造权限不能代替这项外部可用性证据。不为测试新增公共 factory，也不允许为测试缓存或跨 run 复用随机源。

标准正态是独立 sampler/distribution 的职责。它消费 `RandomSource`，但不成为 Xoshiro 状态转换的一部分；是否采用 Box–Muller、Ziggurat、spare sample 或其他私有状态，必须在 Spec 澄清公共组件形状后由 Plan 比较并决定。任何采样状态必须归一个明确的 sampler 或 run 私有对象所有，不得进入全局状态或跨 run 复用。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| `ulong` run seed 与显式 seed 列表 | Runner/Experiment 调用方 | 复制并按现有计划规则检查数量 | 缺少显式 seed 时按批准的 64 位派生规则生成；列表不足继续失败。 |
| `xoshiro256++` 状态初始化与推进 | `RandomSource` 内部实现 | 否 | 必须与批准的 SplitMix64 初始化及作者参考状态转换的已知输出一致。 |
| 每 run 随机源生命周期 | `OptimizationRunContext` | 是 | 每个 run 新建实例；不得跨 run、Group 或线程共享。 |
| 随机源创建与替换 | Core Execution | 是 | 只能从当前 run seed 内部创建 `RandomSource`；无外部构造、继承、实现或 factory 注入路径。 |
| 均匀区间参数 | 调用方与内置随机源 | Core Randomness 在采样/写入前验证 | 非有限、反向、相等或其他未支持范围按公共异常契约失败，且不推进状态或部分写入。 |
| 有界整数的无偏映射 | 随机源实现 | 否 | 内置实现不得使用有模偏差的简单余数映射。 |
| 标准正态采样和可选私有状态 | 独立 sampler/distribution | 否 | 必须保持 run 私有、可重复、无调用级分配并满足统计验收。 |
| 算法随机请求顺序 | 对应 Optimizer | 否 | 类型和方法名迁移不重排请求；Bat/PSO 有界 double 同时采用 FR-007 明确的上界舍入修正，后续重排必须进入独立算法 Spec。 |
| Repair、Evaluate、比较和取消 | Core 与既有策略 | 是 | 时机、计数、异常和状态隔离不得因随机项目迁移而改变。 |

## 功能需求

### FR-001：Core Randomness 子系统与封闭随机源

- 前置条件：调用方引用 `Metaheuristics.Core`。
- 触发行为：Core 创建 run Context，或调用方通过 Context、Initializer 或 Repair 消费随机能力。
- 预期结果：Core 公开密封 `RandomSource`，其构造函数为 internal；调用方可调用其标量与 Fill 成员，但不能自行创建、继承、实现或替换随机源。随机状态在类型内部固定由 `xoshiro256++` 实现。
- 边界情况：不得存在 `IRandomSource`、公共 Xoshiro 类型、公共构造、派生点、factory、服务定位器、反射或全局注册表；不为未来实现预建无消费者抽象。
- 验收标准：项目引用图保持不变；公开 API 和残留审查证明只有封闭 `RandomSource`；Release JIT/基准证明标量和 Fill 路径无接口或虚分派。

### FR-002：`xoshiro256++` 内部实现与 64 位播种

- 前置条件：调用方提供任意 `ulong seed`。
- 触发行为：Core 从 `ulong seed` 内部构造 `RandomSource` 并请求输出。
- 预期结果：`RandomSource` 内部使用四个 64 位状态字和 `xoshiro256++` 1.0 输出/状态转换；单个 64 位 seed 通过 SplitMix64 展开为四字状态，且不得形成全零状态。
- 边界情况：seed `0`、`ulong.MaxValue` 和相邻 seed 均有效；不得使用当前时间、进程状态、线程 ID 或共享熵隐式修改显式 seed。
- 验收标准：参考状态、边界 seed 和长序列已知输出测试与作者算法一致；相同目标环境、版本、seed 和调用序列重复运行一致。

### FR-003：基础标量与同名 Fill 重载

- 前置条件：存在有效 `RandomSource` 和调用方拥有的目标 Span。
- 触发行为：调用基础采样成员。
- 预期结果：公共契约使用以下命名和能力，不引入 `NextUInt64`、`NextInt32`、`FillUniform` 等重复名称：

  ```csharp
  public sealed class RandomSource
  {
      internal RandomSource(ulong seed) { /* ... */ }

      public ulong NextULong() { /* implementation omitted */ return 0; }

      public void Fill(Span<ulong> destination) { /* ... */ }

      public double NextDouble() { /* implementation omitted */ return 0; }

      public double NextDouble(double minimum, double maximum) { /* implementation omitted */ return 0; }

      public void Fill(Span<double> destination) { /* ... */ }

      public void Fill(Span<double> destination, double minimum, double maximum) { /* ... */ }

      public int NextInt(int minimum, int maximum) { /* implementation omitted */ return 0; }

      public void Fill(Span<int> destination, int minimum, int maximum) { /* ... */ }
  }
  ```

- 边界情况：空 Span 是无操作且不得推进状态；所有区间使用包含 minimum、排除 maximum 的半开语义；所有有界入口拒绝 `minimum == maximum` 和反向范围。浮点端点必须有限；即使 `maximum - minimum` 溢出，也必须使用防溢出映射支持所有有限且 `minimum < maximum` 的区间，并修正舍入到 maximum 的结果以保持上界排除。
- 验收标准：API、正常、空 Span、边界值、异常前状态和异常前目标保持测试覆盖；内置有界整数通过无偏映射参考测试。

### FR-004：弱序列兼容与 Fill 自由度

- 前置条件：使用同一 Core 包版本和目标执行环境。
- 触发行为：以相同 seed 执行随机调用。
- 预期结果：只有完全相同的方法调用序列与参数才保证产生相同结果；Fill 长度属于调用参数。实现可以为标量、批量、长度分段和尾部选择不同私有路径。
- 边界情况：一次 `Fill(100)` 不承诺等价于两次 `Fill(40)`/`Fill(60)`，也不承诺等价于一百次标量调用；不同方法间不承诺固定的底层 `ulong` 消费对应关系。
- 验收标准：重复调用序列测试通过；测试和文档不得把未承诺的切分、标量/批量、跨平台或跨版本等价性固化为契约。

### FR-005：独立标准正态采样

- 前置条件：调用方持有有效 `RandomSource`。
- 触发行为：通过 Core Randomness 的独立标准正态 sampler/distribution 请求一个或一批 `double`。
- 预期结果：组件只生成均值 `0`、标准差 `1` 的标准正态样本，并提供标量与 `Span<double>` 批量入口；均值/标准差缩放由调用方负责。组件不成为 `RandomSource` 成员，也不成为 Xoshiro 状态转换的一部分。
- 公共形状：

  ```csharp
  public static class StandardNormal
  {
      public static double Sample(RandomSource random);

      public static void Fill(RandomSource random, Span<double> destination);
  }
  ```

- 边界情况：空 Span 不消费随机状态；不保证正态标量与批量或不同 Fill 切分等价。Box–Muller、Ziggurat、spare sample、尾部丢弃和私有状态的选择不得泄漏为算法或 Core 的职责。
- 验收标准：`StandardNormal` 是无对象生命周期的静态 distribution；不保存跨调用 spare，但 Fill 可在单次调用内成对利用样本。Plan 再以统计、吞吐、分配和 JIT 证据选择实现算法。固定调用序列重复一致，调用级托管分配为零。

### FR-006：Core 随机契约与 `ulong` seed 一次性迁移

- 前置条件：Core Randomness 公共契约和替代 ADR 已获批准。
- 触发行为：Runner 创建 run Context，Initializer/Repair 消费随机源，或 Experiment 规划并记录 seed。
- 预期结果：`OptimizationRunContext.Random`、`ICandidateInitializer.Initialize` 和 `ICandidateRepair.Repair` 使用 `RandomSource`；Context 从本 run 的 `ulong seed` 内部创建它。Runner、Experiment options、显式 seed 列表、RunGroup plan、结果和摘要中的 seed 全部使用 `ulong`。
- 边界情况：不保留 `int` 重载、隐式兼容包装器、`System.Random` 适配器、双随机源或随机源 factory；现有正负 `int` seed 轨迹不保留。Experiment 使用 `unchecked(BaseSeed + (ulong)repetitionIndex)` 获得每个 repetition 的 seed；该模 `2^64` 加法在合法 `int` repetition index 范围内不碰撞，不调用 SplitMix64 或了解 Xoshiro 状态。
- 验收标准：公开 API、示例、XML、测试与残留搜索证明运行时契约没有 `System.Random` 或 `int` seed；相同 repetition 在不同 Case 和不同 RunGroup 拆分下仍取得相同 `ulong` seed。

### FR-007：现有策略与算法接线及有界采样修正

- 前置条件：Core 公共签名已切换到 `RandomSource`。
- 触发行为：构建并运行现有 Clamp、Reflect、RandomReset、Bat、PSO、Firefly 和 Cuckoo。
- 预期结果：现有实现把随机参数/字段/局部变量和基础调用迁移到 `RandomSource`，例如 `Next(int)` 改为 `NextInt`；不重排循环、分支、Repair/Evaluate 时点或算法工作区。Bat/PSO 的非相等有界 double 请求统一采用 FR-003 的半开范围，包括修正旧线性映射可能舍入到上界的结果；这是独立于 PRNG 替换的数值行为变化。相等边界继续直接返回且不消费随机数，现有 Options 范围验证保持。
- 边界情况：固定 seed 的具体位置、Evaluation 和最优轨迹允许因更换 PRNG 与 seed 宽度而改变；RandomReset 仍只在其既有有限越界条件下请求随机样本。
- 验收标准：行为测试证明调用条件、次数、Repair/Evaluate 时机、取消、连续 run 重置和 RunGroup 隔离保持；测试不再以旧 `System.Random` 具体数值作为参考。Bat/PSO 增加相邻可表示端点的范围测试，例如 `[1, Math.BitIncrement(1))` 必须返回 `1`，并覆盖相等边界不消费随机数；不得把该舍入修正仅归因于固定 seed 轨迹变化。

### FR-008：后续算法优化隔离

- 前置条件：本 Spec 已实现并验证基础随机能力与 Core 迁移。
- 触发行为：提出 Bat 或 Cuckoo 的批量随机和 SIMD 改造。
- 预期结果：每种算法通过后续独立 Spec 决定随机样本的批次布局、控制流重排、标量/批量轨迹、工作区和端到端门槛；Core Randomness 不包含算法专用接口。
- 边界情况：本 Spec 的 Randomness 基准或新 Fill API 不构成 Bat/Cuckoo 生产改造授权。
- 验收标准：本 Spec 完成时不存在未经单独批准的 Bat/Cuckoo 随机缓冲、抽样重排或新 SIMD 生产路径。

## 非功能需求

### NFR-001：确定性、状态与线程安全

- 测量方式：同一目标环境的重复调用序列、并发 Group、连续 run、异常和状态隔离测试。
- 可接受阈值：相同版本、环境、seed、方法序列及参数逐位一致；每个 run 独占一个随机源；实例不保证线程安全且不得并发或跨 run/Group 共享；异常参数不推进内置随机源或部分写入目标。
- 证据类型：自动化测试、实现审查和并发隔离测试。

### NFR-002：Xoshiro 与分布正确性

- 测量方式：作者参考已知输出、SplitMix64 seed 展开测试、有界整数频数/拒绝边界测试，以及固定样本量的标准正态均值、方差、分位数和尾部检验。
- 可接受阈值：Xoshiro 状态转换与参考逐字一致；无简单模偏差；标准正态统计阈值在 Plan 中预先定义且不得依赖随机重试或不稳定断言。
- 证据类型：已知答案测试、确定性统计测试和算法来源审查。

### NFR-003：批量性能与零调用级分配

- 测量方式：BenchmarkDotNet 比较 seeded `System.Random` 标量循环、`RandomSource` 标量循环和 `RandomSource.Fill`；覆盖 `ulong`、均匀 `double`、有界 `int` 和标准正态，主要长度 32、128，诊断长度 0、1、2、7、8、31、33、127、129，并记录 JIT/硬件/Runtime/分配。
- 可接受阈值：所有内置标量和 Fill 采样调用级托管分配为零；主要长度的均匀 Fill 均快于对应 seeded `System.Random` 标量循环。具体加速门槛、正态候选准入与 Core 集成端到端门槛由 Approved Plan 在运行命令获用户批准前确定。
- 证据类型：BenchmarkDotNet、MemoryDiagnoser、反汇编和端到端基准。

### NFR-004：封闭具体路径无随机分派抽象

- 测量方式：Release JIT/反汇编、标量与 Fill 基准、实现审查。
- 可接受阈值：`RandomSource` 及其 Xoshiro 状态推进循环不得调用随机源接口、虚成员、delegate、反射、factory 或服务定位器。允许直接调用私有状态 helper；是否内联本身不构成验收门槛。吞吐与分配按 NFR-003 验收，JIT 证据用于定位和解释瓶颈，不能仅为消除 call 复制状态转换。
- 证据类型：反汇编、基准和源码审查。

### NFR-005：有限 API 与实现范围

- 测量方式：公开 API、项目引用、命名和残留搜索。
- 可接受阈值：只存在 FR-003 的封闭 `RandomSource`、其内部 Xoshiro 状态实现和 FR-005 的标准正态组件；不存在 `IRandomSource`、公共 Xoshiro 类型、备用 PRNG、切换层、排除的分布、后端、状态管理、byte Fill、跳转、多流或兼容壳。Fill 统一使用同名重载，整数方法使用 `NextULong`/`NextInt`，不出现 `NextUInt64`/`NextInt32`/`FillUniform`。
- 证据类型：API 审查、`rg` 残留搜索、Release build 和文档验证。

## 职责与替代关系

- 新增的概念：Core 内的 Randomness 子系统；只能由 Core 构造的公共密封 `RandomSource`；内部 `xoshiro256++` 状态实现；基础标量/Fill 重载；独立标准正态 sampler/distribution；统一 `ulong` seed。
- 被替代的概念：Core Context 创建和公开 `System.Random`；Initializer/Repair 的 `System.Random` 参数；Runner/Experiments/Results 的 `int` seed；Cuckoo 私有标准正态实现仅在后续算法 Spec 批准迁移时才被替代。
- 必须删除的旧行为或公共入口：所有运行时 `System.Random` 公共签名、`int` seed API/存储、旧固定 seed 数值测试及任何兼容重载、随机源接口、factory、适配器或双随机源。
- 明确保留的旧概念及独立理由：Experiment 仍规划共享 repetition seed；Context 仍拥有每 run 随机源；Optimizer 仍决定请求顺序；Initializer/Repair 仍消费同一 run 流；Repair/Evaluate、取消和 RunGroup 生命周期不变。
- 完成后每个概念的唯一所属层：Core Randomness 拥有 PRNG、基础均匀采样和通用分布组件；Core Execution 拥有 run 随机源创建与传递；Experiments 拥有与 PRNG 无关的 64 位 seed 计划；Algorithms 拥有算法抽样时机和后续批量工作区；Tests/Benchmarks/Verification 拥有正确性与性能证据。

## 成功标准

- Core 可以从 `ulong` seed 为每个 run 创建独立 `RandomSource`，调用方只通过 Context 获得它，并以标量或 Fill 重载获得明确范围的基础随机样本。
- 标准正态通过独立组件生成，不绑定或复制到各 PRNG，也不把 Cuckoo 公式变成通用分布职责。
- Core、Experiments、Initializer、Repair 和现有算法只使用 `RandomSource` 与 `ulong` seed，不存在 `System.Random`/`int` seed 兼容壳。
- 相同环境和完全相同调用序列可复现，同时明确允许 Fill 切分、标量/批量、跨平台和跨版本序列不同。
- 密封 `RandomSource.Fill` 不引入随机源接口/虚分派，并具备零调用级分配和经验证的主要批量性能收益。
- 现有算法完成接线和 FR-007 的 Bat/PSO 有界采样修正，没有未经批准的 Bat/Cuckoo 批量化、随机重排或 SIMD 扩展。
- 替代 ADR、工程规范、架构概览、API 文档、示例、测试、基准和最终 Verification 与实现一致。

## 已澄清决定

- 已确认：默认生成器为 `xoshiro256++`，不是 `xoshiro256**` 或 `xoshiro256+`。
- 已确认：不提供 `IRandomSource`；公共 `RandomSource` 为 sealed 且只有 internal 构造，用户可消费但不能创建、继承、实现或替换随机源。
- 已确认：`xoshiro256++` 是 `RandomSource` 的唯一内部实现；不预建第二 PRNG、engine 接口、factory 或切换层，新的真实模型需求由后续 Spec/ADR 处理。
- 已确认：Randomness 作为 `Metaheuristics.Core` 内的子系统，不新增 `csproj`、程序集、NuGet 包或发布关系。
- 已确认：统一使用 `ulong` seed，不保留 `int` 兼容入口。
- 已确认：Fill 使用同名重载；整数标量方法命名为 `NextULong` 和 `NextInt`。
- 已确认：基础分布只包括完整 `ulong`、均匀 `double`、有界无偏 `int` 和标准正态；正态只提供标准形式。
- 已确认：所有有界入口拒绝 `minimum == maximum`；浮点端点必须有限。
- 已确认：不保证 Fill 任意切分等价，也不保证 Fill 与连续标量调用等价。
- 已确认：不保证跨平台或跨版本一致；只保证相同版本、目标环境、seed、方法序列和参数的重复执行。
- 已确认：第一阶段同时迁移 Core、Experiments、Initializer 与 Repair 的公共随机/seed 签名；不要求迁移前后算法随机轨迹一致。
- 已确认：Bat/PSO 非相等有界 double 采用上界排除修正，这是本次显式批准的数值变化；相等边界、配置验证和请求时机保持。
- 已确认：byte Fill、跳转、流拆分、状态导入导出、全局共享、额外分布和算法改造均排除。
- 已确认：标准正态公共组件为静态 `StandardNormal`，公开 `Sample(RandomSource)` 和 `Fill(RandomSource, Span<double>)`；不保存跨调用 spare，具体采样算法留给 Plan。
- 已确认：有界 `double` 支持所有有限且 `minimum < maximum` 的端点，即使宽度溢出也使用防溢出映射，并保持上界排除。
- 已确认：Experiment 使用 `unchecked(BaseSeed + (ulong)repetitionIndex)` 派生 seed，不调用 SplitMix64 或了解 Xoshiro。

## 批准记录

- 规格批准：项目作者
- 批准日期：2026-09-05
- 修订批准：2026-09-05，项目作者在设计审查后同意修订并提交；明确外部公共 API 测试路径、FR-007 上界舍入修正，以及以实测收益而非私有 helper 是否内联作为性能验收依据。
- 批准时需明确接受的风险：迁移会破坏所有 `System.Random`/`int` seed 公共调用并改变既有固定 seed 搜索轨迹；Fill 的长度和调用划分属于序列语义，调用方不得把不同切分视为等价；随机源是 Core 拥有的封闭具体能力，无用户替换点；标准正态算法和性能只有在 Approved Plan 的候选验证后确定。
