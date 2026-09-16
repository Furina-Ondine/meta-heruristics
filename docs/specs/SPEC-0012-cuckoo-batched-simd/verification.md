# SPEC-0012 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：—
- 最终结果：`Pending`
- 执行状态：尚未开始；本文件只预填批准范围、验证项目和验收门槛，当前没有实验或实现证据。
- A 基线源 hash：待 T001 锁定
- 验证候选源 hash：待实现阶段记录

## 需求覆盖

| 需求 | 计划实现位置 | 待执行测试或基准 | 待同步文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `CuckooOptimizer.cs` 的 StandardNormal 迁移与 `double[3D]` 采样工作区（待实施） | 正态角色切片、奇数 D/尾部、统计、固定 seed 重复、复用和私有 Gaussian/spare 残留检查（待执行） | Spec、Plan、必要 XML/开发者说明（待核对） | Pending |
| FR-002 | Lévy/遗弃公式、I-map、C1/C2/C3 算术候选（待实施） | 同样本逐元素差分、分支与特殊值分类、P=1/小 P 映射穷举、C3 数值域/回退及局部基准（待执行） | 若采用 C3，VectorOps 模板源注释、输入判断注释和 Developer Guide 数值说明（待执行） | Pending |
| FR-003 | 逐候选 Generate→Repair→Evaluate→替换→best 生命周期与 run 工作区复用（待实施） | 回调事件、动态 best、冻结 worst 目标索引、实时 population、两套随机状态、取消/异常、RunGroup 隔离（待执行） | 采样布局、轨迹变化和状态边界说明（待核对） | Pending |
| NFR-001 | 采样工作区、私有算术候选和完整 run（待实施） | A/B/C 局部与完整 BenchmarkDotNet、分配、JIT/硬件路径和 Release 工程验证（待执行） | Plan 门槛、环境、命令、源 hash、结果和限制（待执行） | Pending |

## 预定验证范围

实现阶段必须依据已批准 Plan 记录以下项目；这里的“预定”不代表已执行或通过：

- 单候选采样工作区为一块 `double[3D]`，每次 Lévy 候选执行一次 `StandardNormal.Fill(2D)` 和一次单位 `Fill(D)`；不拆分 `2D` 正态，不跨候选缓存样本，不建立索引缓冲，不恢复 `Fill(Span<int>, int, int)`。
- Cuckoo 的索引方案单独对照：P>1 使用现有 `NextInt` 的 `first`/`P-1` 映射，P=1 不消费索引，遗弃数量为 0 不消费遗弃样本；每次生成仍读取实时 population/best。
- 逐候选生命周期、Repair/Evaluate 顺序、评估计数、动态 best 和 FindWorstIndices 后的冻结目标索引必须与 Plan 一致；工作区跨 run 复用但不清零，由测试验证每次读取前本次有效区间已完整写入。
- C1 直接 TensorPrimitives 适用性先审查。C2 保留 `Math.Pow`；C3 仅评估 Lévy 分母的受限 `Exp(p * Log(x))`，记录快路径、Math.Pow 回退、标量尾部和特殊值分类。C3 若未通过门槛，不得保留为生产能力。
- 正确性维度为 `1、2、7、8、15、16、24、25、31、32、33、64、65、127、128、129、1024`，另含空 Span、非对齐切片、哨兵、in-place、P=1/2、小 P、遗弃率 0/1、指数边界和取消/异常场景。

## 性能报告（仅性能类修改）

性能类修改必须使用同一基线和同一输入记录局部与端到端证据。当前尚未运行任何基准，结果保留为 `Pending`。

| 层级 | 被测场景 | 基线与修改后结果 | 加速比 | 分配 | 结论 |
| --- | --- | --- | --- | --- | --- |
| 改动部分 | A/B 单候选正态与单位采样、Lévy/遗弃内核、I-map 独立对照 | 待 T008 记录 | Pending | 待记录 | Pending |
| 算术增量 | B/C C1/C2/C3，同一样本与同一索引布局 | 待 T005/T006/T008 记录 | Pending | 待记录 | Pending |
| 代表性整体任务 | Sphere + Clamp(-5,5)，P=64，seed `20260905`，MaxIterations=10/100；首次 run 与复用 workspace run | 待 T008 记录 | Pending | 待记录 | Pending |

历史与组合证据：H/A、A/B、B/C、A/C、H/C 均待测，历史源提交、依赖适配、统一 CopyTo 修订及产物路径待填。工作区预期一份 3D double 数组、载荷 24D 字节；实际首次/复用分配与稳态分配待测。

### 待记录的固定条件

- 主要维度 D=32、128；诊断补充 D=24、25、64、65 及完整计划维度矩阵。
- 默认指数 1.5、遗弃率 0.25、P=64、LevyCandidateCount=2；补充指数 0.5、1、1.99、合法范围端点附近配置、遗弃率 0/1 和 P=1/2。
- B 相对 A 的含采样局部门槛为至少 `1.10×`，完整 run 至少 `1.02×`；C 相对 B 的算术局部门槛为至少 `1.10×`，完整 run 至少 `1.02×`；最终保留版本相对 A 的完整 run 至少 `1.02×`。诊断回退不得超过确认的 5%。所有结果以 BenchmarkDotNet Mean 的 `基线耗时 / 修改后耗时` 表示，并记录置信区间。
- 默认 Release、同一 SDK/runtime、BenchmarkDotNet 2 launches、8 warmups、20 measured iterations、MemoryDiagnoser；记录 `Vector<double>.Count`、硬件支持、实际 case 数、完整 PowerShell 命令、源 hash 和工作区载荷。
- B/C 算术对照使用同一组正态、单位样本和索引输入；计入完整 scratch、Fill/缓冲读写、范围判断、回退、尾部和候选消费成本，不能把随机布局或 I-map 收益归入算术 SIMD。

## 数值、抽样与回调验证

| 验证项目 | 预定检查 | 证据 | 结果 |
| --- | --- | --- | --- |
| 正态角色与尾部 | 前 2D 切片分别作为 numerator/denominator，奇数 D、非满向量尾部只读取本次写入值 | 待 T002/T007 测试记录 | Pending |
| 分布与状态 | 固定 seed 0、1、20260905；每角色 2^18 样本，均值、方差、相关系数按 Plan 门槛；单值/批量状态分别观察后续值 | 待测试记录 | Pending |
| Lévy 公式 | Abs 后加 epsilon、两次分子乘法、乘后除、0.8/0.2 加法顺序及结果分类 | 独立标量参考差分（待执行） | Pending |
| C3 受限数值域 | x 接近 epsilon/1/极大值，p 很大/Infinity，零/次正规/有限/Infinity/NaN/负值，混合快路径与回退 lane；有限结果 `1e-14` 绝对加 `1e-12` 相对 | C3 数值矩阵和回退谓词（待执行） | Pending |
| 索引对 | 小 P 穷举 I-map，验证无相等索引、双射、P=1 不消费；与原相等重抽独立比较 | 索引映射测试/诊断基准（待执行） | Pending |
| 实时状态 | 后续 Lévy 候选看到前一候选替换后的 best/population；遗弃目标索引冻结但输入值实时读取 | 回调事件 fixture（待执行） | Pending |
| 生命周期隔离 | Repair/Evaluate 次序、计数、取消、异常、连续 run、并发实例与 RunGroup 拆分/调度 | 优化器与 Experiment 测试（待执行） | Pending |

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| Cuckoo 私有 `NextGaussian`、`_hasSpareGaussian`、`_spareGaussian` 与 spare 重置 | 删除，标准正态唯一由 Core `StandardNormal` 提供 | 待 T002/T009 搜索 | Pending |
| `Fill(Span<int>, int, int)` 及算法层整数 SIMD | 不恢复，不新增生产依赖 | 待 T004/T009 搜索 | Pending |
| 跨候选索引预取、索引数组或下一候选输入快照 | 不建立；索引按候选局部生成，population/best 实时读取 | 待 T003/T004/T009 搜索 | Pending |
| C3 通用 Pow/helper/公开入口/运行时开关 | 不新增；仅允许受限 Lévy 分母候选 | 待 T006/T009 搜索 | Pending |

## 架构一致性

- 策略职责是否保持独立：待实现与测试；Initializer/Repair/Evaluate 的职责和调用时点不得迁入算法批处理。
- 是否新增重复验证：待审查；Core 继续负责 StandardNormal/NextInt，Cuckoo 只负责算法公式和布局。
- 是否存在无消费者抽象：待审查；不保留失败的 C3、整数 Fill 或索引预取抽象。
- 职责是否位于批准的项目层：待审查；随机分布在 Core，Lévy/遗弃与私有 SIMD 在 Algorithms。
- 是否出现未经批准的兼容层：待审查；不增加公共 SIMD、通用 Pow 或随机映射兼容壳。

## 工程验证

- Restore：Pending
- Release Build：Pending
- Tests：Pending
- 生成器测试：Pending
- 文档验证器自测：Pending
- Format：Pending
- 文档链接与规格检查：Pending
- DocFX：Pending
- Benchmark 或分配分析：Pending

## 未解决问题

C1 直接 TensorPrimitives 的适用性、C2/C3 的局部性能以及 C3 可验证的快路径范围均待执行阶段决定；在证据产生前不把任一候选标为采用。尚无实现、实验或性能通过证据，Spec 不能据此进入 `Implemented`。
