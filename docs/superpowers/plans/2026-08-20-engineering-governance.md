# Engineering Governance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 建立精炼、分层且同时约束人类贡献者和 AI 代理的工程治理文档，并记录已确认的技术选择。

**Architecture:** `ENGINEERING.md` 是长期工程契约的唯一权威来源，`AGENTS.md` 只提供代理执行入口；架构概览描述当前状态，ADR 解释历史选择，README 仅保留面向使用者的摘要和链接。文档避免复制同一规则，变更决策时通过新 ADR、工程规范和架构概览同步更新。

**Tech Stack:** Markdown、.NET 10、Git、PowerShell、Semantic Versioning 2.0.0

## Global Constraints

- 所有文档使用简洁中文；一项规则只在一个权威文件中完整定义，其他文件使用链接。
- 不修改运行时代码、项目文件、依赖或公共 API。
- 保留 `docs/superpowers/specs/2026-08-19-project-foundation-design.md` 和现有实施计划。
- `Metaheuristics.Core`、`Metaheuristics.Algorithms` 和 `Metaheuristics.Experiments` 统一版本并锁步发布。
- 首版继续采用连续 `double` 候选和标量 `double` 目标值；泛型单目标值通过 Objective Policy 定义能力，不要求 `IComparable<T>`。
- 首版不引入远程执行、集群、GPU、多目标、二进制或排列表示。
- 每个任务结束时执行 `git diff --check`，只提交该任务拥有的文件。

---

### Task 1: 工程规范与代理入口

**Files:**
- Create: `ENGINEERING.md`
- Create: `AGENTS.md`

**Interfaces:**
- Consumes: `docs/superpowers/specs/2026-08-20-engineering-governance-design.md`
- Produces: 后续架构和 ADR 文档引用的工程规范入口

- [ ] **Step 1: 创建精炼的 `ENGINEERING.md`**

按以下固定章节写入规则，每节只保留可执行约束：文档权威性、项目边界、API 与扩展、运行状态与随机性、数值正确性、性能、测试和质量、代码风格、版本发布、决策变更。

必须明确：既定依赖图；用户装配而库管理生命周期；`Optimizer` 与 `Session` 隔离；禁止全局随机流和字符串注册中心；性能优化需要基准；Release 构建与测试门槛；SemVer 和三个运行时包锁步发布；重大架构变化需要 ADR。

- [ ] **Step 2: 创建简短的 `AGENTS.md`**

只写代理工作入口：修改前阅读 `ENGINEERING.md`、架构概览和相关 ADR；保护用户修改；行为变化由测试定义；不绕过依赖、状态和数值规则；完成前验证；发现文档冲突时报告。通过相对链接引用权威文档，不复制具体技术论证。

- [ ] **Step 3: 验证入口文档**

Run:

```powershell
Test-Path ENGINEERING.md
Test-Path AGENTS.md
rg -n "ENGINEERING.md|architecture/overview|docs/decisions" AGENTS.md
git diff --check
```

Expected: 两个 `Test-Path` 均为 `True`，代理入口包含三类权威链接，`git diff --check` 无输出。

- [ ] **Step 4: 提交工程规范**

```powershell
git add ENGINEERING.md AGENTS.md
git commit -m "docs: add engineering governance"
```

### Task 2: 当前架构与 ADR 机制

**Files:**
- Create: `docs/architecture/overview.md`
- Create: `docs/decisions/README.md`

**Interfaces:**
- Consumes: `ENGINEERING.md`
- Produces: 当前架构视图和所有 ADR 使用的格式、状态与替代规则

- [ ] **Step 1: 创建架构概览**

文档只描述当前状态，包含：六个项目职责；批准的项目依赖图；`Problem + Optimizer -> Runner -> Session -> Result` 单次运行流程；`typed factories -> isolated runs -> aggregate` 批量实验流程；当前扩展点；首版明确排除项。对理由只链接 ADR，不在概览重复展开。

- [ ] **Step 2: 创建 ADR 索引和模板**

`docs/decisions/README.md` 说明 ADR 不原地重写历史决策，使用 `Accepted`、`Superseded`、`Rejected` 三种状态；固定章节为状态、背景、决策、替代方案、后果、重新评估条件；列出 `0001` 至 `0007` 的索引。

- [ ] **Step 3: 验证结构和链接目标**

Run:

```powershell
Test-Path docs/architecture/overview.md
Test-Path docs/decisions/README.md
rg -n "Core|Algorithms|Experiments|Examples|Tests|Benchmarks" docs/architecture/overview.md
rg -n "Accepted|Superseded|Rejected|0001|0007" docs/decisions/README.md
git diff --check
```

Expected: 文件存在，概览列出六个项目，ADR 索引包含状态和完整编号，diff 检查无输出。

- [ ] **Step 4: 提交架构文档**

```powershell
git add docs/architecture/overview.md docs/decisions/README.md
git commit -m "docs: describe architecture and decisions"
```

### Task 3: 平台、范围、分层和执行 ADR

**Files:**
- Create: `docs/decisions/0001-platform-and-toolchain.md`
- Create: `docs/decisions/0002-library-scope-and-evolution.md`
- Create: `docs/decisions/0003-project-and-package-boundaries.md`
- Create: `docs/decisions/0004-composition-and-execution-model.md`

**Interfaces:**
- Consumes: `docs/decisions/README.md`, `ENGINEERING.md`, `docs/architecture/overview.md`
- Produces: 平台、路线、包边界和运行模型的决策依据

- [ ] **Step 1: 记录平台与工具链决策**

`0001` 记录 .NET 10、`net10.0`、默认稳定 C#、`.slnx`、集中包版本、xUnit v3、Microsoft Testing Platform、BenchmarkDotNet、跨平台运行时项目；替代方案包括旧 LTS、多目标框架和额外风格工具，说明目前不采用的原因。

- [ ] **Step 2: 记录库范围与演进顺序**

`0002` 记录连续 `double` + 标量 `double` 起步，之后依次为 Objective Policy 泛型单目标值、多目标、二进制和排列表示；明确 Policy 而非 `IComparable<T>` 定义比较和统计能力；暂不建设远程、集群或 GPU 后端。

- [ ] **Step 3: 记录项目和包边界**

`0003` 记录六个项目的职责和唯一允许的依赖方向，明确 `Experiments` 不引用具体算法，论文业务代码不进入运行时包。

- [ ] **Step 4: 记录构造与执行模型**

`0004` 记录用户装配、库管理生命周期；单次运行传实例、批量实验传强类型工厂；`Optimizer -> Session -> Runner` 分工；拒绝全局注册中心、服务定位器和跨运行共享状态。

- [ ] **Step 5: 验证四份 ADR**

Run:

```powershell
rg -l "状态.*Accepted" docs/decisions/0001-*.md docs/decisions/0002-*.md docs/decisions/0003-*.md docs/decisions/0004-*.md
rg -n "IComparable|Objective Policy|GPU|Session|强类型工厂" docs/decisions/000*.md
git diff --check
```

Expected: 四份 ADR 均为 `Accepted`，关键决策可检索，diff 检查无输出。

- [ ] **Step 6: 提交基础 ADR**

```powershell
git add docs/decisions/0001-platform-and-toolchain.md docs/decisions/0002-library-scope-and-evolution.md docs/decisions/0003-project-and-package-boundaries.md docs/decisions/0004-composition-and-execution-model.md
git commit -m "docs: record foundational decisions"
```

### Task 4: 数值、性能、版本 ADR 与 README 导航

**Files:**
- Create: `docs/decisions/0005-candidate-objective-and-constraints.md`
- Create: `docs/decisions/0006-evaluation-performance-and-reproducibility.md`
- Create: `docs/decisions/0007-versioning-and-release.md`
- Modify: `README.md`

**Interfaces:**
- Consumes: `ENGINEERING.md`, `docs/architecture/overview.md`, `docs/decisions/README.md`
- Produces: 完整首批决策集和用户可发现的文档入口

- [ ] **Step 1: 记录候选、目标值和约束决策**

`0005` 记录首版连续 `double` 向量、标量 `double`、优化方向；可行性优先；两个不可行解按归一化加权总违背量比较；等式约束使用绝对与相对容差；约束判断和修复分离。

- [ ] **Step 2: 记录评估、性能与可复现性决策**

`0006` 记录单点同步评估为基础契约，本机同步批量评估为可选快速路径；每个 run 独立种子和随机流；并发度不改变单次序列；性能优化必须由基准支撑且不能损害确定性。

- [ ] **Step 3: 记录版本与发布决策**

`0007` 记录 SemVer 2.0.0；三个运行时包使用同一版本并锁步发布；`1.0.0` 前破坏性变化至少提升 `MINOR`；`1.0.0` 后 `MAJOR`、`MINOR`、`PATCH` 分别对应不兼容变化、兼容功能和兼容修复；使用 `alpha`、`beta`、`rc` 预发布标识。

- [ ] **Step 4: 精简 README 并增加导航**

保留项目定位、首版范围、路线图、开发命令和项目来源。把详细架构、执行模型和质量规则缩为摘要，链接 `ENGINEERING.md`、`docs/architecture/overview.md` 和 `docs/decisions/README.md`，避免与权威文件重复。

- [ ] **Step 5: 执行完整验证**

Run:

```powershell
Test-Path docs/decisions/0005-candidate-objective-and-constraints.md
Test-Path docs/decisions/0006-evaluation-performance-and-reproducibility.md
Test-Path docs/decisions/0007-versioning-and-release.md
rg -n "ENGINEERING.md|docs/architecture/overview.md|docs/decisions/README.md" README.md
rg -n "T[B]D|T[O]DO|待.定|PLACEHOLDER|FIXME" ENGINEERING.md AGENTS.md README.md docs/architecture docs/decisions
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
git diff --check
```

Expected: 三份 ADR 存在；README 包含三个入口；占位符搜索无匹配；restore、build、test 成功且测试失败数为零；diff 检查无输出。

- [ ] **Step 6: 提交完整治理文档**

```powershell
git add README.md docs/decisions/0005-candidate-objective-and-constraints.md docs/decisions/0006-evaluation-performance-and-reproducibility.md docs/decisions/0007-versioning-and-release.md
git commit -m "docs: complete engineering decisions"
```

- [ ] **Step 7: 确认最终仓库状态**

Run:

```powershell
git status --short --branch
git log --oneline -5
```

Expected: 当前分支为 `main`，工作区干净，最近提交包含本计划的四个文档提交。
