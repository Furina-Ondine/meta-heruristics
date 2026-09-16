# SPEC-0013 技术计划：PSO 批量采样、整代系数预取与候选融合

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`ce43725516d70d70edbe000ffc713e073adb0b94`
- 批准时 Spec 内容 SHA-256（UTF-8、LF）：`2f9a8b42d1278953510e0ed8bcf2b129327d0c1baee59e80e506fd18ad69128f`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 创建日期：2026-09-14
- 修订日期：2026-09-15
- 批准人：项目作者
- 批准日期：2026-09-15

[共同验证计划](../simd-plan.md)是本 Plan 的组成部分，规定基线 A/B/C、数值通则、性能门槛与测量记录要求；本文件定义算法特有的设计。项目作者于 2026-09-15 批准本 Plan 及共同验证附件。Spec 基线提交记录已提交版本，本次批准还包含此前逐项确认的未提交 Spec 修订，以上内容摘要锁定批准时的完整 Spec；旧提交不能单独代表本次批准范围。

2026-09-15，项目作者已批准 Spec FR-002 的采样规则修订及本 Plan 的具体布局。本次落实 Plan 批准记录，不启动实现或实验。

## 当前实现调查

- `src/Metaheuristics.Algorithms/Pso/PsoOptimizer.cs` 的 ResetForRun 每粒子执行 Initializer→Repair→逐维初始化 Velocity→Evaluate，再建立 personal/global best。
- GenerateCandidate 只调用两次标量 NextDouble，依次作为 cognitive/social；各乘一次系数后广播至所有维度。随后调用 VectorOps.ComputePsoVelocity、TensorPrimitives.Clamp、TensorPrimitives.Add，最后 Repair 并复制 personal best。
- Advance 先生成全部候选，再全部 Evaluate，再更新 personal/global best 和种群角色。惯性只在代际入口计算。
- `VectorOps.simd.cs` 已有 double 512→256→128→scalar 速度级联，`VectorOps.cs` 仅声明 partial 类。现有 PsoBenchmarks 含 Scalar、TensorPrimitives、VectorOps 内核及旧标量/生产完整 run；不能把旧 scalar baseline 当本轮增量基线。
- PsoOptimizerTests、VectorOpsTests、TensorPrimitivesPsoTests 已覆盖公式、特殊值、回调、抽样顺序和相等边界；需要更新初始化改为批量后的样本参考。
- ADR-0025 已替代历史 Clamp/Add 必须使用 TensorPrimitives 的限制；无需为允许融合再次改 ADR。SPEC-0010 的 Vector<T> 限制只针对 Core 随机状态。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：当前已有 SIMD + Clamp/Add | 成熟直接生产基线 | 三次遍历 | 无 | 基线与失败回退 |
| B-init：只批量初始化速度 | 无需新增工作区 | 短 run 受播种成本影响 | 旧轨迹改变 | 独立候选 |
| B-coeff：整代系数批量预取，原算术 | 扩大单位随机批次 | 2P 个 double 的读写与持久工作区 | 算法与策略样本重新分配 | 重新纳入独立候选 |
| C1：直接 TensorPrimitives | 简单、可比较 | 操作覆盖范围有限 | 必须保留求值次序 | 首个候选 |
| C2：速度+Clamp+位置融合级联 | 减少读写与遍历 | 新的双输出内核 | Clamp 特殊值/别名 | 无适用直接操作或未过门槛时评估 |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 随机状态与有界映射 | Core | Core | 使用已完成的 Fill |
| 每粒子两随机系数 | PsoOptimizer 即时采样 | PsoOptimizer 即时采样或整代私有缓冲 | 角色布局仍由算法拥有，保持跨维度相关结构 |
| 无状态速度与位置计算 | VectorOps + TensorPrimitives | 通过门槛的直接 TensorPrimitives 或私有融合内核 | 私有算术职责 |
| personal/global best、工作区 | PsoOptimizer | PsoOptimizer | 不跨阶段迁移状态 |

## 采样布局与融合设计

初始化候选 B-init：每粒子 Initializer→Repair 返回后，非相等速度范围直接 RandomSource.Fill(particle.Velocity, lower, upper)，恰好 ceil(D/L) 轮批量状态；相等范围用 Span.Fill(constant)，不调用随机源。随后 Evaluate，再进入下一粒子。沿用既有 Velocity 数组，新增持久载荷为 0。当前候选仍采用每粒子一次 Fill，这是具体布局选择，不再以回调本身作为禁止其他布局的理由。

系数基线：GenerateCandidate 依次调用两次标量 NextDouble，合成 cognitiveScale/socialScale 后广播。即使对应 coefficient=0，仍保留两次抽样。该路径用作对照及无收益时的回退。

整代系数候选 B-coeff 的拟议布局如下，其中 P 为种群大小、L 为 Core 批量 lane 数：

1. 若最终采用该候选，在 EnsureWorkspace 为 Optimizer 分配一个 double[2P]，载荷 16P 字节；不按维度 D 放大，不缓存随机源，不与其他 Group 共享。
2. 每次 Advance 在原有 run 初始化检查与惯性计算后、首个 GenerateCandidate 前，调用一次 RandomSource.Fill 覆盖整个缓冲。该调用消耗 ceil(2P/L) 轮批量状态，不推进单值状态；不添加跨代预取，也不移动原有取消检查。
3. 固定交错布局为 [cognitive0, social0, cognitive1, social1, ...]。粒子 i 读取索引 2i 和 2i+1，分别乘原有系数后对全部维度广播。系数为零仍生成并分配对应样本；不把同一对样本复用于多个粒子。
4. Core 按既有 Fill 规则丢弃调用尾轮未写出的 lane，不用标量补尾，不拆成两个 P 长调用。Repair 若也使用批量入口，会从整代预取之后的批量状态继续；仅使用单值入口的策略不被本次 Fill 直接推进，但相对旧版的单值消费总量已变化。
5. 只提前生成随机样本。粒子生成与 Repair、personal best 复制、全体 Evaluate、随后 personal/global best 更新保持原顺序；不能预先计算跨越这些时点的位置或 best 快照。初始化策略的实际调用顺序同样不变。
6. 缓冲跨 run 复用，无需每 run 或每代清零，也不增加额外重置。每代读取前由上述 Fill 完整写入 2P 个样本，只读取本次已写入范围；粒子索引为本次循环的局部变量。取消或异常后数组内容可保留，无需清理，不回滚随机源；原有取消检查与异常后实例不可复用契约保持。

B-init 与 B-coeff 分别和 A 对照，再评估二者组合。算术融合 C 必须与相同采样布局的 B 对照，避免把整代预取的收益算入融合。B-coeff 的局部计时涵盖整代 Fill、缓冲读写及 P 个候选的相关调用，不仅测两个数组索引读取；同时报告每代与每粒子摊销成本。沿用共同门槛，完整 run 计入 16P 工作区的首次分配及后续复用；各项候选分别可以删除。

融合内核拟为 Algorithms 私有 `UpdatePsoCandidate`，输入 source Position/Velocity、personal/global best、惯性、两个已缩放系数及速度上下界，输出 target Velocity 与 target Position。保持现有求值结构：

```text
v = ((inertia * sourceVelocity)
     + (cognitiveScale * (personalBest - sourcePosition)))
     + (socialScale * (globalBest - sourcePosition))
bounded = clamp(v, lower, upper)
targetVelocity = bounded
targetPosition = sourcePosition + bounded
```

通用 512/256/128 完整块加手写标量尾部，硬件门由现有模板展开。加载本块所需 source 后写两个 target，来源和目标数组由 PsoState 的双工作区隔离；不增加公开 Span API 或重叠支持契约。Clamp 必须匹配现有生产组合与标量参考的 NaN、Infinity、边界和零符号规则，不能未经测试直接用可能改变特殊值的 Min/Max 组合。保持表达式顺序且不显式 FMA。

融合只替换 GenerateCandidate 的三次算术调用；Repair、personal best 复制、全体 Evaluate 与随后 best 更新原位保留。惯性计算不移进内核。

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Options/速度上下界 | 原构造器 | 一次 | 否 | 原异常类型与零宽合法性 |
| 输入等长、双输出隔离 | 工作区创建与测试 | 首次/测试 | 无逐块验证 | 源状态不被覆盖 |
| 两系数及速度/位置 | 独立标量公式与当前生产组合 | 全诊断维度 | 仅测试 | 不变为逐维随机系数 |
| 初始化/整代 Fill 与回调 | 独立流参考/记录策略 | 每测试 run | 仅测试 | 固定角色及状态推进，实际回调/状态更新顺序不变 |

## API 与行为变化

拟新增通过门槛的私有融合内核或整代系数缓冲；初始化与整代系数分别决定是否改用批量消费。删除通过验收后失去消费者的旧 PSO 内核与私有 NextDouble helper。公共签名和调用方组装保持，旧 seed 轨迹不作为兼容承诺。同环境新 seed 重复、Group 隔离、实际回调/状态更新顺序及原异常/取消规则保持。

## 替代与清理计划

先全仓检索 ComputePsoVelocity 的生产、测试和 benchmark 消费者。融合通过后，将生产旧入口删除，并将历史对照移至 benchmark 专用参考；测试改为验证完整融合结果，不为保留历史 benchmark 而保留无生产消费者的 runtime 内核。若融合失败保留原路径并删实验内核。

更新 PsoOptimizerTests 的初始化随机参考；若采用 B-coeff，把“每次 GenerateCandidate 必须调用两次标量 NextDouble”的断言改为交错批次布局与每粒子一对系数广播断言，不将旧消费顺序当作兼容要求。TensorPrimitivesPsoTests 中仍有真实参考价值的测试保留；重复且只镜像已删除生产入口的测试改写。失败的预取候选连同无消费者缓冲删除。更新架构概览的实际选定路径和兼容说明，不改 Options 示例。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 否 | 使用现有单位与有界 double Fill | Core 回归 |
| Algorithms | 是 | PsoOptimizer、VectorOps.simd.cs | 公式、生成器测试 |
| Experiments | 实现否，验证是 | 独立 Group 不变 | 拆分/并发测试 |
| Examples | 签名否 | 编译及 seed 说明 | Release build |
| Tests | 是 | 初始化、整代系数布局与融合差分 | Release tests |
| Benchmarks | 是 | 当前生产 A/B/C 对照 | 内核、Reset、完整 run |
| XML 文档 | 是 | 环境/轨迹必要摘要 | DocFX |
| 用户/API 文档 | 是 | 演进状态与兼容说明 | 文档验证 |
| ENGINEERING | 否 | 既有约束继续有效 | 审查 |
| ADR | 否 | ADR-0025 已允许融合 | 审查 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | 同两系数的 Velocity/Position 双输出差分、Clamp 端点与特殊值、personal/global best 更新顺序 | 内核与优化器 | 共同数值准则及事件表 |
| FR-002 | 初始化范围/相等范围不消费、整代交错角色与尾轮、策略分别消费两类随机状态、两系数共享、固定 seed/复用/Group/取消/异常 | 优化器与 Experiment | 角色/状态推进参考、实际回调事件和生命周期证据 |
| NFR-001 | B-init、B-coeff 及其组合分别评估；C 与同布局 B 对照；首次/复用完整 run | BenchmarkDotNet | 每个保留改动的增量门槛与 16P 字节成本 |

诊断补充惯性最小值/衰减、cognitive/social 为零、相等速度上下界、越界速度、非整倍数末尾和 NaN/Infinity。不同维度的零宽范围均不能消费 Fill。纯融合且使用同一随机输入时须先通过局部差分再执行性能测量。

整代布局另外覆盖 P=1、2、3、7、8、15、16、31、32、33、64，以及使 2P 跨越实际 L 边界的种群大小。日志策略分别不采样、使用单值采样、使用批量采样；按新布局预测后续样本，不要求与旧策略样本相同。复用测试确认每代 2P 个样本先完整写入再读取；取消/异常 fixture 覆盖首个和中间粒子的 Repair、随后 Evaluate，确认异常传播、评估计数与合法复用规则保持。

## 风险和回退

融合最主要风险是 Clamp 分类漂移与读写顺序；预取主要风险是角色索引错误、误移动实际回调/状态更新，以及小种群下批量与缓冲开销。各候选依据独立和组合门槛选择，允许只保留初始化、系数预取或融合中的有效部分，也允许全部不采用并如实报告。不得用单纯标量速度参考的巨大比值替代相对现有 SIMD 的增量证据。

## ADR 判断

不新增 ADR。若需要改变两系数共享、公共后端、算法布局、ISA 专属运算或 Core 随机接口，退回 Spec/ADR。

本次修订只改变 SPEC-0013 的随机样本调度约束；实际回调与状态更新时点不变，符合 ADR-0025。该许可不自动扩展到 Bat、Cuckoo 或 Firefly。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-09-15
- 验收标准确认（2026-09-15）：项目作者确认共同验证计划的性能门槛、验证负载及本 Plan 的数值预算；该次确认仅针对验收标准。
- 整体批准（2026-09-15）：项目作者通过“批准plan”批准本 Plan 和共同验证附件；候选采用仍须通过既定数值与性能门槛。该次仅记录批准，尚未创建 Tasks 或启动实现/实验。
- 任务拆分（2026-09-16）：按项目作者授权建立 [Tasks](./tasks.md) 和 [Verification 模板](./verification.md)，任务及验证结果均为 Pending；尚未启动实现或实验。
