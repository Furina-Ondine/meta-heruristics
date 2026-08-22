# ADR-0008: Experiment RunGroup 与可复用 Worker

## 状态

状态：Superseded

由 [ADR-0009](0009-group-scoped-optimizer-execution.md) 替代。

替代 [ADR-0004](0004-composition-and-execution-model.md)。

## 背景

ADR-0004 规定一次 run 创建并释放一个独占 Session。该模型能够隔离状态，但批量实验中的每次重复都会重新分配种群位置、速度、适应度和临时缓冲区。对于高维、大种群或大量重复实验，这些物理分配会反复发生。

实验 Case 的运行时长和初始化成本差异只有用户最清楚。若把整个 Case 作为一个调度任务，长 Case 会在实验尾部造成单核长时间运行；若把所有 Repetition 一次性展开为并行 Task，又会制造过大的任务队列，并同时触发大量算法工作区初始化。

需要在保持 run 逻辑隔离、确定性和显式依赖的前提下，引入用户可控的 Case 拆分、有限调度与物理内存复用。

## 决策

- 用户以强类型配置和 Group Factory 定义 Case；Experiment 不使用字符串注册、反射构造或服务定位器。
- 每个 Case 声明 `Repetitions = N` 和 `RunGroupCount = P`，其中 `1 <= P <= N`，默认 `P = 1`。
- Planner 将 N 次 Repetition 确定性地均衡拆分为 P 个 RunGroup，并将不同 Case 的 Group 轮转交错。
- Scheduler 只认识 RunGroup，使用一个全局并发上限进行有界、惰性调度；不增加 `MaxConcurrentCases`，也不提前为全部 Repetition 创建 Task。
- Group Factory 为每个 RunGroup 创建独立 Problem 和 Optimizer。不同 Group 不共享这些实例，但可以引用同一份不可变输入数据。
- `Optimizer` 创建 Group 独占的 `Worker`。Worker 不保证线程安全，拥有种群数组和算法临时缓冲区，并在当前 Group 的多个顺序 run 之间复用这些物理存储。
- 每个 run 创建独立的运行上下文和 `Random(seed)`。Worker 在运行前重置全部逻辑状态并重新填写种群内容，不复用上一 run 的随机状态、最佳状态、计数或轨迹。
- 具体算法自行管理 Worker 工作区；Core 不建立统一数组池或通用 Workspace 布局。
- 单次 Runner API 内部创建、使用并释放一个 Worker；Experiment 在 RunGroup 完成后释放 Worker。
- 一个 run 异常后继续实验，但当前 Worker 视为可能损坏并被销毁；Group 剩余 run 使用新建环境继续。Group 初始化或重建失败时，其剩余 run 标记失败。
- 取消停止投放新 Group，协作取消已开始的 run，并返回包含已完成、失败、取消和未开始记录的部分结果。
- Experiment 使用共享 seed 序列，不同 Case 的相同 Repetition 默认使用相同 seed。Group 拆分和并发度不得改变 seed。
- 成功 run 的最佳位置保存到 Case 级矩形二维数组；公开只读访问，不暴露可变数组。
- 第一波聚合提供均值、中位数、最小值、最大值和样本标准差，只统计成功 run；不提供文件导出、高级统计或进度通知。
- `IStoppingCondition` 保持现有接口，但必须可重入且不保存 run 级可变状态。

## 替代方案

- 每次 run 创建独立 Session 和全部数组：隔离简单，但无法复用主要工作区，保留为单次运行便利路径的内部行为而非批量实验路径。
- 整个 Case 作为一个任务：初始化次数最少，但长 Case 会造成明显尾部负载不均。
- 每个 Repetition 创建一个 Task：细粒度负载均衡更好，但任务数量、同时初始化量和工作区峰值难以控制。
- Scheduler 同时维护 Case 间和 Case 内并发度：可以动态分配配额，但显著增加调度状态；RunGroup 已能由用户直接表达 Case 最大并行度和复用粒度。
- Config 由 Experiment 自动解释并构造实例：调用表面较短，但需要注册表、反射、服务定位或限制构造方式，不适合开放算法与用户问题。
- 自定义可重置 PRNG：可避免每个 run 的一个小对象分配，但增加算法正确性、版本稳定性和统计测试维护成本；没有分配数据证明其必要性。
- 全局数组池：可能进一步降低分配，但不同算法的工作区形状、所有权和清理要求尚未稳定，首版由算法 Worker 自主管理更清晰。

## 后果

批量实验可以在用户指定的并行度和内存副本数之间取舍，并把主要数组分配摊销到同一 Group 的多个 run。调度器保持全局有界且不依赖具体算法。

Worker Reset 成为新的关键正确性边界。算法必须完整覆盖上一次 run 的逻辑状态；测试必须证明 seed、种群、最佳状态、计数和轨迹不跨 run 污染。运行异常后重建 Worker 会牺牲低频失败路径的分配收益，以保证继续执行的可靠性。

静态 RunGroup 拆分不能解决所有未知时长问题。Group 内某个不结束的 run 会阻塞其后续 run；进程内执行也无法安全强杀不响应取消的代码。用户需要通过 `RunGroupCount`、停止条件和协作取消控制风险。

接受本决策后，现有 Core Session 实现与最新决策之间存在明确的迁移阶段。实现完成前，架构概览必须继续报告实际 Session 状态，不得把 Worker API 描述为已交付。

## 重新评估条件

出现以下需求时通过新 ADR 重新评估：

- 远程、跨进程或可强制终止的执行后端；
- 动态 Case 配额、优先级、权重或工作窃取；
- 基准证明需要全局数组池、自定义 PRNG 或其他通用工作区抽象；
- Stateful stopping condition、异步 Group 初始化或异步资源释放成为稳定用例；
- 结果规模要求流式存储或不保留全部最佳位置。
