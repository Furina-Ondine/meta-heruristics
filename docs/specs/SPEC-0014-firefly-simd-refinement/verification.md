# SPEC-0014 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 共同验证附件：[`../simd-plan.md`](../simd-plan.md)
- 验证日期：—
- 当前阶段：执行前；实现、测试和基准均尚未开始
- 最终结果：`Pending`

本报告是执行前的证据表。下列每一项结果均为 `Pending`，不得把计划中的场景、命令或预期门槛写成已通过结论。

## 需求覆盖

| 需求 | 计划实现位置 | 待测场景与证据 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `src/Metaheuristics.Algorithms/Firefly/FireflyOptimizer.cs`；必要的 Algorithms 私有算术路径 | 原 attractor 顺序与 source.Evaluation 资格；距离使用前次 Repair 后的当前 target；每次移动立即 Repair；短维度、尾部、特殊值、in-place；零 random step/attractiveness 仍完成移动 | `spec.md`、`plan.md`、本报告 | Pending |
| FR-002 | Firefly 私有 `_randomWalk[D]` 与当前 run `RandomSource` | 每个合格实际移动一次 `Fill(D)`，消费 `ceil(D/L)` 轮批量状态且不消费标量状态；D 个样本先写后读；无合格时不采样；跨顺序 run 复用且不清零/额外重置；回调、Evaluate、取消、异常和 Group 隔离不变 | `spec.md`、`plan.md`、本报告 | Pending |
| NFR-001 | `benchmarks/Metaheuristics.Benchmarks/FireflyBenchmarks.cs`；`verification.md` 性能表 | A（当前生产 SIMD）与 B（每次移动 Fill(D)）比较采样成本；B/C 算术使用相同样本及固定吸引事件；局部内核与固定工作量完整 run；报告分配、移动数量、硬件、Runtime、源 hash 和置信区间 | `plan.md`、`../simd-plan.md`、本报告 | Pending |

## 具体待测场景

### FR-001：逐吸引者移动语义

- 以两个以上严格更优 attractor 构造事件记录，确认遍历顺序不变；相等 Evaluation 和较差 Evaluation 不采样、不计算、不移动、不 Repair。
- 让 Repair 确定性地改变 target.Position，检查下一 attractor 的 `DistanceSquared` 和 attractiveness 读取该修复后的位置；不得使用原始 source.Position，也不得提前计算下一 attractor 的距离或吸引力。
- 对 `randomStep = 0` 和 attractiveness 为零的合格移动，确认仍读取该移动的 D 个样本、执行原公式并调用 Repair；全体 Evaluate 仍在候选生成完成后进行。
- 覆盖共同附件的维度集合 `1、2、7、8、15、16、24、25、31、32、33、64、65、127、128、129、1024`，并检查标量尾部、空/非对齐诊断切片、精确 in-place 和特殊值分类。

结果：`Pending`；上述场景尚未执行。

### FR-002：采样、状态和生命周期

- 对每个合格实际移动记录一次 `Fill(D)`，以最终 SPEC-0010 的实际 `L` 核对其批量状态消费为 `ceil(D/L)` 轮；确认该路径不推进标量随机状态。
- 检查 `_randomWalk[D]` 在同一 Optimizer 的顺序 run 间保持物理复用，不清零或额外重置；每次移动先写满 D 个样本再读取，仅读取本次有效范围，容量余量旧值不参与结果。
- 检查无合格 attractor 时没有 Fill、移动或 Repair；Initializer、Repair 和全体 Evaluate 的调用时机/计数保持；取消和异常遵循原有传播、检查和 Optimizer 生命周期，不新增清理分支。
- 用固定 seed 重复、同一 Optimizer 顺序复用、独立实例并发及不同 RunGroup 拆分/调度检查新版本内部一致性。允许相对旧版本的轨迹变化；同时观察只消费单值的策略与批量 Fill 的后续状态关系。

结果：`Pending`；上述场景尚未执行。

## 性能报告（仅性能类修改）

所有结果、置信区间和分配数据均待执行后填写。加速比统一为 `基线耗时 / 修改后耗时`。

| 层级 | 被测场景 | 基线与修改后配置 | 输入/工作量 | 结果 |
| --- | --- | --- | --- | --- |
| 改动部分 | Firefly 单次移动内核 | A：现有生产 SIMD 与原采样；B：每次移动 `Fill(D)`、原缩放和原位置算术；C：B 采样加优先直接 TensorPrimitives、必要时私有融合 | B/C 算术使用相同预生成样本及固定吸引事件，A/B 采样对照计入真实采样成本；`D=2、7、8、15、16、24、25、31、32、33、64、65、127、128、129`；同一硬件和 Runtime | Pending—未运行 |
| 代表性整体任务 | OptimizationRunner 固定工作量完整 run | A/B/C 使用相同 Problem、Options、Initializer、Repair、seed 和迭代预算 | Sphere、Clamp(-5,5)、种群 64、seed `20260905`、`MaxIterations(10/100)`；`D=32/128`；新建首次 run与同一 Optimizer 顺序复用 run分别记录 | Pending—未运行 |

待记录项目：B/A 局部至少 `1.10×`、完整 run 至少 `1.02×`；C/B 算术至少 `1.10×`、完整 run 至少 `1.02×`；最终保留方案相对 A 的完整 run 至少 `1.02×`；主要点及诊断点按共同附件检查不超过 5% 回退。稳态调用无新增托管分配，新增持久工作区按字节数、数组数量和首次分配单独报告。另报告 H/A、A/B、B/C、A/C、H/C，锁定历史与当前源提交并统一纳入独立 CopyTo 改动；新增持久载荷预期为 0，实际分配待测。需要记录 `Vector<double>.Count`、`Vector.IsHardwareAccelerated`、Vector128/256/512 支持、SDK/Runtime、OS/CPU、BenchmarkDotNet 配置（2 launches、8 warmups、20 measured iterations）、完整命令、源 hash、预计/实际 case 数和置信区间。以上均为 Pending。

固定 10/100 代只代表固定工作量执行成本，不作为达到目标精度的完整求解时间；目标精度耗时的目标函数、误差、评估预算和 seed 集合尚未定义，结果为 Pending。

## 删除与残留检查

| 被替代概念 | 预期处理 | 待检查位置/方法 | 结果 |
| --- | --- | --- | --- |
| 生产 Firefly 路径逐维 `NextDouble` 随机采样 | B 路径改为每次实际移动一次 `Fill(D)`；若 B 被采用则历史 A 只留隔离基准；未采用时保留原生产路径 | `FireflyOptimizer.cs`、全仓 `NextDouble`/`FillRandomWalk` 搜索 | Pending |
| `_randomWalk` 的清零、额外 reset、持久游标或有效数量 | 不新增；跨顺序 run 复用，写满本次 D 后只读本次范围，尾部旧值不处理 | `FireflyOptimizer.cs` 工作区初始化与 ResetForRun 路径 | Pending |
| Firefly 跨移动 Bk/K/int 索引分块预取及 `8KD` 额外载荷 | 当前批准布局不采用；实现和文档中不得残留当前实施候选 | SPEC-0014 相关文档、生产代码和测试搜索 | Pending |
| 被拒绝的 TensorPrimitives/融合候选、额外 scratch 或运行时切换 | 未通过数值/性能门槛的代码与配置删除；保留有实际消费者的回退/参考 | `FireflyOptimizer.cs`、私有算术路径、基准 | Pending |
| 旧随机顺序测试或声称旧 seed 轨迹必须一致的断言 | 改为 B 的 Fill(D)、批量状态消费、Repair 后 target 与新版本重复性断言 | `FireflyOptimizerTests.cs` 及测试搜索 | Pending |

## 架构一致性

- Core 继续拥有 run 级随机状态和 Fill 语义，Algorithms 只负责 Firefly 的采样调用、缩放和移动：Pending。
- 吸引资格、顺序、Repair 和 Evaluate 的职责及回调时机保持在现有层级，不新增重复验证：Pending。
- `_randomWalk[D]` 只作为 Firefly 私有实际消费者；不存在无消费者公共缓冲、公共 SIMD 后端或运行时切换兼容层：Pending。
- 距离归约、标量尾部、特殊值和 in-place 约束仍由批准的私有算法路径承担，未扩展 Core 的批量评估或公共种群布局：Pending。
- 任何新增入口、参数或算术候选均能由 Approved Spec/Plan 和 ADR-0019、ADR-0025 追踪；发现未批准选择时停止并退回上游产物：Pending。

## 工程验证

- Restore：Pending—尚未执行
- Release Build：Pending—尚未执行
- Tests：Pending—尚未执行
- 生成器测试：Pending—尚未执行
- Format 与 `git diff --check`：Pending—尚未执行
- 文档链接、规格覆盖与文档验证器自测：Pending—尚未执行
- DocFX：Pending—尚未执行
- BenchmarkDotNet 与分配分析：Pending—尚未执行

## 未解决问题

- 当前没有新增的规格问题；实现、测试、残留搜索、性能门槛和工程检查均尚未执行，因此不能将 SPEC-0014 标记为 `Implemented`。结果：`Pending`。
