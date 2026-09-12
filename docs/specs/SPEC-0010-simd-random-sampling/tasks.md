# SPEC-0010 实施任务

按 [`plan.md`](./plan.md) 实施。任务状态在完成后更新。

## T001：双状态与向量采样实现

- 状态：`Completed`
- 覆盖需求：FR-001、FR-002、FR-003、FR-004、FR-005、FR-006
- 依赖：无
- 实施内容：四个 `ulong` 单值状态 + 四个 `Vector<ulong>` 批量状态；批量 lane i 初始化为 `Jump^(i+1)(S)`；四 ref 向量转换体；`NextULongVector`/`NextDoubleVector`；四个 `Fill` 重载改走批量状态；`StandardNormal.Fill` 走向量采样与向量 Box–Muller。
- 内存与计算措施：状态字段每调用只载入/写回一次；完整轮 `StoreUnsafe` 直写并 4 轮展开；尾块整轮推进后经栈缓冲写入；有界 double 的区间换算在调用外一次完成；正态输入重排只在栈上进行。
- 验证结果：Release 构建 0 警告 0 错误；`Metaheuristics.Tests` 在默认宽度、`DOTNET_MaxVectorTBitWidth=128`、`=512` 与 `DOTNET_EnableHWIntrinsic=0` 四种配置下全部通过。

## T002：独立参考、边界与随机质量

- 状态：`Completed`
- 覆盖需求：FR-001、FR-002、FR-003、FR-004、FR-005、NFR-002
- 依赖：T001
- 实施内容：测试工程内独立的逐 lane 标量参考（含 Jump 播种、排放顺序、乘高位拒绝与半开区间映射）；长度矩阵与后续状态检查；单值/批量隔离；正态尾块与消费轮数；分配检查沿用既有用例。
- 验证结果：`RandomSourceBatchTests` 新增 8 个用例，`RandomSourceTests`/`StandardNormalTests` 更新批量断言后全部通过；`StandardNormal` 固定种子 100 万样本统计门槛沿用并通过。

## T003：三宽度性能测量与 JIT 证据

- 状态：`Completed`
- 覆盖需求：FR-006、NFR-001
- 依赖：T002
- 实施内容：只比较随机源自身；`DOTNET_MaxVectorTBitWidth` 取 128/256/512 三种宽度；A 为 SPEC-0009 副本、R 为标量参考；记录每个宽度的 `Vector<ulong>.Count` 与分配；用反汇编确认批量状态算术与正态向量数学实际执行。
- 验收门槛：见 [`plan.md`](./plan.md) 的“性能口径与门槛”。
- 验证结果：三宽度矩阵与反汇编证据已完成并写入 [`verification.md`](./verification.md)。256/512 位全部达标；128 位的批量原始/单位填充为 0.72×–0.79×，低于 0.90× 门槛，NFR-001 记为 `Failed`，处理方式待项目作者决定。

## T004：文档与最终清理

- 状态：`Completed`
- 覆盖需求：FR-001、FR-002、FR-003、FR-004、FR-005、FR-006、NFR-001、NFR-002
- 依赖：T003
- 实施内容：`verification.md` 用直白语言写结论与读法；证据只保留最终报告需要的 BenchmarkDotNet 报告与反汇编；删除中间脚本、临时项目与原始日志；更新 Spec 状态与 ADR/架构说明。
- 验证结果：`verification.md`、证据索引与三宽度产物已就位；Spec 与索引状态同步为 `Implementing`（因为 NFR-001 未通过，未标记 `Implemented`）；架构概览已同步。
