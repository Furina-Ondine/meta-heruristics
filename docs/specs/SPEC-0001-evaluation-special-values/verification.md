# SPEC-0001 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-08-27
- 最终结果：`Passed`

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `Evaluation`、`ContinuousProblem` | `EvaluationSpecialValueTests.EvaluationRejectsNaNAndAcceptsInfinities`、`ProblemAcceptsInfiniteObjectivesButRejectsNaN` | ADR-0015、Objective XML、User/Developer Guide | Passed |
| FR-002 | `EvaluationComparer` 沿用 `double.CompareTo` 的已验证顺序 | `InfinityOrderingFollowsOptimizationDirection`、`EqualInfinitiesCompareAsEquivalent` 及既有可行性测试 | ADR-0015、Comparer XML、User Guide | Passed |
| FR-003 | `ContinuousProblem`、`ConstraintEvaluation` | `ProblemRejectsInvalidConstraintValues`、`ConstraintEvaluationRejectsUnorderedOrNegativeValues`、`ConstraintEvaluationAcceptsConsistentPositiveInfinity` | ADR-0015、Constraint XML、User/Developer Guide | Passed |
| FR-004 | `ContinuousProblem` 允许累计饱和，`ConstraintEvaluation` 接受一致 Infinity | `ProblemAggregatesUnboundedAndOverflowedViolations` | ADR-0015、ConstraintEvaluation XML | Passed |
| FR-005 | `StoppingConditions`、`NumericStatistics`、`ExperimentStatisticsCalculator`、Example | TargetObjective 参数化测试；Experiment 的 7 个 Infinity/极端有限统计用例；Example 运行 | ADR-0015、统计/停止 XML、User/Developer Guide | Passed |
| NFR-001 | Core 只做 `IsNaN`/符号标量分类；统计为一次物化、一次排序、两次线性遍历 | 代码审查；Release Build；未触发 Benchmark 条件 | Plan 信任设计、ADR-0015、ENGINEERING | Passed |

## 删除与残留检查

| 被替代概念 | 预期处理 | 残留搜索结果 | 结果 |
| --- | --- | --- | --- |
| Objective/Target 必须有限 | 删除 `IsFinite` 与陈旧异常/XML | 运行时代码无残留；ADR-0015 仅保留历史背景 | Passed |
| Constraint 与总违背量必须有限 | 接受 `+Infinity` 并删除累计溢出异常 | 运行时代码无残留 | Passed |
| 非空 Mean/Median/StandardDeviation | 直接改为 nullable，不保留兼容属性 | 仓库消费者均已迁移；无旧属性或包装入口 | Passed |
| 所有 `finite` 文本 | 只删除评估域旧规则 | Bat 参数与数值范围仍要求有限，具有独立算法不变量 | Passed |

残留命令：

```powershell
rg -n "non-finite|must be finite|finite objective|有限目标值|必须有限|IsFinite\(objective|IsFinite\(target|total constraint violation overflowed" src tests examples docs ENGINEERING.md
```

## 架构一致性

- 策略职责是否保持独立：是。Objective/Constraint 产生标量，Core 定义评估域，Experiments 只聚合成功 run。
- 是否新增重复验证：否。两个公开构造入口各保护自己的信任边界，算法、比较器和调度器不重复验证。
- 是否存在无消费者抽象：否。没有新增公共类型；保留的值对象和统计类型均有真实消费者。
- 职责是否位于批准的项目层：是。Core 与 Experiments 的依赖图没有变化。
- 是否出现未经批准的兼容层：否。三个统计属性直接破坏性改为 nullable。

## 工程验证

- Tool Restore：Passed，DocFX `2.78.5` 已还原。
- Restore：Passed，六个项目已还原。
- Release Build：Passed，零警告、零错误。
- Tests：Passed，78/78。
- Format：Passed，SPEC-0001 修改的全部 C# 文件通过 `dotnet format --verify-no-changes --include ...`。当前 Windows checkout 的既有未修改文件受 `core.autocrlf=true` 影响；CI 在 LF checkout 上继续执行全仓格式门禁。
- 文档链接与规格检查：Passed。
- 文档验证器负向自测：Passed。
- DocFX：Passed，零警告、零错误。
- Example：Passed，单次运行与双 Case Experiment 正常输出。
- Diff：Passed，`git diff --check` 无错误。
- Benchmark 或分配分析：不适用。Core 只替换标量谓词并删除一个分支；统计保持原有物化、排序、遍历与分配阶数，未触发 Approved Plan 的基准条件。

## 未解决问题

- 无。
