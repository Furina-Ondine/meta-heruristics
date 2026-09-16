# SIMD 规格联合审查

本文只聚合已批准规格和审批边界；需求以各 Spec 为准，不是技术 Plan。

## 已确认范围

2026-09-06，项目作者确认：四种算法均进一步优化；允许批量采样导致改造前后固定 seed 轨迹变化；随机源要求原始状态推进本身 SIMD 化，允许调整单流状态模型。

## 已批准的完整规格

| Spec | 具体方案与主要风险 |
| --- | --- |
| [SPEC-0010](SPEC-0010-simd-random-sampling/spec.md) | 保留 xoshiro256++；SplitMix64 展开单值初态，内部 Jump 为每个批量 lane 派生起点；单值与批量状态相互隔离，批量 lane 数取运行时的 `Vector<T>.Count`；增加播种与状态体积成本。 |
| [SPEC-0011](SPEC-0011-bat-batched-simd/spec.md) | 候选内及显式跨候选样本预取；允许给未选中的扰动分支预生成随机样本；保持相等边界不消费对应样本和实际回调顺序。 |
| [SPEC-0012](SPEC-0012-cuckoo-batched-simd/spec.md) | 用 StandardNormal.Fill 替代私有正态及 spare；允许显式预取样本/索引对，使用现有 NextInt 的不同索引映射；保留逐候选实时读取、替换和 best 更新顺序。 |
| [SPEC-0013](SPEC-0013-pso-simd-refinement/spec.md) | 评估速度、限幅、位置融合、初始化批量化及显式布局的整代系数预取；每个粒子仍共享一对 cognitive/social 系数，实际回调与状态更新顺序保持。 |
| [SPEC-0014](SPEC-0014-firefly-simd-refinement/spec.md) | 批量随机游走及缩放融合，允许同一源候选内跨移动预取样本；维持吸引者顺序、基于前次 Repair 后位置的距离和每次移动后的 Repair。 |

## 架构决策变更

- 随机源方案要求替代 ADR-0020 的单组四字状态限制；保留封闭公共能力、Core 所有权、ulong seed 和 Group 生命周期。
- PSO 融合要求替代 ADR-0016 对 Clamp/Add 必须使用 TensorPrimitives 的限制；当前直接 TensorPrimitives 与私有融合内核的选择、以及基准执行规则见 ADR-0025。
- ADR-0019 的 Reflect 标量和生成器仅服务 Algorithms 决策继续有效；Core 随机 SIMD 使用私有通用 intrinsic，不新增生成器引用。
- 替代决策已记录为 ADR-0021、ADR-0022；旧 ADR 保留原文并标记 Superseded。

## 后续交付顺序

随机源已完成，详细证据见 [SPEC-0010 Verification](SPEC-0010-simd-random-sampling/verification.md)。2026-09-14 开始剩余算法规划，按既有编号顺序推进 Bat、Cuckoo、PSO、Firefly；每份 Plan 单独批准和验收，不以联合 Spec 批准代替实现批准。Cuckoo 本轮已明确使用现有 NextInt，不恢复公共整数 Fill；新增整数 SIMD 能力留待独立规格。

| Plan | 状态 | 主要审阅点 |
| --- | --- | --- |
| [Bat](SPEC-0011-bat-batched-simd/plan.md) | Approved | 单候选批量采样，比较标量分支与掩码 SIMD；分离采样/算术/完整 run 收益，scratch 为 3D 个 double |
| [Cuckoo](SPEC-0012-cuckoo-batched-simd/plan.md) | Approved | 单候选正态/单位采样，逐候选索引映射；Math.Pow 基线与受限输入域 Log→乘法→Exp 融合候选，采用后明确注释/文档边界；实时种群语义保持 |
| [PSO](SPEC-0013-pso-simd-refinement/plan.md) | Approved | 初始化 Fill 与整代 2P 系数预取分别评估、每粒子一对系数共享、速度/Clamp/位置融合 |
| [Firefly](SPEC-0014-firefly-simd-refinement/plan.md) | Approved | 每次实际移动一次 Fill(D)，复用已有缓冲；逐次 Repair 与随机缩放融合 |

[共同验证计划](simd-plan.md)是四份 Plan 的附件，固定直接生产基线、增量对照、数值通则与性能门槛；验收标准、四份完整 Plan 及本附件已于 2026-09-15 获项目作者批准。本轮 SIMD 工作交付规划文档、Pending Tasks 和 Verification 模板，尚未实施 SIMD 或继续运行基准；此前另行授权的数组复制改为 Span.CopyTo 独立提交，不作为 SIMD 实施或性能验收证据。

验证需分别隔离随机源收益、算法批量化收益、算术 SIMD 收益和最终组合收益；基线为本次改造前代码，PSO/Firefly 基线包含已有 SIMD。完整 run 计入随机源创建、ResetForRun 和固定工作量执行。保留路径须有内核与端到端数据；基准配置、完整命令、源 hash 和结果记录于 Verification。

## 执行文档

2026-09-16，四份任务清单与验证模板已齐备，任务和验证结果均为 Pending。

| 算法 | 任务清单 | 验证模板 |
| --- | --- | --- |
| Bat | [Tasks：8 项](SPEC-0011-bat-batched-simd/tasks.md) | [Verification](SPEC-0011-bat-batched-simd/verification.md) |
| Cuckoo | [Tasks：9 项](SPEC-0012-cuckoo-batched-simd/tasks.md) | [Verification](SPEC-0012-cuckoo-batched-simd/verification.md) |
| PSO | [Tasks：7 项](SPEC-0013-pso-simd-refinement/tasks.md) | [Verification](SPEC-0013-pso-simd-refinement/verification.md) |
| Firefly | [Tasks：5 项](SPEC-0014-firefly-simd-refinement/tasks.md) | [Verification](SPEC-0014-firefly-simd-refinement/verification.md) |

## 审批记录

- 规格批准：项目作者，2026-09-06，通过“同意”批准联合规格。
- 当前阶段：SPEC-0010 为 Implemented、其 Plan 为 Approved；SPEC-0011 至 SPEC-0014 为 Approved、对应 Plan 为 Approved。Cuckoo 整数范围已于 2026-09-15 明确。
- Spec 批准授权制定 Plan；实现和基准遵守明确批准范围，不设逐轮测量确认。
- 当前工作边界（2026-09-15）：完善规划、拆分 Tasks 并建立 Verification 模板，不继续实验；Bat 的分支 SIMD 收益仍需后续阶段判定。
- PSO 采样规则修订（2026-09-15）：项目作者批准以显式布局取代跨回调预取禁令；该次尚未批准具体布局、候选性能门槛及整份 Plan，不构成实验或实现授权。
- Bat/Cuckoo/Firefly 采样规则修订（2026-09-15）：项目作者批准相应显式样本预取边界，保留实际回调和依赖状态读取时机；批准 Bat 常量区间不采样及 Cuckoo 使用现有 NextInt 的不同索引对映射、单巢不采样索引。本次同意授权修订文档并提交、推送，该次尚未批准整份 Plan。
- 验收标准确认（2026-09-15）：项目作者确认局部 1.10×、主要完整 run 1.02×、诊断负载回退不超过 5%，以及既定数值预算、特殊值分类/回退与验证负载。主要维度为 32/128，补充 24/25/64/65 覆盖实际使用范围；具体对照和测试规则以共同附件及各 Plan 为准。
- Plan 整体批准（2026-09-15）：项目作者通过“批准plan”批准 SPEC-0011 至 SPEC-0014 的完整 Plan 及共同验证附件。下一阶段为 Tasks 拆分；本次仅落实审批，未启动实现或实验。
- Tasks 拆分授权（2026-09-15）：项目作者要求拆分四份 Tasks 并补入 Verification 模板；任务全部为 Pending，报告结果全部为 Pending，未启动实现或实验。
