# SPEC-0012 技术计划：Cuckoo 批量正态、Lévy 与遗弃算术

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`ce43725516d70d70edbe000ffc713e073adb0b94`
- 批准时 Spec 内容 SHA-256（UTF-8、LF）：`0ae5a332a3c093fc699be006f85157f5ea903bfc5ad4b74c6c5a2cb55cf8e9d5`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 创建日期：2026-09-14
- 修订日期：2026-09-15
- 批准人：项目作者
- 批准日期：2026-09-15

[共同验证计划](../simd-plan.md)是本 Plan 的组成部分，规定基线 A/B/C、数值通则、性能门槛与测量记录要求；本文件定义算法特有的设计。项目作者于 2026-09-15 批准本 Plan 及共同验证附件。Spec 基线提交记录已提交版本，本次批准还包含此前逐项确认的未提交 Spec 修订，以上内容摘要锁定批准时的完整 Spec；旧提交不能单独代表本次批准范围。

## 当前实现调查

- `src/Metaheuristics.Algorithms/Cuckoo/CuckooOptimizer.cs` 在 GenerateLevyCandidate 逐维调用两次私有 NextGaussian，保存 _hasSpareGaussian/_spareGaussian；随后调用单位 NextDouble 引导。
- 私有正态以 Math.Max(u, double.Epsilon) 生成半径，Core StandardNormal.Fill 已使用 1-u、双单位向量同 lane 配对、先 cos 向量后 sin 向量写出。迁移不能要求与旧 Gaussian 样本相同。
- Lévy 分母为 `Pow(Abs(normal)+1e-10, 1/LevyExponent)`，是加 epsilon，不是 Max 钳制。分子乘 _levySigma 再乘 GaussianScale，步长先乘 levyScale 再除分母；最后是 source + 0.8*step + 0.2*guidance。
- 遗弃候选先单值抽两个种群索引，种群大于 1 且索引相等时只重抽第二个。逐候选 Generate→Repair→Evaluate→条件替换→更新 best；后续候选可读到刚更新的种群和 best。
- Lévy 候选按 candidateIndex 直接对应 population，没有随机索引抽样；每个候选都立即 Repair、Evaluate、条件替换并更新 best，后续 Lévy 候选读取新的 best。全部 Lévy 完成后只调用一次 FindWorstIndices，冻结本轮遗弃目标索引列表，但遗弃输入 population/best 仍随前一替换实时变化。排序及 tie 顺序保持，不优化排序。现有工作区为 population、candidates、sortedIndices 与 best。
- CuckooOptimizerTests 已有评估/Repair 计数、单巢、复用及隔离测试，尚缺精确抽样消费、回调事件、立即 best 更新与“冻结 worst/live population”覆盖；这些测试需新增。RandomMigrationBenchmarks 提供完整 run，CuckooRandomDiagnosticsBenchmarks 可辅助诊断，需独立新增同样本候选内核对照。

## 整数采样范围与独立候选

2026-09-15 项目作者确认：本轮使用现有 NextInt 逐候选生成不同索引对，不做索引预取或新增索引缓冲，不恢复 SPEC-0010 已删除的 `Fill(Span<int>, int, int)`，不新增公共整数 SIMD API。Core 继续唯一拥有通用无偏整数分布；后续若有证据支持新增整数 SIMD 能力，另行修订 Core 规格。本轮范围问题已关闭，整份 Plan 已于 2026-09-15 获批；本次只落实规划审批，不运行实验。

种群大小 P>1 时，候选方案先取 `first=NextInt(0,P)`，再取 `second=NextInt(0,P-1)`；若 `second>=first` 则 `second++`。对每个 first，该映射将 P-1 个等概率值一一对应到其余索引，保持有序不同索引对的均匀分布。它消除算法层的相等索引重抽，不保证两次底层状态推进，因为 Core NextInt 内部仍可能拒绝采样。P=1 时直接返回 (0,0)，不消费索引样本；遗弃数量为 0 时不消费任何遗弃样本。

索引映射作为独立候选，与原重抽方式比较，不能把它的收益计入正态批量化或算术 SIMD。新旧映射允许改变策略后来取得的随机值，不能以此声称破坏同版本可复现性。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：私有正态与原逐维算术 | 直接生产基线 | 分布重复、标量采样 | 不满足迁移完成条件 | 仅基线 |
| B：StandardNormal.Fill/单位 Fill + 原算术 | 清理分布职责，隔离批量收益 | 3D scratch、旧轨迹改变 | 正态/角色布局 | 首个候选 |
| C1：直接 TensorPrimitives | 可利用现有适用操作 | 操作覆盖范围有限 | 超越函数误差 | 首个候选 |
| C2：保留 Math.Pow，私有融合其余算术 | 减少遍历、限制数学变化 | 私有级联与 scratch 阶段 | 溢出/epsilon/乘除次序 | 无适用直接操作或未过门槛时评估 |
| C3：Lévy 分母用向量 Log→乘法→Exp 并融合后续算术 | 分母也进入向量计算链 | 输入域判断、数值验证及必要回退 | 不等价于通用 Pow，边界与误差需验证 | 受限输入域候选，独立评估 |
| I-map：逐候选不同索引对映射 | 消除外层相等索引重抽 | 标量状态消费改变 | 仍须实时读取 population/best | 独立评估 |
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

单候选 B 使用一块 `double[3D]`，每 Optimizer 载荷 24D 字节：normalNumerator[D]、normalDenominator[D]、unit[D]。EnsureWorkspace 首次分配，跨 run 复用且无需清零或额外重置；不创建新随机源，不保留正态 spare。每次读取前完整写入本次有效区间，只读取本次已写入范围。

每个 Lévy 候选，在现有调用位置按顺序执行：

1. 一次 StandardNormal.Fill，目标为 scratch 前 2D 个元素；前 D 个分配给 numerator、后 D 个给 denominator，不做跨候选缓存。消费 `2*ceil(D/L)` 轮批量状态。
2. 一次 RandomSource.Fill(unit)，消费 ceil(D/L) 轮批量状态。
3. 对当前 source 和当前 best 计算原 Lévy/引导公式，然后原位调用 Repair。合计 `3*ceil(D/L)` 轮，不消费标量状态；即使尾部不满向量，也只由 Core 的本次 Fill 处理。

布局不要求每个向量恰好对应同一随机角色；角色按输出 Span 的前后 D 分配。B/C 共用完全相同的两次 Fill 与数组切片，禁止为了更快把单次 2D 正态拆成两次 D 调用而静默改变序列。

遗弃候选以原索引重抽作为基线，独立评估上一节的新映射。索引选定后读取当时的 population，RandomSource.Fill(unit) 一次，再计算：
`best + ((0.5*decayFactor)*(first-second)) + ((unit-0.5)*AbandonmentPerturbationScale)`。
新映射在种群为 1 时跳过原来的两次索引调用；扰动系数为零时仍保留 unit Fill。abandonmentCount=0 时不进入该路径，不能多消费样本。逐候选 Repair/Evaluate/替换/best 更新原位保留，不缓存下一候选的输入快照。

本 Plan 仅采用上述单候选布局。一个候选完成 Generate/Repair/Evaluate/替换/best 更新后，再为下一个候选覆盖复用样本缓冲。遗弃阶段只在全部 Lévy 完成且 FindWorstIndices 冻结目标列表后开始；逐候选生成的索引用于读取当时的实时 population，不能把冻结 worst 列表误解成冻结种群内容。NextInt 与 Fill 分别推进标量与批量状态，测试覆盖策略也消费这两套状态时的后续值。

各样本角色只读取本次已写入的 D 个元素，first/second 为本次候选生成的局部值，不增加索引数组。取消、异常后数组内容可保留，无需清理，也不推迟原回调处的取消检查。算术 B/C 对照采用相同索引方案和同一样本角色布局。

## 算术阶段与数值准则

B 的算术保留 Math.Pow 及原括号顺序。C1 首先检查目标框架 TensorPrimitives 是否有可直接表达目标操作的适用操作。C2 在没有适用直接操作或未通过门槛时评估：分母阶段独立形成 `Pow(Abs(normal)+1e-10, reciprocalExponent)`，再由 double 512/256/128/scalar 级联融合分子缩放、步长、引导与位置写回；遗弃内核独立融合差分和扰动。

当前项目引用 System.Numerics.Tensors 10.0.11。该包虽有 TensorPrimitives.Pow 批量 API，但对应[官方源码](https://github.com/dotnet/dotnet/blob/e2f47b0110ed922f21a1522da67279133ce28f32/src/runtime/src/libraries/System.Numerics.Tensors/src/System/Numerics/Tensors/netcore/TensorPrimitives.Pow.cs)的 PowOperator.Vectorizable 为 false，实际采用标量 T.Pow；不能把调用该 API 视为 Pow 已向量化。其向量重载已写出 Exp(p*Log(x))，所用 Log/Exp 在当前框架具有向量路径。禁用所关联的[官方问题](https://github.com/dotnet/runtime/issues/100535)包含负底数与整数指数的反例，不能直接把这条计算链作为通用 Pow。

C3 仅评估本算法的 Lévy 分母：先按原次序得到 `x = Abs(normal) + 1e-10`、`p = 1 / LevyExponent`，再在 Algorithms 私有 double 512/256/128 级联内使用现有 Vector.Log、向量乘法与 Vector.Exp，继续计算步长、引导与目标位置。正常有限样本使 x 严格为正，但这只是候选前提，不自动保证相对 Math.Pow 的误差与边界分类。该求值改写是 SPEC-0012 FR-002 明确允许的局部例外，其余 Abs/epsilon、分子缩放、乘后除及位置求和顺序不变。不自写 Log/Exp 近似，不新增通用 Pow helper、公开入口或运行时实验开关。

Math.Pow 始终作为数值参考；C2 保留其标量阶段。C3 的候选快路径要求 x 为正有限数、p 为正有限数；不满足时使用 Math.Pow，标量尾部也使用 Math.Pow。p 的条件可按配置在候选循环外判断，x 的异常 lane 可按 lane 或整个向量块回退，具体方式在数值验证阶段固定。极小但合法的 LevyExponent 可能使 p 为 Infinity，不能据此拒绝原有合法 Options。即使 x/p 有限，靠近 1 的底数、较大指数、指数乘积及 Exp 的上溢/下溢边界仍须验证；若基本条件不足以满足既定误差和分类要求，先确定可验证的进一步快路径范围及 Math.Pow 回退条件，再锁定方案进行性能评估。不能通过放宽数值门槛保留失败候选。

C2/C3 的完整候选计时分别计入 Math.Pow 阶段或向量 Log/乘法/Exp、范围判断、回退、尾部及全部 scratch 成本。使用相同样本与算术比较 C2/C3，隔离分母改写的增量；二者仍分别满足共同计划的 B/C 与 A/C 门槛。失败 C3 删除；只有已独立通过门槛的 C2 才能作为生产回退方案，不能只报告融合最后几条运算。

事前数值规则：

- 每个有限 Pow 结果相对误差不超过 1e-12，另加 1e-14 绝对预算；NaN/Infinity 分类、Infinity 与零符号一致，禁止一边有限一边溢出。
- 同正态与单位样本的候选位置误差 `<= 1e-12 + 1e-11*(abs(source)+abs(0.8*step)+abs(0.2*guidance))`；遗弃采用对应三项绝对值之和。中间项必须先满足分类检查；若误差尺度求和溢出，改用按最大有限项归一化的检查，不能以无限容差放行。
- 保持 Abs 后加 epsilon、分子两次乘法、乘后除及 0.8/0.2 加法顺序；不显式 FMA，不改成 Max、分母倒数相乘或合并常量而改变求值次序。
- 诊断 exponent 为 0.5、1、1.5、1.99，并覆盖接近 (0,2) 两端的合法配置、正态 0/次正规/极值/NaN/Infinity、unit 0/最大小于 1、奇数维及尾部。已存在的极端配置溢出行为不静默“修复”。
- C3 增补 x 接近 epsilon、等于/紧邻 1、极大有限值，p 很大但有限及倒数溢出为 Infinity 的合法配置，覆盖结果的零/次正规/正常有限/Infinity 分界与混合正常、回退 lane。对受限计算 helper 另测零、负底数、NaN/Infinity 输入确实回退到 Math.Pow。记录实际快路径范围和检查谓词，比较完整向量块、标量尾部与回退后的分类和误差，不能仅用普通正态样本证明边界正确。
- 正态统计沿用 Core 已有质量门槛。算法角色切片补充固定 seed 0、1、20260905，每 seed 2^18 对输入，分子/分母各自均值绝对值 <=0.02、方差与 1 偏差 <=0.03、角色相关系数绝对值 <=0.02；固定 D=1、7、32 的实际批次布局各测，取足样本后截断统计。失败不换 seed 重试。Lévy 重尾不使用有限方差假设，主要以同输入参考逐值差分及正负计数检验（7σ）验证角色接线。

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Options/指数/种群 | 原构造器 | 一次 | 否 | 原参数异常 |
| 正态分布/状态 | Core 已有测试+角色集成测试 | 测试阶段 | 无重复逐值验证 | 删除 spare 后仍正确配对 |
| 输入切片/活跃区 | 工作区与哨兵测试 | 首次/测试 | 无逐块验证 | 有效区间先写后读，读取不超过本次已写入范围 |
| Pow 与候选位置 | 独立标量参考 | 全数值矩阵 | 仅测试 | epsilon、乘除次序、分类 |
| 回调/实时 best | 记录 Repair/Evaluate 的选择 fixture | 每测试 run | 仅测试 | 后续候选见到之前替换 |

## API 与行为变化

当前方案无新增公共 API；正态迁移、索引映射与单候选批次布局改变旧 seed 轨迹。删除 Cuckoo 私有通用正态及 spare，保留 Lévy 配置/LogGamma、索引和选择职责。整数 SIMD 扩展不在本轮范围内，不能依据本 Plan 增加 Core API。

另有现状文档残留：user-guide.md 的随机能力说明仍列 Span<int> Fill，与已批准的 SPEC-0010 删除决定不一致。本轮记录该差异，不借此恢复 API；后续文档同步应以当前已批准决定修正。SPEC-0009 的旧伪 API 是被后续规格替代的历史内容，benchmark References 内旧 int Fill 属于历史参考，均不能作为生产依赖或恢复能力的理由。

## 替代与清理计划

若 C3 通过评估并采用，说明与代码一起交付：

- 在 `VectorOps.simd.cs` 的 Lévy 分母实现附近和调用方输入构造/范围判断处写维护注释：列明 `Abs(normal)+1e-10` 与指数前提、实际快路径范围、Math.Pow 回退条件、非通用 Pow 的反例，以及 SPEC-0012 FR-002、数值测试和 Verification 的位置。注释放在维护的模板源中，不只写在生成结果或提交说明里。
- 在开发者指南的 Cuckoo 数值实现说明中解释此局部变换的适用性和限制，链接 Spec 与 Verification；最终误差门槛、验证输入范围、回退规则和局部/端到端结果记录于 Verification。更改 epsilon、指数范围、Log/Exp 实现或把 helper 用于其他公式时，必须重新核对前提并验证，不能沿用旧结论。
- 若影响需要向调用方说明的数值行为，在相关公共 XML 备注中给出必要摘要；私有内核的维护细节留在源码注释和开发者文档。未采用时仅在 Verification 记录评估结论，不把候选写成已实现能力。

删除 NextGaussian、_hasSpareGaussian、_spareGaussian 与 ResetForRun 的 spare 重置；历史私有公式只在基准 A 作为明确标注的参考存在。搜索 Cuckoo 源码中的 Gaussian、spare、Box–Muller、Math.Log/Math.Sin/Math.Cos，区分需保留的 ComputeLevySigma/LogGamma 参数计算，不能机械删除算法参数公式。

新增独立批次消费参考与回调日志，覆盖单候选角色切片/尾部、策略消费两套状态、种群 1 不采样索引、遗弃率 0/1、顺序替换及动态 best；保留现有计数与隔离测试。小 P 穷举 first 与 P-1 个原始 second，验证映射为双射、无相等索引，不能只依赖随机统计。失败算术候选删除；强制正态迁移若无收益并与 NFR 冲突，退回 Spec 请求决策，不伪称全部完成。

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
| FR-001 | 单候选正态 Fill 角色切片、奇数 D、无 spare、固定统计/重复/复用 | Core 集成与优化器 | 消费轮数、分类和删除残留 |
| FR-002 | epsilon/Pow/缩放差分，C3 受限输入域、分类边界及回退，种群 1、遗弃 0/1、逐候选索引映射双射；采用后的注释/文档核查 | 内核、优化器与文档审查 | 预定容差、回退谓词、决策事件及维护说明 |
| FR-003 | Lévy 逐次替换/动态 best、遗弃冻结 worst 列表但实时读取 population/best、策略抽两套状态、Group/取消 | 优化器与 Experiment | 完整事件/后续随机状态 |
| NFR-001 | Lévy 与遗弃局部，C2/C3 分母增量，索引方案独立及组合，首次及复用完整 run | BenchmarkDotNet | 共同门槛、24D scratch 载荷、范围判断/回退成本、JIT |

主要性能配置沿用默认指数 1.5、遗弃率 0.25、种群 64、LevyCandidateCount=2；诊断补充 exponent 0.5/1/1.99、遗弃率 0/1、种群 1/2 和全部诊断维度。每个被保留内核独立满足局部门槛，完整 run 检验组合效果。

## 风险和回退

主要风险为正态角色布局、C3 误用为通用 Pow 或越过验证输入域、Lévy 数值语义、遗弃中的实时种群/best，以及批量采样缓冲成本。先通过同输入参考，分别计量索引映射、批量采样与算术融合。新分布 API、未获批准的分布变化、实际回调或选择行为变化仍须退回 Spec/ADR。

## ADR 判断

现有建议方案不新增 ADR，沿用 ADR-0019/0024/0025。整数 SIMD 若引入公开能力或改变分布职责，需重新判定并提交上游修订。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-09-15
- 范围确认（2026-09-15）：项目作者批准现有 NextInt 索引对方案及显式跨候选预取边界；不恢复公共整数 Fill，不等于整份 Plan 批准。
- 当前布局选择（2026-09-15）：项目作者同意先采用单候选批量正态/单位采样、逐候选索引映射，保留 Math.Pow 作为基线；本 Plan 据此收敛。
- 数值候选与说明要求（2026-09-15）：项目作者同意评估 C3 受限输入域的向量 Log→乘法→Exp 融合，并要求采用后在文档与实现注释中明确适用边界、误差及回退条件；不是对候选收益或整份 Plan 的提前批准。
- 验收标准确认（2026-09-15）：项目作者确认共同验证计划的性能门槛、验证负载及本 Plan 的数值预算；该次确认仅针对验收标准。
- 整体批准（2026-09-15）：项目作者通过“批准plan”批准本 Plan 和共同验证附件；候选采用仍须通过既定数值与性能门槛。该次仅记录批准，尚未创建 Tasks 或启动实现/实验。
- 任务拆分（2026-09-16）：按项目作者授权建立 [Tasks](./tasks.md) 和 [Verification 模板](./verification.md)，任务及验证结果均为 Pending；尚未启动实现或实验。
- 执行补充（2026-09-17）：项目作者启动连续实施；受限 Vector128 路径只要求不弱于同公式标量参考，具体边界见共同附件，其他数值及性能门槛不变。
