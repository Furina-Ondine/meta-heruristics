# SPEC-0013 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 共同验证附件：[`simd-plan.md`](../simd-plan.md)
- 模板创建日期：2026-09-15
- 验证日期：—
- 最终结果：`Pending`

仅为后续填写的报告模板；下列位置与场景是待核验项，不是已完成的实现、测试或性能证据。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | 待填：PsoOptimizer / 选定算术路径 | 待填：同输入双输出差分、Clamp 分类、回调与 best 更新事件（T001/T004/T005/T007） | 待填：实际路径与必要数值说明 | Pending |
| FR-002 | 待填：初始化 Fill、整代 2P 系数候选及工作区 | 待填：角色布局、后续随机状态、复用/Group/取消/异常（T002/T003/T005/T007） | 待填：实际采样布局与旧轨迹说明 | Pending |
| NFR-001 | 待填：最终保留候选及删除项 | 待填：B-init、B-coeff、组合、C 的增量与完整 run（T001/T006/T007） | 本报告性能与分配证据待填 | Pending |

## 数值、采样与生命周期证据

| 检查项 | 待验证内容 | 证据与结果 |
| --- | --- | --- |
| 算术 | 原括号次序、Velocity/Position 双输出、Clamp 端点、NaN/Infinity/零符号，误差按 Plan | Pending |
| 初始化 | 非相等范围 Fill、相等范围不抽样、Initializer→Repair→初始化→Evaluate | Pending |
| 整代系数 | 一次 Fill(2P)、交错角色、每粒子一对共享、零系数仍消费、ceil(2P/L) 轮 | Pending |
| 回调与状态 | 单值/批量策略后续状态、Repair 与 Evaluate 顺序、personal/global best 更新 | Pending |
| 工作区与生命周期 | 先写后读、顺序 run 复用、Group/并发、取消/异常传播与计数 | Pending |
| 路径矩阵 | Plan 维度/种群矩阵、尾部/切片/哨兵、实际硬件与软件路径、JIT | Pending |

## 性能报告（仅性能类修改）

门槛、数值预算和固定负载以共同附件及 Plan 为准，不根据结果修改。加速比按 `基线耗时 / 修改后耗时`；不从旧纯标量 PSO 推断本轮增量。

| 层级 | 被测场景 | 基线与修改后结果 | 加速比 | 结论 |
| --- | --- | --- | --- | --- |
| 改动部分 | B-init ResetForRun / A | 待测 | 待测 | Pending |
| 改动部分 | B-coeff 整代采样、读写与候选消费 / A；每代及每粒子成本 | 待测 | 待测 | Pending |
| 改动部分 | 两项 B 组合 / A；算术 C / 同布局 B | 待测 | 待测 | Pending |
| 代表性整体任务 | D=32/128、10/100 代；首次/复用完整 Runner；B-init、B-coeff、组合及 C | 待测 | 待测 | Pending |
| 诊断负载 | 共同附件的短维度、24/25/64/65、尾部、零系数与相等速度范围 | 待测 | 待测 | Pending |
| 历史与组合 | H/A、A/B、B/C、A/C、H/C，同机同 harness | 待测 | 待测 | Pending |

- 基线 H/A 源提交、依赖、最小适配与统一 CopyTo 修订：待填。
- 候选源提交或补丁 hash、基准参考来源：待填。
- OS/CPU/SDK/Runtime、Core L、Algorithms 128/256/512 支持及实际限制开关：待填。
- 完整命令、filter、预计/实际 case 数、launch/warmup/iteration、产物路径：待填。
- 逐点 Mean、误差/置信区间、迭代/评估/Repair 数：待填。
- 分配：B-init 新增载荷预期 0，B-coeff 预期一个 2P double 数组、16P 字节载荷；实际首次/复用分配、数组数量与稳态调用级分配待测。
- 候选保留、删除、不确定及原因：待填；不将未测或失败候选写为成功。
- 完整 run 是固定工作量耗时；达到精度的求解时间尚无证据，不作收敛速度结论。

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| ComputePsoVelocity 旧 runtime 入口 | 仅成功融合且无生产消费者时删除；历史参考留基准工程 | 待检索 | Pending |
| 私有 NextDouble helper | 删除失去消费者的 helper，保留仍需使用的路径 | 待检索 | Pending |
| 失败候选内核与 2P 缓冲 | 未采用且无消费者时删除 | 待检索 | Pending |
| 旧布局断言/重复测试 | 更新新布局；保留有独立参考价值的测试 | 待检索 | Pending |

## 架构一致性

- 策略职责是否保持独立：待验证 Core 分布与 Algorithms 角色/工作区边界。
- 是否新增重复验证：待检查构造验证、工作区隔离与热路径。
- 是否存在无消费者抽象：待完成全仓残留检索。
- 职责是否位于批准的项目层：待核验项目依赖及生成器作用范围。
- 是否出现未经批准的兼容层：待核验公共 API、运行时开关及旧入口。

## 工程验证

- Restore：Pending
- Release Build：Pending
- Tests：Pending
- 生成器测试：Pending
- Format：Pending
- 文档链接与规格检查：Pending
- 文档验证器自测：Pending
- DocFX：Pending
- Benchmark 或分配分析：Pending

## 未解决问题

- T001 至 T007 均尚未执行，当前不能判定候选收益或将 Spec 标为 Implemented。
- 运行后逐项补充不支持的硬件路径、失败/不确定结果及必要上游修订；不以模板代替证据。
