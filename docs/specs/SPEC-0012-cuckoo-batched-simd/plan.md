# SPEC-0012 技术计划：Cuckoo 批量正态、Lévy 与遗弃算术

## 元数据

- 状态：`Draft`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`0a74f08592c89b0c09f5e242bb3c111c7f967c7b`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 创建日期：2026-09-14
- 修订日期：2026-09-15
- 批准人：—
- 批准日期：—

[共同验证计划](../simd-plan.md)是本 Plan 的组成部分，规定基线 A/B/C、数值通则、性能门槛与测量记录要求；本文件定义算法特有的设计。下列方案均待本 Plan 批准。

## 当前实现调查

- `src/Metaheuristics.Algorithms/Cuckoo/CuckooOptimizer.cs` 在 GenerateLevyCandidate 逐维调用两次私有 NextGaussian，保存 _hasSpareGaussian/_spareGaussian；随后调用单位 NextDouble 引导。
- 私有正态以 Math.Max(u, double.Epsilon) 生成半径，Core StandardNormal.Fill 已使用 1-u、双单位向量同 lane 配对、先 cos 向量后 sin 向量写出。迁移不能要求与旧 Gaussian 样本相同。
- Lévy 分母为 `Pow(Abs(normal)+1e-10, 1/LevyExponent)`，是加 epsilon，不是 Max 钳制。分子乘 _levySigma 再乘 GaussianScale，步长先乘 levyScale 再除分母；最后是 source + 0.8*step + 0.2*guidance。
- 遗弃候选先单值抽两个种群索引，种群大于 1 且索引相等时只重抽第二个。逐候选 Generate→Repair→Evaluate→条件替换→更新 best；后续候选可读到刚更新的种群和 best。
- Lévy 候选按 candidateIndex 直接对应 population，没有随机索引抽样；每个候选都立即 Repair、Evaluate、条件替换并更新 best，后续 Lévy 候选读取新的 best。全部 Lévy 完成后只调用一次 FindWorstIndices，冻结本轮遗弃目标索引列表，但遗弃输入 population/best 仍随前一替换实时变化。排序及 tie 顺序保持，不优化排序。现有工作区为 population、candidates、sortedIndices 与 best。
- CuckooOptimizerTests 已有评估/Repair 计数、单巢、复用及隔离测试，尚缺精确抽样消费、回调事件、立即 best 更新与“冻结 worst/live population”覆盖；这些测试需新增。RandomMigrationBenchmarks 提供完整 run，CuckooRandomDiagnosticsBenchmarks 可辅助诊断，需独立新增同样本候选内核对照。

## 整数采样范围与独立候选

2026-09-15 项目作者批准：本轮使用现有 NextInt，分别评估不同索引对映射与跨候选索引预取，不恢复 SPEC-0010 已删除的 `Fill(Span<int>, int, int)`，不新增公共整数 SIMD API。Core 继续唯一拥有通用无偏整数分布；后续若有证据支持新增整数 SIMD 能力，另行修订 Core 规格。本轮范围问题已关闭，整份 Plan 仍待批准，当前只做规划、不运行实验。

种群大小 P>1 时，候选方案先取 `first=NextInt(0,P)`，再取 `second=NextInt(0,P-1)`；若 `second>=first` 则 `second++`。对每个 first，该映射将 P-1 个等概率值一一对应到其余索引，保持有序不同索引对的均匀分布。它消除算法层的相等索引重抽，不保证两次底层状态推进，因为 Core NextInt 内部仍可能拒绝采样。P=1 时直接返回 (0,0)，不消费索引样本；遗弃数量为 0 时不消费任何遗弃样本。

映射替换与预取分别计量：先比较原重抽和逐候选新映射，再比较同一映射的逐候选调用与索引缓冲。索引预取只依赖本 run 固定的 P，不提前读取对应位置。新旧布局允许改变策略后来取得的随机值，不能以此声称破坏同版本可复现性。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：私有正态与原逐维算术 | 直接生产基线 | 分布重复、标量采样 | 不满足迁移完成条件 | 仅基线 |
| B：StandardNormal.Fill/单位 Fill + 原算术 | 清理分布职责，隔离批量收益 | 3D scratch、旧轨迹改变 | 正态/角色布局 | 首个候选 |
| C1：直接 TensorPrimitives | 可利用现有适用操作 | 操作覆盖范围有限 | 超越函数误差 | 首个候选 |
| C2：保留验证过的 Pow 阶段，私有融合其余算术 | 减少遍历、限制数学变化 | 私有级联与 scratch 阶段 | 溢出/epsilon/乘除次序 | 无适用直接操作或未过门槛时评估 |
| B-block：跨候选预取正态/单位样本 | 扩大批次 | 有界缓冲及角色布局变化 | 不得提前读取候选位置 | 独立评估 |
| I-map / I-prefetch：索引映射 / 索引预取 | 消除外层重抽 / 集中取得索引 | 标量状态消费改变 / 索引缓冲 | 仍须实时读取 population/best | 分别评估后再组合 |
| 提前捕获 best/population 位置 | 可减少读取 | 后续候选读不到替换结果 | 违反实时状态规则 | 不采用 |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 标准正态与 spare | Core 与 Cuckoo 重复 | Core StandardNormal | 删除重复分布 |
| Lévy 参数、LogGamma、缩放 | Cuckoo | Cuckoo | 属于算法公式 |
| 有界整数分布 | Core NextInt | Core NextInt | 不复制通用无偏映射 |
| 批次布局/候选算术 | Cuckoo | Cuckoo/私有 VectorOps | 保持算法职责 |
| 排序、种群和 best | CuckooOptimizer | CuckooOptimizer | 保持逐候选更新 |

## 批次布局与工作区

单候选 B 使用一块 `double[3D]`，每 Optimizer 载荷 24D 字节：normalNumerator[D]、normalDenominator[D]、unit[D]。EnsureWorkspace 首次分配，run 间只复用存储；不创建新随机源，不保留正态 spare。所有活跃切片先写满再读取。

每个 Lévy 候选，在现有调用位置按顺序执行：

1. 一次 StandardNormal.Fill，目标为 scratch 前 2D 个元素；前 D 个分配给 numerator、后 D 个给 denominator，不做跨候选缓存。消费 `2*ceil(D/L)` 轮批量状态。
2. 一次 RandomSource.Fill(unit)，消费 ceil(D/L) 轮批量状态。
3. 对当前 source 和当前 best 计算原 Lévy/引导公式，然后原位调用 Repair。合计 `3*ceil(D/L)` 轮，不消费标量状态；即使尾部不满向量，也只由 Core 的本次 Fill 处理。

布局不要求每个向量恰好对应同一随机角色；角色按输出 Span 的前后 D 分配。B/C 共用完全相同的两次 Fill 与数组切片，禁止为了更快把单次 2D 正态拆成两次 D 调用而静默改变序列。

遗弃候选以原索引重抽作为基线，独立评估上一节的新映射。索引选定后读取当时的 population，RandomSource.Fill(unit) 一次，再计算：
`best + ((0.5*decayFactor)*(first-second)) + ((unit-0.5)*AbandonmentPerturbationScale)`。
新映射在种群为 1 时跳过原来的两次索引调用；扰动系数为零时仍保留 unit Fill。abandonmentCount=0 时不进入该路径，不能多消费样本。逐候选 Repair/Evaluate/替换/best 更新原位保留，不缓存下一候选的输入快照。

分块候选比较 K=1、4、8，最终 K 随 Plan 审阅及后续证据确定，不增加公共配置。每块包含当前阶段接下来的 m<=K 个候选，工作区为 `double[3KD]`；如采用索引预取，另有 `int[2K]`，载荷分别为 24KD 和 8K 字节，二者可独立采用。Lévy 块先一次 StandardNormal.Fill(2mD)，前 mD 为所有 numerator、后 mD 为所有 denominator，每个角色内按候选顺序切 D；再一次单位 Fill(mD)。正态消费 `2*ceil(mD/L)` 轮，单位样本消费 `ceil(mD/L)` 轮。后续逐候选使用切片，在实际生成时才读当前 source/best，然后立即 Repair/Evaluate/替换/更新 best。

遗弃块只在全部 Lévy 完成且 FindWorstIndices 冻结目标列表后开始；I-prefetch 按候选顺序先填 m 对标量索引，然后单位 Fill(mD)。不采用 I-prefetch 时保留逐候选索引调用，可独立搭配单位样本分块。标量与批量状态的路由分别由 NextInt 与 Fill 保持，策略可同时消费两套状态，测试需覆盖交错后的后续值。索引只用于候选生成时读取实时 population；不能把冻结 worst 列表误解成冻结种群内容。

最后一块只填活跃 m 个候选，不为 K-m 个空槽采样。块不跨 Lévy/遗弃阶段或 run；取消、异常时丢弃剩余预取样本，不推迟原回调处的取消检查。顺序 run 仅复用物理缓冲，不继续使用上次逻辑内容。算术 B/C 对照须采用相同 K、同一索引方案和同一角色布局。

## 算术阶段与数值准则

B 的算术保留 Math.Pow 及原括号顺序。C1 首先检查目标框架 TensorPrimitives 是否有可直接表达目标操作的适用操作。C2 在没有适用直接操作或未通过门槛时评估：分母阶段独立形成 `Pow(Abs(normal)+1e-10, reciprocalExponent)`，再由 double 512/256/128/scalar 级联融合分子缩放、步长、引导与位置写回；遗弃内核独立融合差分和扰动。

Pow 若使用向量实现，必须先通过单独数值测试和 B/C 性能门槛；不能默认向量 Pow 被许可产生任何误差。没有通过的 Pow 留在 Math.Pow 标量阶段，额外 SIMD 仅融合已经证明的四则算术。不增加自写近似 Pow、LogGamma 或正态公式。C2 的完整候选计时包括前置 Pow 及 scratch 成本，不能只报告融合最后几条运算。

事前数值规则：

- 每个有限 Pow 结果相对误差不超过 1e-12，另加 1e-14 绝对预算；NaN/Infinity 分类、Infinity 与零符号一致，禁止一边有限一边溢出。
- 同正态与单位样本的候选位置误差 `<= 1e-12 + 1e-11*(abs(source)+abs(0.8*step)+abs(0.2*guidance))`；遗弃采用对应三项绝对值之和。中间项必须先满足分类检查；若误差尺度求和溢出，改用按最大有限项归一化的检查，不能以无限容差放行。
- 保持 Abs 后加 epsilon、分子两次乘法、乘后除及 0.8/0.2 加法顺序；不显式 FMA，不改成 Max、分母倒数相乘或合并常量而改变求值次序。
- 诊断 exponent 为 0.5、1、1.5、1.99，并覆盖接近 (0,2) 两端的合法配置、正态 0/次正规/极值/NaN/Infinity、unit 0/最大小于 1、奇数维及尾部。已存在的极端配置溢出行为不静默“修复”。
- 正态统计沿用 Core 已有质量门槛。算法角色切片补充固定 seed 0、1、20260905，每 seed 2^18 对输入，分子/分母各自均值绝对值 <=0.02、方差与 1 偏差 <=0.03、角色相关系数绝对值 <=0.02；固定 D=1、7、32 的实际批次布局各测，取足样本后截断统计。失败不换 seed 重试。Lévy 重尾不使用有限方差假设，主要以同输入参考逐值差分及正负计数检验（7σ）验证角色接线。

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Options/指数/种群 | 原构造器 | 一次 | 否 | 原参数异常 |
| 正态分布/状态 | Core 已有测试+角色集成测试 | 测试阶段 | 无重复逐值验证 | 删除 spare 后仍正确配对 |
| 输入切片/活跃区 | 工作区与哨兵测试 | 首次/测试 | 无逐块验证 | 不读取上 run 残留 |
| Pow 与候选位置 | 独立标量参考 | 全数值矩阵 | 仅测试 | epsilon、乘除次序、分类 |
| 回调/实时 best | 记录 Repair/Evaluate 的选择 fixture | 每测试 run | 仅测试 | 后续候选见到之前替换 |

## API 与行为变化

当前方案无新增公共 API；正态迁移、索引映射与显式预取布局改变旧 seed 轨迹。删除 Cuckoo 私有通用正态及 spare，保留 Lévy 配置/LogGamma、索引和选择职责。整数 SIMD 扩展不在本轮范围内，不能依据本 Draft 增加 Core API。

另有现状文档残留：user-guide.md 的随机能力说明仍列 Span<int> Fill，与已批准的 SPEC-0010 删除决定不一致。本轮记录该差异，不借此恢复 API；后续文档同步应以当前已批准决定修正。SPEC-0009 的旧伪 API 是被后续规格替代的历史内容，benchmark References 内旧 int Fill 属于历史参考，均不能作为生产依赖或恢复能力的理由。

## 替代与清理计划

删除 NextGaussian、_hasSpareGaussian、_spareGaussian 与 ResetForRun 的 spare 重置；历史私有公式只在基准 A 作为明确标注的参考存在。搜索 Cuckoo 源码中的 Gaussian、spare、Box–Muller、Math.Log/Math.Sin/Math.Cos，区分需保留的 ComputeLevySigma/LogGamma 参数计算，不能机械删除算法参数公式。

新增独立批次消费参考与回调日志，覆盖各 K 的活跃切片/尾部、策略消费两套状态、种群 1 不采样索引、遗弃率 0/1、顺序替换及动态 best；保留现有计数与隔离测试。小 P 穷举 first 与 P-1 个原始 second，验证映射为双射、无相等索引，不能只依赖随机统计。失败算术候选删除；强制正态迁移若无收益并与 NFR 冲突，退回 Spec 请求决策，不伪称全部完成。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 否 | 现有 StandardNormal.Fill/NextInt/Fill 足够 | Core 回归 |
| Algorithms | 是 | Cuckoo scratch/正态清理/私有算术 | 差分与残留检索 |
| Experiments | 实现否，验证是 | Group 独占不变 | 拆分/并发测试 |
| Examples | 签名否 | 旧 seed 不保证原结果 | 编译 |
| Tests | 是 | 样本布局、统计、选择与回调 | Release tests |
| Benchmarks | 是 | Lévy/遗弃 A/B/C、完整 run | 比值、分配、JIT |
| XML 文档 | 是 | 分布职责与兼容摘要 | DocFX |
| 用户/API 文档 | 是 | 实现状态与兼容边界 | 文档验证 |
| ENGINEERING | 建议方案否 | 不迁移分布职责 | 审查 |
| ADR | 否 | 既有 ADR-0025 覆盖算术，采样边界由本 Spec 修订 | 边界审查 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | 单候选/分块正态 Fill 角色切片、奇数 D、无 spare、固定统计/重复/复用 | Core 集成与优化器 | 消费轮数、分类和删除残留 |
| FR-002 | epsilon/Pow/缩放差分，种群 1、遗弃 0/1、索引映射双射与预取 | 内核与优化器 | 预定容差及决策事件 |
| FR-003 | Lévy 逐次替换/动态 best、遗弃冻结 worst 列表但实时读取 population/best、策略抽两套状态、Group/取消 | 优化器与 Experiment | 完整事件/后续随机状态 |
| NFR-001 | Lévy 与遗弃局部、索引方案独立及组合、首次及复用完整 run | BenchmarkDotNet | 共同门槛、各 K 的 scratch/索引载荷、JIT |

主要性能配置沿用默认指数 1.5、遗弃率 0.25、种群 64、LevyCandidateCount=2；诊断补充 exponent 0.5/1/1.99、遗弃率 0/1、种群 1/2 和全部诊断维度。每个被保留内核独立满足局部门槛，完整 run 检验组合效果。

## 风险和回退

主要风险为正态角色布局、Pow 数值误差、遗弃中的实时种群/best，以及预取缓冲成本。先通过同输入参考，分别计量索引映射、样本预取与算术融合。已批准的索引消费变化和显式样本预取不再视为范围未决；新分布 API、未获批准的分布变化、实际回调或选择行为变化仍须退回 Spec/ADR。

## ADR 判断

现有建议方案不新增 ADR，沿用 ADR-0019/0024/0025。整数 SIMD 若引入公开能力或改变分布职责，需重新判定并提交上游修订。

## 批准记录

- 计划批准：—
- 批准日期：—
- 范围确认（2026-09-15）：项目作者批准现有 NextInt 索引对方案及显式跨候选预取边界；不恢复公共整数 Fill，不等于整份 Plan 批准。
