# SPEC-0011 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 共同验证附件：[`simd-plan.md`](../simd-plan.md)
- 模板创建日期：2026-09-16
- 验证日期：—
- 最终结果：`Pending`

本文件只列出待验证项目。实现、测试和基准尚未开始，预期行为和位置不代表已完成证据。

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | 待核验：BatOptimizer、选定算术路径 | 同样本六字段差分、分支阈值、限幅、特殊值与源状态保护（T001/T003/T004/T005/T006） | 待填：实际路径与数值说明 | Pending |
| FR-002 | 待核验：初始化与单候选采样调用 | 有界/常量填充、角色切片、尾轮与后续随机状态（T001/T002/T003） | 待填：采样布局与轨迹说明 | Pending |
| FR-003 | 待核验：工作区、Advance、Repair/Evaluate/选择顺序 | 先写后读、顺序 run、Group/并发、取消与异常（T002/T003） | 待填：工作区与生命周期说明 | Pending |
| NFR-001 | 待核验：最终保留候选 | 初始化、B 采样、Exp 外提、C 算术、完整 Runner 与分配/JIT（T004/T005/T006/T007/T008） | 本报告数据及限制待填 | Pending |

## 数值、抽样与生命周期证据

| 检查项 | 待验证内容 | 证据与结果 |
| --- | --- | --- |
| 随机布局 | 频率直接写目标；pulse/perturbation/accept 使用 3D scratch；依次四次 Fill，频率常量时三次；相等初始化端点不抽样 | Pending |
| 状态推进 | 实际 L 下 4*ceil(D/L) 或 3*ceil(D/L)；策略分别使用单值/批量状态，核对后续样本 | Pending |
| 六字段 | Position、Velocity、Frequency、Loudness、PulseRate、InitialPulseRate；接受/拒绝字段和源状态保护 | Pending |
| 分支 | pulse 严格大于、accept 严格小于；等于阈值、全选/全拒/混合，额外计算不得改变分类或选择 | Pending |
| 算术 | 共同有限值预算、NaN/Infinity/零符号、限幅端点、原求值顺序；Exp 外提独立差分 | Pending |
| 生命周期 | 全体生成并逐候选 Repair→全体 Evaluate→选择；固定 seed、先写后读/复用、Group/并发、取消/异常 | Pending |
| 执行路径 | 共同维度集合、尾部、允许的别名/切片/哨兵、实际 128/256/512 及软件路径和 JIT | Pending |

## 性能报告（仅性能类修改）

按 Plan 和共同附件逐点判定，不根据结果降低门槛。加速比为 `基线耗时 / 修改后耗时`；同输入 B/C 算术差分与 A/B 随机布局变化分别处理。

| 层级 | 被测场景 | 基线与修改后结果 | 加速比 | 结论 |
| --- | --- | --- | --- | --- |
| 改动部分 | 初始化 ResetForRun / 原初始化 | 待测 | 待测 | Pending |
| 改动部分 | B 单候选采样与消费 / A 原采样与分支 | 待测 | 待测 | Pending |
| 独立贡献 | Math.Exp 因子外提；局部及完整 run | 待测 | 待测 | Pending |
| 算术增量 | C1/C2 / 同布局 B；全接受/全拒绝/混合，含额外分支计算、写回及尾部 | 待测 | 待测 | Pending |
| 代表性整体任务 | D=32/128，Sphere、Clamp(-5,5)、P=64、seed 20260905、10/100 代；首次/复用完整 Runner | 待测 | 待测 | Pending |
| 诊断负载 | 共同短维度/尾部，24/25/64/65，相等频率和零宽参数 | 待测 | 待测 | Pending |
| 历史与组合 | H/A、A/B、B/C、A/C、H/C，同机同配置 | 待测 | 待测 | Pending |

- H/A 源提交、依赖与最小适配、统一 CopyTo 修订：待填。
- 候选源 hash、OS/CPU/SDK/Runtime、Core L、实际硬件支持与进程开关：待填。
- 完整命令、filter、预计/实际 case 数、2 launches/8 warmups/20 measured iterations 的实际配置与产物路径：待填。
- 逐点 Mean、误差/置信区间、迭代/评估/Repair 计数：待填。
- 分配：预期一份 3D double scratch、载荷 24D 字节；实际首次/复用分配、数组数量及稳态调用级分配待测。
- 候选取舍与限制：待填；B 单独无收益时的组合评估遵循共同附件，不能借组合收益声称 B 单独达标。
- 固定迭代完整 run 不代表达到目标精度的求解时间；当前不作收敛时间结论。

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| 旧随机循环及私有 NextDouble helper | 只删除采用批量路径后失去消费者的实现；保留有效回退 | 待检索 | Pending |
| 失败 C1/C2、Exp 外提候选及无消费者缓冲 | 按取舍结果清理；不保留生产切换开关 | 待检索 | Pending |
| 重复算术入口或历史基线 | 历史对照留基准专用参考，不为其保留无生产消费者 runtime 入口 | 待检索 | Pending |

## 架构一致性

- 策略职责是否保持独立：待核验 Core 随机分布和 Bat 角色/公式的边界。
- 是否新增重复验证：待核验构造验证与热路径。
- 是否存在无消费者抽象：待完成残留检索。
- 职责是否位于批准的项目层：待核验 Algorithms 私有算术与生成器边界。
- 是否出现未经批准的兼容层：待核验公共 API、运行时开关和工作区所有权。

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

- T001 至 T008 尚未执行；分支额外计算的收益、Exp 外提与 C1/C2 的取舍均待证据。
- 当前不能将任何候选标为优化成功，或据此把 Spec 标为 Implemented。
