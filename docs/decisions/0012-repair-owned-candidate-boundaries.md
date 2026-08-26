# ADR-0012: Repair 拥有候选位置边界

## 状态

Accepted

替代 [ADR-0005](0005-candidate-objective-and-constraints.md) 中关于候选边界与可选修复的规定。

## 背景

此前 `ContinuousProblem` 公开边界，并在评估、运行器和算法路径上重复验证候选位置。此模型把边界处理与算法耦合，也迫使库在每次评估前承担防御性验证开销。

本项目处于早期 demo 阶段。用户选择让初始化器和 Repair 共同负责位置合法性：算法不应读取上下界，而应在生成或修改位置后统一委托 Repair。

## 决策

- `ContinuousProblem` 不公开 `Bounds`，也不提供 `ValidatePosition`。它只保留维度；未传 Repair 时，它以构造参数中的边界创建默认 Clamp Repair。
- `ICandidateInitializer` 保持位置级职责，签名为 `Initialize(Span<double> position, Random random)`。算法必须要求调用方提供初始化器；初始化完成后立即调用 Repair。
- `ICandidateRepair` 签名为 `Repair(Span<double> position, Random random)`。Repair 自己拥有边界，算法和运行上下文不传入 Problem 或边界。
- 每个算法在初始化 Position 后、以及每次修改 Position 后调用 `OptimizationRunContext.Repair`。算法实现不得访问变量上下界。
- 内置 Repair 包括 Clamp、Reflect、RandomReset 与 DoNothing。默认使用 Clamp。
- `NaN` 一律保持不变。无界维度不处理。对有界维度，Clamp 将有限越界值和无穷值截断到端点；Reflect 对双侧有限边界的有限越界值镜像映射，并对无穷值或单侧边界退化为 Clamp；RandomReset 对双侧有限边界的非 `NaN` 越界值或无穷值重新均匀采样，并在其他维度退化为 Clamp 或不处理。
- DoNothing 不修改任何位置。它是显式风险选择；除非调用方能够自行保证初始化、位置更新和数值后果，否则不得使用。
- Core 继续验证构造配置、目标值和约束违背量的有限性与范围；位置本身不再由 Core 验证。Repair/Initializer 失职造成的位置后果由调用方负责。

## 替代方案

- 保留 `ContinuousProblem.ValidatePosition`：安全性更强，但重复暴露边界并使算法绕开统一 Repair 路径。
- 让算法读取 `ContinuousProblem.Bounds` 后自行截断：算法与边界策略耦合，且不利于镜像与随机回退等替代策略。
- 把速度、频率等算法内部状态交给通用初始化器：会把算法专属表示泄漏到 Core，且不能形成可复用的通用契约。

## 后果

这是刻意的破坏性 API 变更。调用方必须显式传入位置初始化器；自定义 Repair 必须在构造时保存自己需要的边界。算法循环、候选比较、目标/约束结果验证、随机流隔离和实验调度不改变。

内置 Repair 对位置错误提供常见恢复策略，但不处理 `NaN` 与无界维度。用户手册和 XML 文档必须突出 DoNothing 的风险以及 Repair 的调用时机。

## 重新评估条件

当引入其他候选表示、需要由库重新承担候选位置安全验证，或需要批量/设备端修复时，通过新的 ADR 重新评估本决策。
