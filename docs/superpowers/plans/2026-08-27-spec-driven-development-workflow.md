# Spec-Driven 开发工作流实施计划

## 规格基线

- 对应设计：[Spec-Driven 开发工作流设计](../specs/2026-08-26-spec-driven-development-workflow-design.md)
- 设计状态：`Approved`
- 批准日期：2026-08-27
- 基线提交：`a47141a`

本计划建立仓库原生的 SDD 治理、文档信息架构、生成式 API Reference 和注释质量门禁。它不在未经独立 Spec 批准的情况下改变运行时公共行为或数值语义。

## 当前实现调查

### 已有基础

- `ENGINEERING.md` 已承担持续工程契约，Accepted ADR 已承担决策理由。
- `docs/superpowers/specs/` 与 `plans/` 已存在设计和计划，但被定义为过程或历史档案，格式、状态和完成记录不统一。
- README、User Guide、Developer Guide 和三份手写 API 文档已经存在。
- 三个运行时项目均生成 XML 文档文件，Release 构建和测试当前通过。

### 已知问题

- Approved 功能规格还不是公共行为的权威来源，也没有统一 change package、状态或追踪规则。
- 现有计划存在已完成任务仍未勾选、设计仍标记待审、索引遗漏和替代 ADR 链接失效等漂移。
- README、User Guide、Developer Guide 和 API 文档重复生命周期、线程安全、随机性和 Repair 契约，却没有清晰的读者学习路径。
- `docs/api/*.md` 手工复述成员契约，和 XML 注释形成两个需要同步的来源。
- XML 注释中既有简单属性标签，也有类和方法只复述名称的低信息内容；部分注释已经与实现失配，例如 `BatOptimizerOptions` 仍把候选范围归于 `ContinuousProblem`。
- 仓库没有自动检查 Markdown 链接、规格状态、需求追踪、DocFX 构建或陈旧类型引用的门禁。

## 方案选择

| 方案 | 优点 | 成本与风险 | 结论 |
| --- | --- | --- | --- |
| 仓库原生模板、验证脚本与 DocFX | 可按项目风格定制；不依赖代理专用命令；能渐进迁移现有仓库。 | 需要自行维护少量模板和验证逻辑。 | 采用。 |
| 直接初始化完整 Spec Kit | 命令和模板成熟，具有 clarify/analyze 等流程。 | 会一次性引入较多目录、提示和流程；未先验证是否适合本项目。 | 试点后再评估。 |
| 只修改 `AGENTS.md` 和人工约定 | 实施最快。 | 状态、追踪、链接和 Reference 仍不可验证，容易再次漂移。 | 不采用。 |

成员级 API Reference 使用仓库本地工具清单固定 DocFX `2.78.5`。该版本支持 .NET 10 和 `.slnx`，能够从程序集或项目及其 XML 注释生成 .NET API 文档。首版只要求本地和 CI 可构建，不在本计划中发布网站。

## 目标结构

```text
docs/
  specs/
    README.md
    _templates/
      spec.md
      plan.md
      tasks.md
      verification.md
    SPEC-NNNN-feature-name/
      spec.md
      plan.md
      tasks.md
      verification.md
  api/
    overview.md
  reference/                  # DocFX 配置和手写入口，不提交生成产物
    docfx.json
    toc.yml
eng/
  verify-documentation.ps1
.config/
  dotnet-tools.json
```

`docs/superpowers/` 保留为历史设计与实施档案，不整体迁移。新的权威功能规格只进入 `docs/specs/`。

## 任务 1：建立治理决策和权威关系

### 影响文件

- 新增 `docs/decisions/0014-spec-driven-change-governance.md`
- 更新 `docs/decisions/README.md`
- 更新 `ENGINEERING.md`
- 更新 `AGENTS.md`
- 更新 `docs/superpowers/README.md`

### 实施内容

- 用 ADR-0014 记录双轨风险分类、Approved Spec 的权威地位、批准门、升级规则和历史 `superpowers` 档案的边界。
- 在 `ENGINEERING.md` 中加入显式策略组合、最小必要验证、单一职责归属、彻底替代、文档内容所有权和注释质量规则。
- 本任务只记录验证哲学，不立即修改“目标值必须有限”等现行数值行为；Infinity 语义通过后续独立 Spec 和替代 ADR 落地。
- 在 `AGENTS.md` 中要求修改前分类：高风险走完整 SDD，轻量变更先给短设计，隐藏复杂度只能升级。
- 明确发生冲突时的次序：工程宪法和 Accepted ADR → 对应 Approved/Implemented Spec → 当前状态文档和实现；冲突必须报告，不能选择方便的解释。

### 完成条件

- 新旧权威来源不存在循环定义。
- ADR 状态、索引和替代关系有效。
- `AGENTS.md` 能让新代理在进入代码前判断应使用哪条流程。

## 任务 2：建立权威 change package 与模板

### 影响文件

- 新增 `docs/specs/README.md`
- 新增 `docs/specs/_templates/spec.md`
- 新增 `docs/specs/_templates/plan.md`
- 新增 `docs/specs/_templates/tasks.md`
- 新增 `docs/specs/_templates/verification.md`

### 实施内容

- 定义 `SPEC-NNNN-kebab-case` 的唯一编号与目录规则。
- 把设计中批准的 Spec、Plan、Tasks、Verification 固定章节完整落入模板。
- Spec 模板包含 `FR-NNN`、`NFR-NNN`、信任边界、职责替代、成功标准、待澄清问题和批准记录。
- Plan 模板包含当前实现调查、至少两个方案、验证成本、删除清单、影响矩阵、需求到证据映射和 ADR 判断。
- Tasks 模板要求每项任务引用需求编号、依赖、明确不做、完成条件、命令和实际结果。
- Verification 模板要求需求追踪、替代残留、架构一致性和工程验证证据。
- 定义合法状态和状态迁移；影响公共行为的未决问题存在时禁止进入 `Approved`。

### 完成条件

- 从模板创建的 change package 不需要代理自行发明章节或状态。
- 每个计划任务和验证证据均能追溯到 Spec 需求。

## 任务 3：实现文档和规格验证门禁

### 影响文件

- 新增 `eng/verify-documentation.ps1`
- 新增 `.github/workflows/ci.yml`
- 更新 README 和 `ENGINEERING.md` 的验证命令

### 验证范围

- 所有本地 Markdown 链接目标存在。
- `docs/specs/` 中编号唯一，目录名和文件集合合法。
- Spec 状态只使用批准集合，`Superseded` 必须指向替代 Spec。
- `Approved` Spec 不含未解决的公共行为问题或占位符。
- Plan、Tasks 和 Verification 引用存在的 `FR/NFR`；无来源任务和无证据需求失败。
- Tasks 状态和依赖引用合法。
- ADR 索引与文件状态一致，替代链接有效。
- 文档中不残留已明确删除的公共类型名；残留词表由 change package 的 Verification 提供。

### 实施约束

- 首版使用跨平台 PowerShell 7 和仓库文件，不引入服务或数据库。
- 脚本失败必须指出文件、规则和可修复原因。
- 先在本地验证稳定，再接入 CI；CI 同时执行 Release restore、build、test、DocFX 和文档验证。

### 完成条件

- 对至少一个刻意构造的失效链接、重复编号、缺失需求和无证据需求，验证器能够失败并给出定位。
- 当前仓库在修复已知漂移后通过验证。

## 任务 4：建立生成式 API Reference

### 影响文件

- 新增或更新 `.config/dotnet-tools.json`，固定 DocFX `2.78.5`
- 新增 `docs/reference/docfx.json` 与 `docs/reference/toc.yml`
- 更新 `.gitignore`
- 新增 `docs/api/overview.md`
- 删除被替代的 `docs/api/core.md`、`docs/api/algorithms.md`、`docs/api/experiments.md`
- 更新仓库内指向旧 API 文档的链接

### 实施内容

- DocFX 从三个运行时项目或 Release 程序集和相邻 XML 文件生成公共成员 Reference。
- 配置只包含 public/protected 可调用 API，生成目录不提交版本控制。
- DocFX warning 作为失败处理，确保无效 `<see>`、`<paramref>` 和无法解析的成员被发现。
- `docs/api/overview.md` 只保留按任务组织的入口地图：定义问题、单次运行、批量实验和实现策略。
- 不在 Overview 中复制参数、异常、属性或重载说明；这些内容只来自 XML Reference。

### 完成条件

- `dotnet tool restore` 后能够在干净环境生成三个程序集的 Reference。
- API Overview 中每条入口都链接到生成的对应类型。
- 删除旧 API Markdown 后仓库不存在失效链接。

## 任务 5：按读者任务重写手写文档

### 影响文件

- `README.md`
- `docs/guides/user-guide.md`
- `docs/architecture/developer-guide.md`
- `docs/architecture/overview.md`
- `examples/Metaheuristics.Examples/Program.cs`，仅在需要建立可引用示例区域时修改

### README

- 收敛为定位、当前能力与限制、最短示例、单次/批量两个入口和文档导航。
- 删除详细生命周期、线程安全和成员级契约的重复说明。

### User Guide

- 以使用内置算法解决一个问题为第一学习路径。
- 先建立能力、两条入口和五个必需概念，再给出完整单次运行。
- 之后按最大化、Repair、约束、停止、seed 和批量实验等用户任务渐进展开。
- 使用 DocFX 代码 include 或指向可构建 Example 的单一源码区域，避免复制不可编译的示例。

### Developer Guide

- 按 Objective/Constraint、Initializer/Repair、Stopping Condition、Optimizer 和 Experiment 扩展任务组织。
- 保留每类扩展必须遵守的状态、随机性、所有权和性能约束，但链接权威契约，不复制整段规则。
- 增加修改既有职责时的影响调查和彻底替代入口。

### 可理解性验收

- 依据设计中的读者问题逐项走查，并记录答案所在章节。
- 同一完整契约只保留一个权威来源；其他位置最多保留任务相关摘要和链接。
- 架构概览只描述当前状态，不承担教程或变更流程说明。

## 任务 6：审计和重写 XML 注释

### 影响范围

- `src/Metaheuristics.Core/**/*.cs`
- `src/Metaheuristics.Algorithms/**/*.cs`
- `src/Metaheuristics.Experiments/**/*.cs`

### 实施内容

- 生成公共 API 清单，逐项核对实现、测试、Approved Spec 和 Accepted ADR。
- 优先修复错误契约，包括仍声称边界由 `ContinuousProblem` 拥有的注释。
- 类、结构、记录、接口、构造函数、方法、事件、委托和非显然枚举必须提供签名之外的信息。
- 简单属性允许“获取某值”式最低注释；具有范围、默认值、所有权、生命周期、可空、状态或数值语义的属性必须补充完整契约。
- 删除或重写逐句复述实现、无依据性能承诺和已失效架构说明。
- 将参数、返回值、异常、线程安全、所有权和特殊数值语义保留在 XML，不在 Overview 重复。

### 完成条件

- DocFX 以 warnings-as-errors 成功生成。
- 公共 API 注释抽查覆盖 Core、Algorithms、Experiments 的入口、策略接口和高风险借用数据。
- 注释中的类型名、异常条件和生命周期与测试及实现一致。
- 本任务不顺带改变运行时行为；发现契约和实现冲突时先报告并创建独立 Spec。

## 任务 7：以数值特殊值语义作为首个 SDD 试点

### 目标

创建 `docs/specs/SPEC-0001-evaluation-special-values/`，把已经确认的意图正式规格化：目标值 `NaN` 必须失败，正负 Infinity 可以按优化方向比较；约束违背量保持非负且拒绝 `NaN`，正 Infinity 可以表达无界不可行程度。

### 边界

- 本任务只完成 Spec、澄清和用户批准，不预先实施运行时代码。
- Spec 必须列出当前 `Evaluation`、`ConstraintEvaluation`、比较、统计、Experiment、测试、XML 和用户文档的连带影响。
- Spec 批准后另行生成其 Plan 和 Tasks，并用新流程完成实现。

### 完成条件

- 验证器能够检查首个真实 change package。
- 试点暴露的模板负担、遗漏和误报被记录到 Verification 或工作流改进项。

## 任务 8：最终验证与交付

```powershell
dotnet tool restore
dotnet restore Metaheuristics.NET.slnx --property:NuGetAudit=false
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
pwsh ./eng/verify-documentation.ps1
dotnet docfx docs/reference/docfx.json --warningsAsErrors
dotnet format Metaheuristics.NET.slnx --verify-no-changes --no-restore
git diff --check
```

最终报告必须包含：

- 文档链接、规格状态和需求追踪验证结果；
- DocFX 生成结果和 warning 数量；
- Reader Guide 问题到章节的映射；
- XML 注释抽查清单及发现的行为冲突；
- 被删除旧 API 文档和更新链接的残留搜索；
- 首个试点进入下一阶段前需要用户批准的边界。

## 风险与回退

- **流程过重**：只迁移新 change package，不批量改写历史设计；试点后再调整模板。
- **验证脚本误报**：规则必须输出具体文件和依据；无法可靠机械判断的信息增量保留人工审查。
- **DocFX 引入成本**：工具由本地 manifest 固定，不成为运行时包依赖；生成产物不提交。
- **文档重写造成契约漂移**：以实现、测试、Accepted ADR 和 Approved Spec 交叉核对；冲突停止并报告。
- **一次变更范围过大**：任务 1—4 先建立基础设施，任务 5—6 再重构内容；每个任务单独验证并保持可回退提交。

## 计划批准门

本计划获得用户批准前，不开始任务 1 的实施。实施过程中出现未列出的公共行为、架构选择或运行时语义时，停止并回到 Spec/Plan 审批。
