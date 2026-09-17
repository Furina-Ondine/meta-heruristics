# SPEC-0014 技术计划：Firefly 批量随机游走与缩放融合

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`ce43725516d70d70edbe000ffc713e073adb0b94`
- 批准时 Spec 内容 SHA-256（UTF-8、LF）：`a0efe5f16bd0d8e8c2d70cdfcaff142d7c4156ecf5132c5943a5a47d6104c502`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 创建日期：2026-09-14
- 修订日期：2026-09-15
- 批准人：项目作者
- 批准日期：2026-09-15

[共同验证计划](../simd-plan.md)是本 Plan 的组成部分，规定基线 A/B/C、数值通则、性能门槛与测量记录要求；本文件定义算法特有的设计。项目作者于 2026-09-15 批准本 Plan 及共同验证附件。Spec 基线提交记录已提交版本，本次批准还包含此前逐项确认的未提交 Spec 修订，以上内容摘要锁定批准时的完整 Spec；旧提交不能单独代表本次批准范围。

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
| B：每次实际移动 Fill(D)+原缩放+原位置内核 | 隔离批量收益 | 仍有缩放遍历 | 批量状态推进改变序列 | 唯一采样候选 |
| C1：直接 TensorPrimitives | 使用现成适用操作 | 操作覆盖范围有限 | 必须保持 in-place 语义 | 首个候选 |
| C2：单位随机缩放并入位置级联 | 少一次 scratch 读写 | 扩展位置内核参数 | 缩放次序/alias | 无适用直接操作或未过门槛时评估 |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 随机状态与单位分布 | Core | Core | 使用现有 Fill |
| 每次实际移动的采样与缩放系数 | Firefly | Firefly | 算法行为 |
| 距离与位置算术 | VectorOps | 通过门槛的直接 TensorPrimitives 或私有融合内核 | 现有私有职责 |
| 吸引资格、顺序和 Repair | FireflyOptimizer | FireflyOptimizer | 逐移动时序不能跨越 |

## 采样布局与算术设计

GenerateCandidate 直接沿 sourcePopulation 的原 attractor 顺序遍历。对每个 attractor，先按现有 EvaluationComparer 与 source.Evaluation 判断资格；不合格时不采样、不计算、不移动、不 Repair。合格时立即调用 `RandomSource.Fill` 填满已有 `_randomWalk[D]`，该次 `Fill(D)` 消费 `ceil(D/L)` 轮批量状态且不消费标量随机状态；随后只读取本次写入的 D 个样本，计算距离和 attractiveness、更新位置并立即 Repair，再继续下一个 attractor。不预先收集索引、不为后续 attractor 预取样本。

已有 `_randomWalk[D]` 是 B 的全部随机 scratch，跨顺序 run 复用，不清零或额外重置；每个有效移动都先写满 D 个样本再读取，读取范围只限本次写入的 D 个元素。没有合格 attractor 时不调用 Fill、不移动、不 Repair；取消或异常时沿用既有取消检查、异常传播和 Optimizer 生命周期规则，不新增清理或跨阶段状态处理。

B 在 scratch 上保留逐维缩放 `randomStep * (u-0.5)`，随后调用原距离和原位置内核。C 保留 scratch 为单位样本，扩展私有位置内核的 randomStep 参数，按原括号计算：

```text
walk = randomStep * (unitRandom - 0.5)
movement = (attractiveness * (attractorPosition - currentPosition)) + walk
destination = currentPosition + movement
```

B 沿用原有 `_randomWalk[D]`，新增持久载荷为 0。目标为精确 in-place，当前移动所需 current 必须加载后再存回；不支持任意部分重叠。距离仍用既有 DistanceSquared，读取上次 Repair 后 target.Position；attractiveness 的 Math.Exp 仍由 Optimizer 在本次移动中计算，不能预先对后续 attractor 计算距离或吸引力。

即使 randomStep=0 或 attractiveness=0，只要 attractor 合格，B 仍为该移动分配 D 个样本并执行原移动公式和 Repair；通过该移动自己的 `Fill(D)` 写满后读取。不引入“系数为零便跳过”的随机语义优化。RandomStep 的代际衰减保持现状。Initializer 与全体 Evaluate 阶段不改动。

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Options | 原构造验证 | 一次 | 否 | 原异常与零系数合法性 |
| scratch/目标长度与别名 | 原工作区及内核测试 | 首次/测试 | 不逐块验证 | `_randomWalk[D]` 跨 run 复用；每次只读本次 Fill(D) 写入的 D 个样本 |
| 同样本缩放/位置 | 标量参考与现有生产组合 | 诊断矩阵 | 仅测试 | 浮点分类及括号 |
| 多 attractor 与策略状态 | 修改位置的记录 Repair | 每测试 run | 仅测试 | 吸引者顺序、下一距离读修复后位置 |

## API 与行为变化

新增只限通过门槛的私有位置内核参数/入口；B 复用已有 `_randomWalk[D]`，修改每个实际移动的随机样本采样路径。公共签名和用户组装保持；旧 seed 轨迹允许变化。吸引资格、吸引者顺序、逐移动距离/吸引力计算、移动和 Repair 顺序、Evaluation 阶段、Group/取消契约不变。

## 替代与清理计划

融合通过后删除旧生产位置入口的无消费者版本；若 B 单独保留，原位置入口仍有实际消费者，不删除。DistanceSquared 保持。拒绝的融合实现、实验配置及额外 scratch 删除。

将 AdvancePreservesPerAttractorAndPerDimensionRandomDrawOrder 改为覆盖 B 的样本布局参考：验证合格 attractor 的原顺序、每次 `Fill(D)` 消费 `ceil(D/L)` 轮批量状态且不消费标量状态、D 个样本先写后读，以及每次 Repair 后下一次距离读取更新后的 target；验证 `_randomWalk[D]` 跨顺序 run 复用且不清零或额外重置。全仓搜索 UpdateFireflyPosition、_randomWalk 与 FireflyMoveBenchmarks，确保 benchmark 历史参考不要求运行时保留旧 API。实现结束后更新架构摘要和随机轨迹边界。

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
| FR-002 | B 每个实际移动一次 Fill(D)；原 attractor 顺序、D 样本先写后读、无合格不消费；零系数仍消费；缓冲跨 run 复用、Group/取消 | 优化器与 Experiment | 后续样本和新版本重复 |
| NFR-001 | 同一移动输入 A/B/C、固定迭代完整 run、实际移动数 | BenchmarkDotNet | 相对现有 SIMD 增量；B 新增持久载荷 0 字节，复用已有 `_randomWalk[D]` |

差分包括 in-place、特殊值和长短维度。数值遵循共同准则，距离本身不变；不因新采样轨迹而允许重新定义距离/吸引规则。完整 run 同时报告移动数，补充固定 attractor 事件内核来区分算术速度与轨迹工作量差异。

## 风险和回退

最大风险是把一次候选误当一次移动，从而提前计算距离/吸引力、使用 source.Position，或跳过中间 Repair。先以两个以上 attractor 和会修改 target 的 Repair 锁定事件，再实现 B；若 C 无增量收益仅保留通过门槛的 B，若 B 的 Fill(D) 与缩放成本不划算则保留 A 并删除候选。

## ADR 判断

不新增 ADR。Spec 已授权同一 source 候选内的跨移动随机样本预取，但本 Plan 当前选择每个实际移动一次 `Fill(D)`，不采用跨移动分块样本布局；本 Plan 也不授权提前计算距离/吸引力、改变吸引资格或顺序、提前或重排用户回调、公共 SIMD/ISA 或跨 source/代/run 的样本布局。若需要这些变化，退回 Spec/ADR。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-09-15
- Spec 边界授权引用：项目作者于 2026-09-15 批准 [spec.md](./spec.md) FR-001/FR-002 的同一 source 候选内显式跨移动随机样本预取；该记录仅引用边界授权，该次未批准整份 Plan。
- 当前布局选择记录：项目作者于 2026-09-15 同意本 Plan 的 B 采样限定为每个实际移动一次 `Fill(D)`，复用 `_randomWalk[D]`，跨顺序 run 复用且不清零或额外重置；该次仅确认布局方向，未启动实现或实验。Spec 的更宽跨移动预取授权保留，但不在当前布局中采用。
- 验收标准确认（2026-09-15）：项目作者确认共同验证计划的性能门槛、验证负载及本 Plan 的数值预算；该次确认仅针对验收标准。
- 整体批准（2026-09-15）：项目作者通过“批准plan”批准本 Plan 和共同验证附件；候选采用仍须通过既定数值与性能门槛。该次仅记录批准，尚未创建 Tasks 或启动实现/实验。
- 任务拆分（2026-09-16）：按项目作者授权建立 [Tasks](./tasks.md) 和 [Verification 模板](./verification.md)，任务及验证结果均为 Pending；尚未启动实现或实验。
- 执行补充（2026-09-17）：项目作者启动连续实施；受限 Vector128 路径只要求不弱于同公式标量参考，具体边界见共同附件，其他数值及性能门槛不变。
