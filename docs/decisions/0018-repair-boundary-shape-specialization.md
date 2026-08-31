# ADR-0018: Repair 边界形状专用化

## 状态

Superseded by [ADR-0019](0019-scalar-reflect-and-algorithm-only-simd-generation.md).

替代 [ADR-0013](0013-tensor-shaped-repair-bounds.md) 中内置 Repair 支持四种独立端点形状的决定。

## 背景

Clamp、Reflect 与 RandomReset 的混合标量/向量端点没有已确认的实际消费者，却要求私有边界模型和每次 Repair/SIMD 块在广播与数组加载之间分派。该通用模型扩大了公共 API 和热路径维护面。

## 决策

- 三种内置 Repair 只公开 `double lower, double upper` 与 `ReadOnlySpan<double> lower, ReadOnlySpan<double> upper` 工厂；不保留混合端点、自动广播、Obsolete 重载或兼容层。
- Factory 在创建时选择 scalar/scalar 或 vector/vector 的密封私有 Repair 类型。标量类型只保存标量端点与标量派生值；向量类型只保存防御性复制的等长边界数组及派生数组。
- 删除通用 `Boundary` 判别模型和每次 Repair 的形状判断。向量类型继续在执行时检查 Position 长度；创建时端点 NaN、顺序、等长及复制语义保持不变。
- Reflect 的 scalar 与 vector SIMD 模板各自只使用广播或数组加载，继续采用已批准的 512→256→128→scalar 回退、特殊 lane 与危险大 offset 标量修补。
- Repair 继续拥有边界，不向算法公开边界；`ICandidateRepair`、随机消费规则、数值规则和调用时机不变。

## 替代方案

- 仅删除 public 混合重载但保留通用 `Boundary`：不采用，因为会留下无消费者的形状分派。
- 每个 Repair 保留一个以 bool 分派的类型：不采用，因为 Reflect 仍必须在热路径选择广播或加载。
- 建立公开泛型边界或运行时后端：不采用，因为会增加 API 和运行时抽象，超出当前需求。

## 后果

这是破坏性 API 变更：混合端点调用方必须按其领域语义显式构造同形状边界。Core 的内置 Repair 实现和 SIMD 模板数量增加，但每个保留形状只有一条端点访问路径。性能接受与数值、分配、确定性验证仍受 ENGINEERING.md 和 SPEC-0007 约束。

## 重新评估条件

若出现已确认的混合边界消费者、需要新的数值类型或候选表示、设备端执行，或实测数据证明另一种边界存储更合适时，新增 ADR 重新评估。
