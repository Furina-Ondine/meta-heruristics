# SPEC-0002 实施任务

## 执行规则

- 只实施已批准的 [`spec.md`](./spec.md) 与 [`plan.md`](./plan.md)；发现新的公共行为、职责、数值公式或性能设计时停止并退回澄清。
- 一个时间只能有一项任务处于 `InProgress`；每项同时完成对应代码、测试和必要 XML 注释。
- 不迁入旧仓库的公共框架、位置边界、全局随机流、共享向量工具、泛型 Fitness/Solution 或兼容壳。
- 正常 run 间可以复用 Optimizer 私有数组；异常后的实例继续复用由 Experiment 现有规则禁止。

## T001：实现并验证 PSO Optimizer

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-006`、`NFR-001`、`NFR-002`
- 依赖：无
- 影响区域：Algorithms 的 Pso 类型、Algorithms 测试。
- 实施内容：新增强类型 Options、RunGroup 私有双缓冲粒子状态和独立全局最佳快照；通过 Context 进行初始化、Repair、评估和随机性；按批准的速度/惯性/认知/社会更新式实施，并先添加对应契约测试。
- 明确不做：不添加位置边界参数、公共粒子类型、共享 VectorOps、批量评估、并行化或基准。
- 完成条件：PSO 的 API、参数验证、固定 seed、方向/约束、Repair、工作区复用、最佳快照和 Sphere fixture 全部有通过证据。
- 验证命令：`dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj --configuration Release -- --filter-namespace Anastasya.Metaheuristics.Tests.Algorithms`
- 验证结果：新增 7 项 PSO 契约测试；Algorithms 命名空间共 19 项测试通过，覆盖 Bat 回归与 PSO 的初始评估、比较、确定性、隔离、复用、Repair、最佳快照、参数边界和 Sphere fixture。

## T002：实现并验证萤火虫 Optimizer

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-004`、`FR-006`、`NFR-001`、`NFR-002`
- 依赖：`T001`
- 影响区域：Algorithms 的 Firefly 类型、Algorithms 测试。
- 实施内容：新增强类型 Options、私有当前/下一代工作区和独立最佳快照；实现严格更优、顺序移动、每次变更后 Repair、随机步长衰减和 Context 评估，并先添加对应契约测试。
- 明确不做：不比较等价候选、不修改当前代、不添加位置边界或共享种群框架。
- 完成条件：萤火虫的 API、参数验证、顺序移动、Repair、最佳快照、方向/约束、固定 seed、工作区复用和 Sphere fixture 全部有通过证据。
- 验证命令：`dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj --configuration Release -- --filter-namespace Anastasya.Metaheuristics.Tests.Algorithms`
- 验证结果：新增 7 项萤火虫契约测试；Algorithms 命名空间共 26 项测试通过，覆盖 Bat/PSO 回归与萤火虫的初始评估、比较、严格顺序移动、Repair、确定性、隔离、复用、参数边界和 Sphere fixture。

## T003：实现并验证布谷鸟 Optimizer

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-005`、`FR-006`、`NFR-001`、`NFR-002`
- 依赖：`T002`
- 影响区域：Algorithms 的 Cuckoo 类型、Algorithms 测试。
- 实施内容：新增强类型 Options、私有种群/候选/正态采样工作区与独立最佳快照；实现已批准的 Mantegna 候选、显式双尺度、衰减、最差巢遗弃、单巢保护、Repair 和 Context 评估，并先添加对应契约测试。
- 明确不做：不读取 Repair 边界、不复用旧静态幂缓存、不使用 `Random.Shared`、不引入公共 Gamma/随机工具或性能优化。
- 完成条件：布谷鸟的 API、参数验证、候选数、尺度、衰减、遗弃、单巢、方向/约束、固定 seed、最佳快照、工作区复用和 Sphere fixture 全部有通过证据。
- 验证命令：`dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj --configuration Release -- --filter-namespace Anastasya.Metaheuristics.Tests.Algorithms`
- 验证结果：新增 8 项布谷鸟契约测试；Algorithms 命名空间共 34 项测试通过，覆盖 Bat/PSO/萤火虫回归与布谷鸟的初始评估、比较、候选数、Repair、单巢、显式尺度、确定性、隔离、复用、参数边界和 Sphere fixture。

## T004：迁移示例与用户文档

- 状态：`InProgress`
- 覆盖需求：`FR-001`、`FR-007`
- 依赖：`T003`
- 影响区域：Examples、README、API Overview、User Guide、架构概览和 XML 文档。
- 实施内容：在 Example 中演示可替换的新增 Optimizer 及一个 Experiment Factory；更新入口地图与当前状态说明；补齐公开 Options/Optimizer 的 XML 所有权、生命周期、随机性、Repair 与数值语义。
- 明确不做：不在手写文档复制成员级异常/参数表，不把二进制遗传算法描述为已支持。
- 完成条件：示例可编译运行，DocFX 无警告，所有用户入口准确列出四种连续算法。
- 验证命令：`dotnet build Metaheuristics.NET.slnx --configuration Release`; `dotnet docfx docfx.json --warningsAsErrors`
- 验证结果：尚未执行

## T005：完成追踪和工程验证

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`FR-006`、`FR-007`、`NFR-001`、`NFR-002`
- 依赖：`T004`
- 影响区域：Verification、全仓库。
- 实施内容：创建逐需求 Verification，运行 Release restore/build/test、Example 烟雾、DocFX、文档门禁、残留搜索和差异检查；记录配置与 seed 固定的 Sphere 证据。
- 明确不做：不在 Verification 阶段引入新行为、性能声明或兼容层。
- 完成条件：所有需求有实现/测试/文档证据，残留审计通过，Spec 进入 `Implemented`。
- 验证命令：按 `verification.md` 的完整命令集合执行。
- 验证结果：尚未执行
