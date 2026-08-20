# ADR-0003: 项目与包边界

## 状态

状态：Accepted

## 背景

核心契约、算法实现、实验编排和开发辅助代码需要分离，防止具体算法或论文业务反向塑造公共运行时 API。

## 决策

六个项目及职责如下：

- `Metaheuristics.Core`：稳定运行时契约，不引用其他仓库项目。
- `Metaheuristics.Algorithms`：具体算法，只依赖 `Core`。
- `Metaheuristics.Experiments`：批量实验编排、强类型工厂与结果聚合，只依赖 `Core`，不引用具体算法。
- `Metaheuristics.Examples`：使用示例，依赖 `Core`、`Algorithms` 和 `Experiments`。
- `Metaheuristics.Tests`：契约与行为测试，依赖 `Core`、`Algorithms` 和 `Experiments`。
- `Metaheuristics.Benchmarks`：运行时热路径基准，依赖 `Core` 和 `Algorithms`。

以上是唯一允许的仓库项目依赖方向。论文业务模型、实验专用逻辑和领域解码不得进入三个运行时包。

## 替代方案

- 单一项目：初期文件较少，但无法可靠约束依赖和发布边界。
- `Experiments` 引用 `Algorithms`：装配更直接，但会把实验基础设施绑定到内置算法。
- 将论文代码纳入运行时包：便于复用既有实现，却会污染通用 API。

## 后果

核心契约可以独立演进，实验层可编排内置或外部算法；新增跨项目能力必须放在依赖方向允许的位置，必要时先形成新 ADR。

## 重新评估条件

当实际用例证明现有职责无法容纳稳定能力，或需要新增可独立发布的包时，通过新 ADR 调整边界。
