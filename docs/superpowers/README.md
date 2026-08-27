# Superpowers 设计与实施档案

本目录保存具体变更的设计和实施过程，分为两类：

- `specs/`：说明任务的设计意图、范围、约束和预期结果；
- `plans/`：把对应设计拆解为可执行的实施步骤、验证方式和交付边界。

`superpowers` 是创建这些文档时沿用的工作流目录名；这里的文件不参与项目编译或运行，也不是使用库时必须安装的组件。

两类文档都是过程/历史档案，不是现行工程规范。发生冲突时，依次以以下资料为准：

1. [`ENGINEERING.md`](../../ENGINEERING.md)：持续有效的工程契约；
2. [`docs/decisions/`](../decisions/README.md) 中状态为 `Accepted` 的 ADR：解释当前决策的背景、理由和替代关系；
3. [`docs/architecture/overview.md`](../architecture/overview.md) 与当前实现：报告和体现系统现状。

## 设计规格（specs）

| 日期 | 主题 | 设计规格（spec） | 对应实施计划（plan） | 用途 |
| --- | --- | --- | --- | --- |
| 2026-08-19 | 项目基础设计 | [项目基础设计](specs/2026-08-19-project-foundation-design.md) | [项目脚手架计划](plans/2026-08-19-project-scaffolding.md) | 定义首版范围、项目结构、依赖边界、问题与评估模型、执行模型及迁移顺序。 |
| 2026-08-20 | 工程治理文档设计 | [工程治理文档设计](specs/2026-08-20-engineering-governance-design.md) | [工程治理计划](plans/2026-08-20-engineering-governance.md) | 设计 README、ENGINEERING、AGENTS、架构概览、ADR 与本目录的职责边界，以及工程治理和发布规则。 |
| 2026-08-22 | Experiment 执行架构 | [Experiment 执行架构与接口设计](specs/2026-08-22-experiment-execution-design.md) | [Experiment 第一版实施计划](plans/2026-08-22-experiment-execution.md) | 定义多 Case、RunGroup 拆分、有界调度、Optimizer 内存复用、seed、部分结果和统计语义。 |
| 2026-08-22 | 蝙蝠算法第一波迁移 | [蝙蝠算法第一波迁移设计](specs/2026-08-22-bat-algorithm-migration-design.md) | [蝙蝠算法第一波迁移实施计划](plans/2026-08-22-bat-algorithm-migration.md) | 审计旧仓库 fix 分支并迁移双缓冲 Bat Optimizer、工作区复用和正确性回归。 |
| 2026-08-26 | Spec-Driven 开发工作流 | [Spec-Driven 开发工作流设计](specs/2026-08-26-spec-driven-development-workflow-design.md) | [Spec-Driven 开发工作流实施计划](plans/2026-08-27-spec-driven-development-workflow.md) | 定义风险分级、架构风格、文档信息架构、注释质量、Spec/Plan/Tasks/Verification 产物和批准门。 |

## 实施计划（plans）

| 日期 | 主题 | 实施计划（plan） | 对应设计规格（spec） | 用途 |
| --- | --- | --- | --- | --- |
| 2026-08-19 | 项目脚手架实施计划 | [项目脚手架计划](plans/2026-08-19-project-scaffolding.md) | [项目基础设计](specs/2026-08-19-project-foundation-design.md) | 将项目基础设计落地为可构建的 .NET 10 六项目解决方案，并验证依赖图、测试、示例和基准宿主。 |
| 2026-08-20 | 工程治理实施计划 | [工程治理计划](plans/2026-08-20-engineering-governance.md) | [工程治理文档设计](specs/2026-08-20-engineering-governance-design.md) | 按任务落地 ENGINEERING、AGENTS、架构概览、ADR 集合和 README 文档导航。 |
| 2026-08-22 | Experiment 第一版实施计划 | [Experiment 第一版实施计划](plans/2026-08-22-experiment-execution.md) | [Experiment 执行架构与接口设计](specs/2026-08-22-experiment-execution-design.md) | 将有状态 Optimizer、单点评估、RunGroup 调度、部分结果和统计设计落地为代码、测试与 API 文档。 |
| 2026-08-22 | 蝙蝠算法第一波迁移实施计划 | [蝙蝠算法第一波迁移实施计划](plans/2026-08-22-bat-algorithm-migration.md) | [蝙蝠算法第一波迁移设计](specs/2026-08-22-bat-algorithm-migration-design.md) | 将修复后的旧 Bat 变体适配到当前 Core 生命周期、方向、约束、seed 和示例。 |
| 2026-08-27 | Spec-Driven 开发工作流实施计划 | [Spec-Driven 开发工作流实施计划](plans/2026-08-27-spec-driven-development-workflow.md) | [Spec-Driven 开发工作流设计](specs/2026-08-26-spec-driven-development-workflow-design.md) | 建立权威 change package、验证门禁、DocFX Reference、读者任务式文档和注释质量审计。 |
