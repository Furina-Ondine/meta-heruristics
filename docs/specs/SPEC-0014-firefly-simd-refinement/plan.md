# SPEC-0014 技术计划：Firefly 批量随机游走与缩放融合

## 元数据

- 状态：`Draft`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`0a74f08592c89b0c09f5e242bb3c111c7f967c7b`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 创建日期：2026-09-14
- 修订日期：2026-09-15
- 批准人：—
- 批准日期：—

[共同验证计划](../simd-plan.md)是本 Plan 的组成部分，规定基线 A/B/C、数值通则、性能门槛与测量记录要求；本文件定义算法特有的设计。下列方案均待本 Plan 批准。

## 当前实现调查

- `src/Metaheuristics.Algorithms/Firefly/FireflyOptimizer.cs` 的 GenerateCandidate 首先复制 source.Position 至 target；依次遍历 sourcePopulation，用 source.Evaluation 判断吸引资格。
- 每个合格 attractor 先逐维写 `randomStep * (NextDouble()-0.5)` 至已有 _randomWalk，再以当前 target.Position 计算 VectorOps.DistanceSquared、标量 Math.Exp 的 attractiveness，调用 UpdateFireflyPosition，立即 Repair。
- source 的 Evaluation 不随本候选移动更新；target.Position 在每次移动/Repair 后变化，下一吸引者必须读新值。Advance 仍全体生成后统一 Evaluate 和选择。
- 当前距离及位置都有固定宽度 SIMD；`FireflyBenchmarks.cs` 包含 Scalar/TensorPrimitives/VectorOps 内核与完整 run 参考。新基线必须选当前 VectorOps 生产路径。
- FireflyOptimizerTests 已测试每次 Repair、固定 seed、工作区复用及逐 attractor/逐维随机顺序。最后一项需改为新批次参考，并保留吸引顺序保护。
- 当前无规格冲突；不需要重写已验证的距离归约，也不把 Core Vector<T> 限制误用于现有算法级联。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：逐维随机缩放+现有 SIMD | 当前生产基线 | 标量采样及 scratch 遍历 | 无 | 基线 |
| B：Fill(_randomWalk)+原缩放+原位置内核 | 隔离批量收益 | 仍有缩放遍历 | 批次尾部改变序列 | 首个候选 |
| C1：直接 TensorPrimitives | 使用现成适用操作 | 操作覆盖范围有限 | 必须保持 in-place 语义 | 首个候选 |
| C2：单位随机缩放并入位置级联 | 少一次 scratch 读写 | 扩展位置内核参数 | 缩放次序/alias | 无适用直接操作或未过门槛时评估 |
| Bk：同一 source 内按原顺序分块预取合格吸引子 | 一次 Fill 覆盖多个实际移动 | 需要索引、尾块和取消处理 | 预取样本与逐移动状态必须分离 | 候选，K=1/4/8 待本 Plan 批准 |
| 一次预取整个源候选的所有合格吸引子 | Fill 次数最少 | 工作区随种群规模增长 | 最大载荷大于固定 K 分块 | 不纳入本 Plan |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 随机状态与单位分布 | Core | Core | 使用现有 Fill |
| 每次实际移动的采样与缩放系数 | Firefly | Firefly | 算法行为 |
| 距离与位置算术 | VectorOps | 通过门槛的直接 TensorPrimitives 或私有融合内核 | 现有私有职责 |
| 吸引资格、顺序和 Repair | FireflyOptimizer | FireflyOptimizer | 逐移动时序不能跨越 |

## 采样布局与算术设计

每个 source 候选先按现有 EvaluationComparer 与 source.Evaluation，沿 sourcePopulation 的原顺序筛出合格 attractor。不合格 attractor 不采样、不移动、不 Repair。B1 逐移动基线在每个合格 attractor 确认后调用一次 `RandomSource.Fill`，填充 D 个单位样本，再完成该移动并立即 Repair。

Bk 是同一 source 候选内的有界分块预取候选，K 为待本 Plan 批准的比较参数 `1、4、8`。按原 attractor 顺序收集最多 K 个合格 attractor 的索引到 `int[K]`，设本块实际数量为 `m`（`0 <= m <= K`），然后只调用一次 `RandomSource.Fill` 填充连续的 `mD` 个单位样本。该调用消费 `ceil(mD/L)` 轮批量状态，不消费标量随机状态；如果用户策略分别消费单值和批量两套随机状态，需验证两套状态的推进和隔离。第 j 个合格 attractor 使用样本缓冲区的 `[jD, (j+1)D)` 切片；按索引数组顺序逐个计算距离和 attractiveness、更新位置并立即 Repair，再消费下一个切片。每个块的随机角色仍是一个移动对应 D 个样本，不合格 attractor 不占样本；不得把多个移动的样本误当成一个移动的距离或位置输入。

Bk 的可复用批次 scratch 由 `double[K*D]` 和 `int[K]` 组成，共 `8KD + 4K` 字节；相对于 B1 的 `double[D]`，它复用并扩展原 `_randomWalk`，新增载荷为 `8D(K-1) + 4K` 字节。采用 Bk 时删除被替代的独立旧 `_randomWalk[D]` scratch，不与批次缓冲并存或重复计量；不建立随整个 sourcePopulation 增长的 `P*D` 或 `P*P*D` 缓冲。最后不足 K 个合格 attractor 时只填充 `mD`，不填充容量余量；没有合格 attractor 时不调用 Fill、不移动、不 Repair。取消或异常发生在收集、Fill 或逐移动消费期间时，当前块未消费的样本立即作废，不延续到下一个 source、代或 run；沿用现有取消检查、异常传播和 Optimizer 生命周期。

B 在 scratch 上保留逐维缩放 `randomStep * (u-0.5)`，随后调用原距离和原位置内核。C 保留 scratch 为单位样本，扩展私有位置内核的 randomStep 参数，按原括号计算：

```text
walk = randomStep * (unitRandom - 0.5)
movement = (attractiveness * (attractorPosition - currentPosition)) + walk
destination = currentPosition + movement
```

B1 沿用原有 `_randomWalk[D]`，新增持久载荷为 0；Bk 使用上述 `double[K*D]` 与 `int[K]` 批次 scratch，当前 D 切片可直接作为移动内核的随机游走输入，不额外复制整块。目标为精确 in-place，当前块所需 current 必须加载后再存回；不支持任意部分重叠。距离仍用既有 DistanceSquared，读取上次 Repair 后 target.Position；attractiveness 的 Math.Exp 仍由 Optimizer 在本次移动中计算，不能预先对全部 attractor 计算距离或吸引力。

即使 randomStep=0 或 attractiveness=0，只要 attractor 合格，仍为该移动分配 D 个样本并执行原移动公式和 Repair；B1 通过该移动自己的 `Fill(D)` 分配，Bk 在块级 `Fill(mD)` 中包含该移动的 D 个样本。不引入“系数为零便跳过”的随机语义优化。RandomStep 的代际衰减保持现状。Initializer 与全体 Evaluate 阶段不改动。

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Options | 原构造验证 | 一次 | 否 | 原异常与零系数合法性 |
| scratch/目标长度与别名 | 原工作区及内核测试 | 首次/测试 | 不逐块验证 | 不覆盖未读取位置 |
| 同样本缩放/位置 | 标量参考与现有生产组合 | 诊断矩阵 | 仅测试 | 浮点分类及括号 |
| 多 attractor 与策略状态 | 修改位置的记录 Repair | 每测试 run | 仅测试 | 吸引者顺序、下一距离读修复后位置 |

## API 与行为变化

新增只限通过门槛的私有位置内核参数/入口和 Bk 所需的 Optimizer 私有 scratch；修改随机样本的批次布局及 `_randomWalk` 的使用路径。公共签名和用户组装保持；旧 seed 轨迹允许变化。吸引资格、吸引者顺序、逐移动距离/吸引力计算、移动和 Repair 顺序、Evaluation 阶段、Group/取消契约不变。

## 替代与清理计划

融合通过后删除旧生产位置入口的无消费者版本；若 B 单独保留，原位置入口仍有实际消费者，不删除。DistanceSquared 保持。拒绝的融合实现、实验配置及额外 scratch 删除。

将 AdvancePreservesPerAttractorAndPerDimensionRandomDrawOrder 改为覆盖 B1 与 Bk 的样本布局参考：验证合格 attractor 的原顺序、`mD` 填充、D 切片映射、未消费尾部丢弃，以及每次 Repair 后下一次距离读取更新后的 target；不因允许样本预取而删除吸引顺序断言。全仓搜索 UpdateFireflyPosition、_randomWalk 与 FireflyMoveBenchmarks，确保 benchmark 历史参考不要求运行时保留旧 API。实现结束后更新架构摘要和随机轨迹边界。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 否 | 复用单位 Fill | Core 回归 |
| Algorithms | 是 | Firefly 与位置模板 | 内核/生成器测试 |
| Experiments | 实现否，验证是 | Group 私有 scratch 保持 | 拆分与并发测试 |
| Examples | 签名否 | 不承诺旧 seed 输出 | 编译 |
| Tests | 是 | 批次、逐次 Repair、融合 | Release tests |
| Benchmarks | 是 | 当前生产基线、移动计数 | 局部与完整 run |
| XML 文档 | 是 | 必要兼容说明 | DocFX |
| 用户/API 文档 | 是 | 现状与兼容摘要 | 文档验证 |
| ENGINEERING | 否 | 无规则变化 | 审查 |
| ADR | 否 | ADR-0025 覆盖 | 审查 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | 多 attractor、相等 Evaluation 不吸引、source 资格、Repair 改位置后下一距离、逐移动事件 | 内核与优化器 | 共同误差预算、完整事件表 |
| FR-002 | B1 的一移动一 Fill；Bk 的原顺序索引、`mD` Fill、D 切片、无合格不消费及取消尾部丢弃；零系数仍消费；复用/Group/取消 | 优化器与 Experiment | 后续样本和新版本重复 |
| NFR-001 | 同一移动输入 A/B/C、固定迭代完整 run、实际移动数 | BenchmarkDotNet | 相对现有 SIMD 增量；B1 新增 0 字节；Bk 总 scratch 为 `8KD + 4K` 字节，相对 B1 新增 `8D(K-1) + 4K` 字节（复用/扩展原 `_randomWalk`，删除被替代旧 scratch） |

差分包括 in-place、特殊值和长短尾部。数值遵循共同准则，距离本身不变；不因新采样轨迹而允许重新定义距离/吸引规则。完整 run 同时报告移动数，补充固定 attractor 事件内核来区分算术速度与轨迹工作量差异。

## 风险和回退

最大风险是把一次候选误当一次移动，从而提前计算距离/吸引力、使用 source.Position，或跳过中间 Repair。先以两个以上 attractor 和会修改 target 的 Repair 锁定事件，再实现 Bk；若 C 无增量收益仅保留通过门槛的 B，若 Bk 的 scratch 与 Fill 成本不划算则保留 B1；均不通过则保留 A 并删除候选。

## ADR 判断

不新增 ADR。Spec 已授权同一 source 候选内的跨移动随机样本预取；本 Plan 不授权提前计算距离/吸引力、改变吸引资格或顺序、提前或重排用户回调、公共 SIMD/ISA 或跨 source/代/run 的样本布局。若需要这些变化，退回 Spec/ADR。

## 批准记录

- 计划批准：—
- 批准日期：—
- Spec 边界授权引用：项目作者于 2026-09-15 批准 [spec.md](./spec.md) FR-001/FR-002 的同一 source 候选内显式跨移动随机样本预取；该记录仅引用边界授权，不代表本 Draft Plan 已批准。
