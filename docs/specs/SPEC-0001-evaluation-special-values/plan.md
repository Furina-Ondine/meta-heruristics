# SPEC-0001 技术计划

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`af81f402e42ee8e2b9d2454ffe69e8a1f9580dee`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`
- 批准人：项目作者
- 批准日期：2026-08-27

## 当前实现调查

- 当前相关类型和入口：Core 的 `ContinuousProblem.Evaluate`、`Evaluation`、`ConstraintEvaluation`、`EvaluationComparer` 和 `StoppingConditions.TargetObjective`；Experiments 的 `NumericStatistics`、`DurationStatistics` 与私有 `ExperimentStatisticsCalculator`。
- 当前调用链：Objective 返回值先在 `ContinuousProblem` 被 `IsFinite` 拒绝，再进入同样要求有限值的 `Evaluation`；每个 Constraint 结果先被 `ContinuousProblem` 拒绝非有限值，有限正值求和后再次拒绝溢出，最后交给要求有限汇总的 `ConstraintEvaluation`。比较器直接使用 `double.CompareTo`。Experiment 只聚合成功 run 的最佳目标值，以 `Average`、排序中点和平方差计算统计。
- 当前职责所属层：Core 已正确拥有单次评估域、比较和停止语义；Experiments 已正确拥有跨 run 统计。无需移动项目职责或新增跨项目依赖。
- 已存在的相似或重复概念：`ContinuousProblem` 与两个公开值对象分别验证同一数值域，这是不同信任入口所需的边界验证，不是应删除的重复抽象；算法不重复验证评估结果。
- 当前测试：`ContinuousProblemTests` 只证明 Objective 的 `NaN` 被拒绝；`EvaluationComparerTests` 只覆盖有限值；没有专门的 TargetObjective 特殊值测试；`ExperimentRunnerTests` 只覆盖有限样本与单样本标准差。
- 当前示例和文档：Example 直接用格式字符串输出 `BestObjective?.Mean`；XML 注释和异常文本多处写“有限”；`ENGINEERING.md` 只说要验证结果，尚未定义扩展实数域；API Reference 从 XML 自动生成。
- 当前基准：没有评估标量校验或统计计算专用基准。统计现有成本为一次数组物化、一次排序及两次线性遍历。
- 与 Spec 的冲突：所有 `IsFinite` 评估边界、TargetObjective 的有限阈值限制、`NumericStatistics` 三个非空属性和现有统计算术均不满足 Approved Spec。
- 与 ADR 的关系：ADR-0012 是现行有限性规则的历史来源，但已经整体由 ADR-0013 替代；ADR-0013 只决定 Repair 边界且保持不变。新的持续数值规则需要独立 ADR，而不是再次替代 ADR-0012。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A. 在评估边界显式分类数值；统计先分类 Infinity，再用缩放后的有限值计算 Mean/StandardDeviation | 规则与 Spec 一一对应；不会先制造 `NaN` 再猜原因；极端有限值也能避免中间求和溢出；不增加公共抽象。 | 统计帮助方法比现有 LINQ 表达式更长，需要完整组合测试。 | 低；职责仍分别位于 Core 与 Experiments。 | 采用。 |
| B. 保留现有 IEEE 算术，最终把所有 `NaN` 替换为 `null` | 改动最少。 | 无法区分相反 Infinity 导致的无定义值与有限样本中间溢出造成的算法错误；错误来源被抹平。 | 高；违背明确失败和可预测数值语义。 | 不采用。 |
| C. 过滤 Infinity，只统计有限样本并新增正负 Infinity 计数 | 有限子集统计简单。 | Mean/Median 不再描述完整样本；需要新公共字段或类型，并偏离已批准 Spec。 | 高；未经需求支持扩大 API。 | 不采用。 |

方案 A 的统计实现遵循以下结构：数组只物化和排序一次；由排序后端点以常数时间分类 Infinity 并取得有限值缩放因子，再分别用一次线性遍历计算缩放域中的均值与样本方差。它不比现有 `Average` 加 `Sum` 增加线性遍历次数，也不新增数组之外的托管分配。

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| Objective 结果域 | Core / `ContinuousProblem` 与 `Evaluation` | 不变 | 两个入口分别保护策略返回值和公开值对象构造。 |
| Constraint 单项与汇总域 | Core / `ContinuousProblem` 与 `ConstraintEvaluation` | 不变 | Core 的比较协议依赖非负且有序的违背量。 |
| Infinity 排序 | Core / `EvaluationComparer` | 不变 | 现有 `double.CompareTo` 已满足规则，只补契约测试与注释。 |
| Infinity 目标阈值 | Core / `StoppingConditions` | 不变 | 属于单次运行的停止策略。 |
| 跨 run 描述统计 | Experiments / `ExperimentStatisticsCalculator` | 不变 | 统计不是 Core 运行生命周期职责。 |
| 长期数值契约 | ENGINEERING + 新 ADR | 新 ADR 明确决策，ENGINEERING 保留摘要 | Spec 定义功能行为，ADR 解释跨功能数值选择。 |

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Objective 策略返回值 | `ContinuousProblem.CreateEvaluation` | 每次评估一次标量分类 | 是 | 只对 `NaN` 抛 `InvalidOperationException`；Infinity 通过。 |
| 直接构造的目标值 | `Evaluation` 构造器 | 每次显式构造一次 | 可能 | 只对 `NaN` 抛 `ArgumentOutOfRangeException`。 |
| Constraint 策略返回值 | `ContinuousProblem.CreateEvaluation` | 每项约束一次标量分类 | 是 | `NaN` 或小于零（含 `-Infinity`）抛 `InvalidOperationException`。 |
| 约束累计值 | `ContinuousProblem.CreateEvaluation` | 每项正违背量一次加法 | 是 | 允许自然饱和到 `+Infinity`；不增加溢出分支。 |
| 直接构造的约束汇总 | `ConstraintEvaluation` 构造器 | 每次显式构造一次 | 可能 | 拒绝 `NaN`、负值及不一致的 count/total/max；接受一致的 `+Infinity`。 |
| TargetObjective 阈值 | 工厂方法 | 构造时一次 | 否 | 只对 `NaN` 抛 `ArgumentOutOfRangeException`。 |
| Experiment 成功样本 | `ExperimentStatisticsCalculator` | 聚合时排序和两次线性遍历 | 否 | 按 Spec 返回扩展实数或 `null`，内部结果不得为 `NaN`。 |

不新增候选位置验证，不在 `EvaluationComparer`、算法循环或 Experiment 调度器中重复检查已经由值对象保证的 `NaN` 不变量。

## API 与行为变化

- 新增：无新的公共类型、方法或兼容入口。
- 修改：
  - `Evaluation` 接受正负 Infinity，仍拒绝 `NaN`。
  - `ConstraintEvaluation` 接受一致的 `+Infinity` 汇总，仍拒绝 `NaN`、负值和不一致关系。
  - `ContinuousProblem.Evaluate` 接受 Objective 的 Infinity、Constraint 的 `+Infinity` 和累计溢出的 `+Infinity`。
  - `StoppingConditions.TargetObjective` 接受正负 Infinity，只拒绝 `NaN`。
  - `NumericStatistics.Mean`、`Median`、`StandardDeviation` 从 `double` 改为 `double?`；`Minimum`、`Maximum` 保持 `double`。
- 删除：总违背量有限求和溢出异常，以及所有声称目标值、非负约束值或目标阈值必须有限的契约文本。
- 破坏性变化：三个统计属性变为可空类型；调用方必须处理未定义统计。按批准的 Spec 不保留旧属性、替代名称或包装类型。
- 调用方迁移方式：使用模式匹配或 `is double value` 处理统计值；Example 在 `null` 时输出 `undefined`，不输出空字符串、`NaN` 或默认零。
- 明确保持不变的行为：公开类型名称、Experiment 结果结构、Minimum/Maximum、有限常规样本的数学结果、可行性优先比较、优化方向、Repair/Initializer 职责、随机性、并发与项目依赖。

## 有限统计的数值实现

- Mean：有限样本按最大绝对值缩放到 `[-1, 1]` 后求和并还原尺度，避免原始总和先溢出。全零样本直接返回零。
- Median：排序后奇数样本取中项；偶数有限中项使用不会先执行原始两数相加的中点算法。单侧 Infinity 与有限值的中点为该侧 Infinity；相反 Infinity 的中点为 `null`。
- StandardDeviation：单样本固定为零；至少两个样本且含任意 Infinity 时为 `null`；全有限样本在缩放域计算以 `n - 1` 为分母的样本标准差，再还原尺度，数学结果超出 `double` 时允许 `+Infinity`。
- Duration、Iterations 和 Evaluations 的源值始终有限。它们继续复用同一统计实现；`DurationStatistics` 在内部显式解包已定义结果，若内部有限性不变量被破坏则立即失败，不以零代替。

## 替代与清理计划

- 被取代的行为：`IsFinite` 对整个评估域的一刀切限制、约束累计溢出异常、利用直接 IEEE 算术计算 Infinity 统计。
- 必须删除的兼容壳：无；不得添加旧版非空统计属性、obsolete 转发属性或配置开关。
- 必须删除或改写的旧测试：把“非有限 Objective 一律失败”改成“NaN 失败”；保留其测试替身但按真实语义命名。新增测试不能只替换旧断言而遗漏 Infinity 成功路径。
- 必须更新的示例和文档：`ENGINEERING.md`、架构概览的 Core/Experiment 状态摘要、User Guide 的问题结果规则、Developer Guide 的 Objective/Constraint 契约、Example 的可空统计输出，以及所有受影响 XML 注释。API Reference 只重新生成验证，不手写成员页面。
- 必须新增的决策：ADR-0015 记录有序扩展目标域、非负扩展约束域、统计的 `null` 语义和不保留兼容壳；不修改 ADR-0013 的 Repair 决策。
- 全仓库残留搜索：`rg -n "non-finite|must be finite|finite objective|有限目标值|必须有限|IsFinite\\(objective|IsFinite\\(target|total constraint violation overflowed" src tests examples docs ENGINEERING.md`，逐项区分仍应有限的算法配置和已被替代的评估契约。
- 保留旧结构：`Evaluation`、`ConstraintEvaluation` 和 `NumericStatistics` 都有真实消费者且职责独立，只修改契约；不存在待删除空壳。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 是 | 五个评估、比较和停止类型的数值域或文档变化。 | Core 单元测试与 Release Build。 |
| Algorithms | 行为受益、代码预计不改 | Bat 只消费 `EvaluationComparer`，不自行验证目标值。 | 现有 Bat 测试，加一个 Infinity 目标集成测试仅在 Core 测试不足以覆盖算法初始化时补充。 |
| Experiments | 是 | 统计属性可空，计算器必须显式处理 Infinity。 | ExperimentRunner 端到端统计测试。 |
| Examples | 是 | 可空 Mean 必须显式显示 `undefined`。 | Example Build 与运行烟雾检查。 |
| Tests | 是 | 新增数值分类、排序、停止、聚合和统计组合。 | 需求映射中的测试集合。 |
| Benchmarks | 否，除非遍历模型变化 | 采用方案保持排序加两次线性遍历及一次数组分配，不进入优化热循环。 | 代码审查；实际增加遍历或分配时升级为 BenchmarkDotNet。 |
| XML 文档 | 是 | 参数域、返回值、异常和 nullable 语义必须更新。 | DocFX warnings-as-errors。 |
| 用户/API 文档 | 是 | User/Developer Guide 加规则与扩展责任；Overview 不复制成员契约。 | 文档验证与人工职责走查。 |
| ENGINEERING | 是 | 将“验证结果”具体化为拒绝 NaN、接受有序 Infinity。 | ADR/Spec 链接与文档门禁。 |
| ADR | 是 | 新增 ADR-0015 并更新索引。 | ADR 验证器。 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | `Evaluation` 直接构造 NaN；Objective 返回 NaN | Core 单元 | 两个信任入口分别抛规定异常，候选位置仍不扫描。 |
| FR-002 | 两方向下 finite/±Infinity、相同 Infinity、可行与不可行组合 | Core 比较单元；必要时 Bat 集成 | `EvaluationComparer` 严格顺序与等价性符合 Spec。 |
| FR-003 | Constraint 返回 NaN、负有限、`-Infinity`、零、正有限、`+Infinity`；直接构造对应组合 | Core 单元 | 无效域失败，非负扩展域成功，count 一致。 |
| FR-004 | finite + `+Infinity`、多个 `+Infinity`、两个 `double.MaxValue` 溢出、非法 total/max 关系 | Core 聚合单元 | total/max/count 确定且不抛旧溢出异常。 |
| FR-005 | TargetObjective 的 NaN/±Infinity 与两方向/可行性；统计的有限、单侧 Infinity、双侧 Infinity、偶数相反中项、单样本、极端有限样本；Example 编译 | Core 停止单元 + Experiment 端到端 | 不产生 `NaN`；nullable 字段、Infinity 和停止原因完全符合表格。 |
| NFR-001 | 审查 Core 只做标量分类；统计仍为一次物化、一次排序、两次线性遍历且不新增集合分配 | 代码审查；条件触发时基准 | 热路径无扫描/分配回归；未触发则 Verification 记录不需要基准的证据。 |

## 实施顺序与提交边界

1. 新增 ADR-0015，更新 ENGINEERING 和 Spec 相关 ADR 链接；不改代码。
2. 先添加 Core 失败测试，再修改 Objective/Constraint/TargetObjective 数值边界和 XML 注释。
3. 添加 Experiment 统计失败测试，再修改 nullable API 与显式特殊值计算；同步迁移现有测试。
4. 更新 Example、User Guide、Developer Guide 和架构概览，生成并验证 API Reference。
5. 执行残留搜索和完整 Verification；只有全部需求有证据后才推进 Spec 状态。

Tasks 在 Plan 批准后按上述边界展开，每项同时提交行为、测试与必要注释，避免先改完整实现再补追踪。

## 风险和回退

- 最大实现风险：Infinity 组合的统计规则实现正确，但极端有限值的中间算术仍产生 `NaN`；其次是 nullable 迁移漏掉 Example 或测试以外的消费者。
- 可能产生的行为漂移：缩放统计的末位舍入可能与 LINQ 的直接求和不同；这属于为避免溢出所需的算法变化，但普通样本的数学结果和现有精确整数测试必须保持。
- 如何及早发现：先用参数化测试覆盖分类笛卡尔积和极端有限值，再改实现；编译器负责发现仓库内 nullable API 消费者；残留搜索发现陈旧有限性文本。
- 回退方式：在 Plan 实施提交内恢复旧代码与文档；不引入兼容开关或双语义并存。已 Accepted 的新 ADR 若实现放弃，则以新 ADR 明确 Rejected/Superseded 状态，不静默回写历史。
- 退回 Spec 澄清的条件：需要为 undefined 统计新增 `null` 以外的公共表示、需要改变 Minimum/Maximum、发现某个成功 run 不允许 Infinity，或无法在不新增热路径扫描的情况下维持 Core 不变量。

## ADR 判断

- 是否触发 ADR：是。
- 判断依据：目标值与约束结果的数值域是跨算法、停止、实验和文档的长期公共契约，且会替代当前工程规则。
- 新 ADR：`ADR-0015: 评估结果使用有序扩展数值域`。
- 替代关系：不再次替代已 Superseded 的 ADR-0012；ADR-0015 明确覆盖其遗留的有限性做法，并声明 ADR-0013 的 Repair 特殊值语义不受影响。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-08-27
