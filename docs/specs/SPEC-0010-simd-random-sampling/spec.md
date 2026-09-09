# SPEC-0010：自适应向量与标量双状态随机源

## 元数据

- 编号：`SPEC-0010`
- 状态：`Approved`
- 创建日期：2026-09-06
- 修订日期：2026-09-08
- 批准人：项目作者
- 批准日期：2026-09-08
- 替代：SPEC-0009 的单组四字状态、排除内部 Jump 及封闭采样成员集合限制；撤销本规格固定 Vector512 方案
- 被替代：无
- 相关 ADR：ADR-0014、ADR-0019、ADR-0024

## 问题与动机

项目作者已放弃旧共享状态候选，并进一步要求：批量状态与运算统一使用自适应宽度的 `System.Numerics.Vector<T>`；不要 BatchCursor，状态推进与标量一样直接传递四个 ref 状态；RandomSource 新增返回 Vector<ulong> 和 Vector<double> 的两个批量 Next 方法；StandardNormal.Fill 调用该向量采样入口。

本修订取代先前批准的固定八 lane 行为。双状态隔离、PRNG、所有权及数值规则继续保留；接口名称和完整消费规则已获批准，并按用户补充要求明确正态向量数学路径。当前运行时代码仍为 SPEC-0009 实现，未执行本设计。

## 目标

- 单个 RandomSource 持有四个 ulong 单值状态字和四个 Vector<ulong> 批量状态向量，二者独立推进。
- 采用运行时决定的 `L = Vector<ulong>.Count = Vector<double>.Count` 个 lane；统一 Vector<T> API，不手写固定宽度级联。
- 直接提供一个向量的原始值与单位 double 样本。

## 非目标

- 不更换 xoshiro256++，不公开 seed 构造、Jump、子流、状态管理或后端配置。
- 不引入 BatchCursor、BeginBatch/EndBatch、持久输出缓存、InlineArray 或 Core 源码生成器。
- 不修改 SPEC-0011 至 SPEC-0014 的算法实现；Vector<T> 统一范围为本次 Core Randomness 改造，不改现有 Algorithms SIMD。
- 不要求有界整数映射向量化；StandardNormal.Fill 的 Box–Muller 数学计算必须使用 Vector API。

## 架构契合

Core 独占状态和分布，Context 从显式 ulong seed 为每 run 创建封闭 RandomSource。Algorithms 决定抽样时机，Experiments 不感知向量宽度。新增向量返回值是采样能力，不暴露内部可变状态。ADR-0024 替代 ADR-0023 的固定 Vector512 选择。

## 信任与责任边界

| 数据或行为 | 责任方 | 验证位置 | 失败或边界 |
| --- | --- | --- | --- |
| seed、实例生命周期 | Core Execution | 每 run 创建 | 不跨 run/Group 共享 |
| 宽度与状态转换 | Core Randomness/.NET | 独立参考和环境记录 | 不由线程或调用长度决定 lane 数 |
| 区间和目标 Span | 调用方/Randomness | 写入和推进前 | 沿用参数异常与原子性 |
| 正态配对 | StandardNormal | 参考及尾部测试 | 不消费标量状态 |
| 调用与回调时机 | Algorithms/策略 | 原调用链 | 不由 Core 重排 |

## 功能需求

### FR-001：双状态与自适应初始化

- 前置条件：以显式 ulong seed 创建 RandomSource。
- 触发行为：初始化两套状态。
- 预期结果：保留 xoshiro256++ 1.0 和 SplitMix64。原规则生成标量初态 S；在临时四字副本上执行 Jump，批量 lane i 为 Jump^(i+1)(S)，i 从 0 至 L-1。构造时完成 L 次 Jump，不推进实际标量状态。
- 边界情况：seed 0、最大值、相邻 seed 有效，lane 不得全零或复制同一个初态。Jump 间距不证明统计独立或不同 seed 不重叠。
- 验收标准：每 lane 初始化和原始序列匹配独立标量参考。状态载荷为 `32 + 32*L` 字节，实际对象头、对齐和构造成本另测；不再固定 288 字节。

### FR-002：公开向量采样 API 与路由

拟采用以下两个无参数公共实例方法；C# 不能仅凭返回类型重载已有 Next 方法，因此使用明确后缀：

```csharp
public Vector<ulong> NextULongVector();
public Vector<double> NextDoubleVector();
```

- 前置条件：已初始化 RandomSource。
- 触发行为：调用任一向量方法。
- 预期结果：恰好推进一轮全部 L 个批量 lane，返回 lane 0 至 L-1 的结果。NextULongVector 返回完整 ulong 范围；NextDoubleVector 对本轮每个 raw 计算 `(raw >> 11) * 2^-53`，结果在 [0,1)。返回值为值类型样本，不是状态视图，不产生调用级分配。
- 路由：原 NextULong、NextDouble 两个重载、NextInt 和 StandardNormal.Sample 只消费标量状态；两个向量 Next、全部 Fill 和 StandardNormal.Fill 只消费批量状态。不能再用“全部 Next* 都是标量”的表述。
- 边界情况：标量与批量互不推进；所有向量方法和 Fill 共享同一批量状态，不新增第三套状态；方法每次返回完整向量，无尾部缓存。
- 验收标准：跨入口混合调用及双向插入隔离测试通过；相同批量初态下两个向量方法的输出满足上述转换关系。

### FR-003：RandomSource.Fill 的排放与拒绝

- 前置条件：合法非空 Fill。
- 触发行为：生成目标样本。
- 预期结果：每轮推进 L 个 lane，按 lane 0 至 L-1 使用原始字；完整块后继续下一轮，公开调用末尾丢弃未用原始字。raw/unit/bounded double 各输出消耗一个原始字，推进轮数为 ceil(length/L)。可直接调用四 ref Vector 内核，不要求每个 RandomSource.Fill 经公开向量方法逐层转发。
- 边界情况：有界整数按同一交织顺序检查原始字，拒绝就继续取下一个，直至填满目标；宽度 1 仍消费原始字。使用调用内普通局部输出向量/索引处理剩余 lane，不引入游标类型或跨调用缓存。内部分块不得额外丢弃原始字。
- 验收标准：独立参考覆盖 0、1、2、L-1、L、L+1、2L-1、2L、2L+1、31、32、33、127、128、129、1024，验证输出和后续批量状态；拒绝跨轮测试通过。

### FR-004：StandardNormal.Fill 使用向量采样

- 前置条件：random 非 null，目标合法。
- 触发行为：StandardNormal.Fill。
- 预期结果：每块调用一次 `random.NextDoubleVector()`，在返回向量内部按相邻 lane 配对：lane 2j 为半径输入 u、lane 2j+1 为角度输入 v，计算 `r = sqrt(-2*log(1-u))`、`a = 2*pi*v`，按 r*cos(a)、r*sin(a) 顺序写入。一个完整向量提供 L 个正态输出。当前 Vector<double> 支持的 Count 为偶数，按其实际 Count 分块。取得单位向量后，半径、角度及 Log/Sqrt/Sin/Cos 计算必须使用 System.Numerics.Vector API（允许合并的 SinCos）；不得逐 lane 调用 Math.Log、Math.Sqrt、Math.Sin、Math.Cos 或 Math.SinCos。
- 边界情况：不足 L 个输出仍调用一次向量方法，只写剩余目标；最后一个单独输出使用完整的一对输入并丢弃配对第二个正态值，未使用的 lane 也不跨调用保留。尾块仍执行完整向量数学，只限制最终写入数量，不能退回标量超越函数。空目标不调用向量方法；null 仍在空目标之前验证。Sample 继续调用原标量路径。
- 验收标准：对非空长度 N，恰好消费 ceil(N/L) 轮批量状态；用逐 lane 参考验证配对、尾部及后续状态，不调用标量 NextDouble 补尾。不承诺与连续 Sample 或不同 Fill 切分等价。正态向量结果不要求与逐 lane Math 逐位一致；Plan 固定数值容差，并用统计与真实向量调用/JIT 证据共同验收。

### FR-005：数值与确定性边界

- 前置条件：同版本、运行时、有效向量宽度、seed、同类调用序列和参数。
- 触发行为：重复采样或混合调用。
- 预期结果：结果可重复；保留半开范围、极端有限 double 端点、上界舍入修正、无偏整数和原异常类型。合法空 Fill 不消费两套状态；无效区间即使目标为空也先验证，失败不写入或推进。
- 边界情况：Vector.Count 是执行环境的一部分，不保证不同宽度、硬件开关、运行时或平台的批量序列相同。单值路径不受向量宽度和批量调用影响，保留仅单值的既有原始/均匀序列。批量返回/Fill 不等价于连续单值调用。
- 验收标准：每个实际宽度对其 L-lane 参考验证；不在不同宽度间建立错误的完整流相等断言。run/Group 隔离和固定 seed 重复测试通过。

### FR-006：直接 ref 向量内核

- 前置条件：本修订 Spec 与相应 Plan 获批准。
- 触发行为：实现批量推进。
- 预期结果：内核直接接受四个 `ref Vector<ulong>`，使用 System.Numerics.Vector 的加法、XOR、移位、OR 和转换；与标量四 ref 状态结构对应。不使用 Vector128/256/512 或 ISA 专属 API，也不建立 BatchCursor。
- 边界情况：运行时无硬件加速时继续使用 Vector<T> 的软件执行，按该环境报告的 L 验证；不切换到单值状态。
- 验收标准：独立参考、零分配和无硬件测试通过；硬件环境下反汇编证明原始状态算术 SIMD 化，不能仅凭 Vector 类型作结论。

## 非功能需求

### NFR-001：分配与性能

- 测量方式：BenchmarkDotNet、MemoryDiagnoser、反汇编、固定工作量完整 run。
- 可接受阈值：所有采样方法调用级分配为零；允许单值一定幅度变慢，预算及批量/整体门槛在 Plan 中事前固定。构造、首次调用、两个新增向量 API 和 Fill 分别测量，完整 run 计入 L 次 Jump。
- 证据类型：当前标量基线与同 L-lane 独立标量参考对照；记录 Vector.Count、Vector.IsHardwareAccelerated、运行时/机器、输入、源 hash、命令、耗时和分配。旧候选结论不计入验收，批准范围内不设逐轮测量确认。

### NFR-002：随机质量

- 测量方式：逐 lane、交织输出、两类流及相邻 seed 检查，覆盖拒绝采样和正态。
- 可接受阈值：状态转换与 Jump 逐字匹配参考；Plan 固定样本量、seed、统计阈值、预算及失败处理，不失败后换 seed 重试。
- 证据类型：已知答案和确定性统计报告；各项使用实际 L 和样本对数，有限统计不证明独立性。

## 职责与替代关系

- 新增：两个公开向量采样方法；RandomSource 内按调用类别分隔的两套状态。
- 替代：旧共享状态路由及此前固定 Vector512/八 lane 与 BatchCursor 设计。
- 删除：固定宽度硬件分派、游标抽象、无消费者 helper/兼容壳；不恢复旧候选。
- 保留：原标量 API、各 Fill、静态 StandardNormal、现有 PRNG、Core/run 所有权及数值异常规则。
- Randomness 仍是唯一状态/分布所属层，算法布局仍归 Algorithms。

## 成功标准

新增向量 API、宽度自适应和双状态隔离符合参考；StandardNormal.Fill 确实以向量采样入口为输入；原始状态推进具有 SIMD 证据；正确性、随机质量、分配与新 Plan 性能门槛通过。设计修订不等于实现完成。

## 已确认与待审查细节

- 用户已指定 Vector<T>、四 ref 内核、去除 BatchCursor、两个返回向量的 Next 方法，以及正态 Fill 使用该方法。
- 本稿将方法命名为 NextULongVector/NextDoubleVector，并具体规定相邻 lane 正态配对和宽度相关复现边界；本修订稿已由项目作者批准。
- 已批准的双状态隔离、现有 PRNG、构造播种和无跨调用缓存方向保留；固定八 lane 的旧批准不再有效。

## 批准记录

- 当前修订规格批准：项目作者
- 批准日期：2026-09-08
- 历史：2026-09-08 固定 Vector512 版本曾获批准；用户随后明确修改上述设计，曾退回 Clarifying；当前批准专指自适应 Vector 修订稿，不沿用旧固定宽度批准。
- 批准时接受的风险：向量宽度影响批量序列和状态体积；公开 API 暴露机器自适应的样本数；构造与短批次成本仍需测量。

- 2026-09-08：项目作者通过“同意”批准自适应 Vector 修订，并明确要求 StandardNormal.Fill 的 Sin/Cos 继续直接使用 Vector API；完整向量数学及尾部写入规则已同步。
