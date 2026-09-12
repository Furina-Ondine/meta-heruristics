# SIMD 规格联合审查

本文只聚合已批准规格和审批边界；需求以各 Spec 为准，不是技术 Plan。

## 已确认范围

2026-09-06，项目作者确认：四种算法均进一步优化；允许批量采样导致改造前后固定 seed 轨迹变化；随机源要求原始状态推进本身 SIMD 化，允许调整单流状态模型。

## 已批准的完整规格

| Spec | 具体方案与主要风险 |
| --- | --- |
| [SPEC-0010](SPEC-0010-simd-random-sampling/spec.md) | 保留 xoshiro256++；SplitMix64 展开单值初态，内部 Jump 为每个批量 lane 派生起点；单值与批量状态相互隔离，批量 lane 数取运行时的 `Vector<T>.Count`；增加播种与状态体积成本。 |
| [SPEC-0011](SPEC-0011-bat-batched-simd/spec.md) | 候选内批量采样及 SIMD；允许给未选中的扰动分支预生成随机样本；保持相等边界不消费对应样本。 |
| [SPEC-0012](SPEC-0012-cuckoo-batched-simd/spec.md) | 用 StandardNormal.Fill 替代私有正态及 spare；批量 Lévy 与遗弃计算；保留逐候选替换和 best 更新顺序。 |
| [SPEC-0013](SPEC-0013-pso-simd-refinement/spec.md) | 评估速度、限幅、位置融合及初始化批量化；每个粒子仍共享一对 cognitive/social 系数。 |
| [SPEC-0014](SPEC-0014-firefly-simd-refinement/spec.md) | 批量随机游走及缩放融合；维持吸引者顺序、基于移动后位置的距离和每次移动后的 Repair。 |

## 架构决策变更

- 随机源方案要求替代 ADR-0020 的单组四字状态限制；保留封闭公共能力、Core 所有权、ulong seed 和 Group 生命周期。
- PSO 融合要求替代 ADR-0016 对 Clamp/Add 必须使用 TensorPrimitives 的限制；保留 TensorPrimitives 优先审查、Algorithms 固定宽度级联和基准准入规则。
- ADR-0019 的 Reflect 标量和生成器仅服务 Algorithms 决策继续有效；Core 随机 SIMD 使用私有通用 intrinsic，不新增生成器引用。
- 替代决策已记录为 ADR-0021、ADR-0022；旧 ADR 保留原文并标记 Superseded。

## 后续交付顺序

随机源的详细 Plan 见 [SPEC-0010 技术计划](SPEC-0010-simd-random-sampling/plan.md)；SPEC-0011 至 SPEC-0014 的 Plan 尚未创建，本轮只实施 SPEC-0010，不修改算法实现。

验证需分别隔离随机源收益、算法批量化收益、算术 SIMD 收益和最终组合收益；基线为本次改造前代码，PSO/Firefly 基线包含已有 SIMD。完整 run 计入随机源创建、ResetForRun 和固定工作量执行。保留路径须有内核与端到端数据；每轮 BenchmarkDotNet 前展示待测代码和完整命令并取得反馈。

## 审批记录

- 规格批准：项目作者，2026-09-06，通过“同意”批准联合规格。
- 当前阶段：五份 Spec 为 Approved，五份 Plan 为 Draft。
- Spec 批准授权制定 Plan；实现和基准仍遵守 Plan 批准与测量门。
