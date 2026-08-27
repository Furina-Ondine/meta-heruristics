# ADR-0015: 评估结果使用有序扩展数值域

## 状态

Accepted

## 背景

Core 当前用“必须有限”同时约束目标值、约束违背量和目标停止阈值。这把不能可靠排序的 `NaN` 与具有明确顺序的正负 Infinity 混为一类。放宽单个构造器又会把 Infinity 传播到比较、约束汇总和 Experiment 统计，其中直接 IEEE 算术可能产生 `NaN`。

项目要求只验证 Core 协议真正需要的不变量，并避免在热路径扫描或修复调用方拥有的候选位置。需要为完整评估消费链定义统一且可测试的特殊值语义。

## 决策

- 目标值使用有序扩展实数域：有限值与正负 Infinity 有效，`NaN` 无效。最小化顺序为 `-Infinity < finite < +Infinity`，最大化反转该顺序。
- 约束违背量使用非负扩展实数域：零、正有限值和 `+Infinity` 有效；负有限值、`-Infinity` 和 `NaN` 无效。`+Infinity` 表示无界不可行程度，有限正值求和溢出也聚合为 `+Infinity`。
- Objective 与 Constraint 的策略返回值在 `ContinuousProblem` 评估边界验证；直接构造的公开值对象在各自构造器验证。算法、比较器和 Experiment 调度器不重复验证。
- `TargetObjective` 使用与目标值相同的阈值域，只拒绝 `NaN`，并按优化方向直接比较正负 Infinity。
- Experiment 的 `NumericStatistics.Minimum` 与 `Maximum` 保持非空扩展实数。Mean、Median 和 StandardDeviation 使用可空 `double`：只有数学结果在批准域中无定义时返回 `null`，绝不以 `NaN` 表示。
- 跨 run 统计先显式分类 Infinity；有限统计使用避免原始中间求和溢出的缩放计算。不得先产生 `NaN` 再统一转换成 `null`。
- 这是刻意的破坏性 API 变化。不为原有非空统计属性保留兼容属性、包装类型或配置开关。
- 候选位置及 Repair 的 `NaN`/Infinity 语义不受本决策影响，继续由 ADR-0013 管理。

具体组合、nullable 条件和验收标准由 [SPEC-0001](../specs/SPEC-0001-evaluation-special-values/spec.md) 定义。

## 替代方案

- 拒绝所有非有限结果：简单，但拒绝可以确定排序的 Infinity，并扩大不必要的防御性验证。
- 保留直接 IEEE 统计并把最终 `NaN` 改成 `null`：改动少，但会掩盖有限样本中间溢出等实现错误。
- 只统计有限样本并另报 Infinity 数量：结果不再描述完整样本，且需要未经需求证明的新公共表示。

## 后果

Core 的每次 Objective/Constraint 结果仍只增加常数次标量分类，不恢复候选维度扫描或托管分配。Infinity 可以贯穿比较、停止、轨迹和结果汇总。Experiment 统计实现更长，三个公共属性变为 nullable，调用方必须显式显示或处理未定义结果。

现行算法配置、Repair 有限边界、容差和其他独立参数仍可要求有限值；残留搜索不能机械删除这些真实不变量。

## 重新评估条件

当目标值泛化为非 `double` 类型、引入多目标统计、需要区分更多未定义原因，或实际消费者证明 nullable 统计不足以表达需求时，通过新 Spec 和 ADR 重新评估。
