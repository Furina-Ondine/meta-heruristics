# SPEC-0011 技术计划：Bat 批量采样与分支 SIMD 候选评估

## 元数据

- 状态：`Draft`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`0a74f08592c89b0c09f5e242bb3c111c7f967c7b`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 创建日期：2026-09-14
- 修订日期：2026-09-15
- 批准人：—
- 批准日期：—

[共同验证计划](../simd-plan.md)是本 Plan 的组成部分，规定基线 A/B/C、数值通则、性能门槛与测量记录要求；本文件定义算法特有的设计。下列方案均待本 Plan 批准。

2026-09-15 项目作者明确本阶段只做计划、不继续实验。下文给出后续可执行的评估设计，不表示批准或启动实现；不预设 Bat SIMD 会通过验收。生产接入与候选取舍留待后续阶段。

## 当前实现调查

- `src/Metaheuristics.Algorithms/Bat/BatOptimizer.cs` 的 ResetForRun 在每个 Initializer→Repair 后逐维交错初始化 Velocity/Frequency/Loudness/PulseRate，然后 Evaluate。
- GenerateCandidate 逐维生成新频率，计算目标速度并 Clamp，再抽脉冲、条件扰动和响度判定。目标 Frequency/Velocity 即使响度判定失败也已更新；源状态直到整代选择都不应被修改。
- Advance 的阶段为“生成并 Repair 全部候选→Evaluate 全部候选→逐候选选择并更新 best”；不能改成逐候选立即 Evaluate。
- 私有 NextDouble 对相等边界直接返回常量；Core 的有界 Fill 要求严格递增端点，必须在算法调用前保留常量分支。
- 工作区只有两组 BatState 与 best；尚无随机 scratch 或 Bat VectorOps。测试位于 BatOptimizerTests，已有拒绝候选速度、半开/相等边界、复用及隔离覆盖。RandomMigrationBenchmarks 提供完整 run，BatWorkspaceReuseBenchmarks 提供工作区对照。
- 当前算法尚未实现 Approved Spec，这是本 Plan 的实施对象；没有已知需要改写公共 API 的冲突。引用随机源最终行为时采用 SPEC-0010/ADR-0024，不沿用已替代 ADR-0021 的旧状态假设。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：当前逐维抽样及分支 | 已验证基线 | 多次标量采样与循环 | 无新增风险 | 基线 |
| B：候选批量抽样，保留标量分支 | 隔离采样收益、容易差分 | 3D 随机 scratch、额外扰动采样 | 消费布局变化 | 待评估，不默认保留 |
| B-block：跨候选分块预取 | 扩大采样批次 | 4KD scratch 与更多缓冲读写 | 只预取样本，状态仍在原位置计算 | 与单候选布局独立比较 |
| C1：直接 TensorPrimitives | 复用现成适用操作 | 操作覆盖范围有限 | 需核对 NaN/限幅语义 | 有适用直接操作时优先评估 |
| C2：批量抽样与私有掩码融合内核 | 减少逐维控制流 | 未选路径计算、多字段读写与掩码成本 | 不能提前写源状态 | 无适用直接操作或未过门槛时评估 |

## GenerateCandidate 分支的评估设计

分支数量本身不能证明 SIMD 的收益或回退。需区分三类控制流：频率相等边界是整个候选共享的配置判断，可留在向量循环外；pulse/accept 是逐维判断，需要比较标量分支与向量掩码；整代选择属于 GenerateCandidate 之后的阶段，不纳入本内核重排。

后续评估分为三个问题，不把不同来源的收益混在一起：

| 问题 | 对照方式 | 必须计入的成本 | 可得出的结论 |
| --- | --- | --- | --- |
| 批量抽样本身是否值得 | A 原单值交错采样 vs B 批量采样后标量分支 | Fill、scratch 写入/读取、未选扰动分支的额外采样 | 是否保留批量化 |
| 掩码算术是否值得 | B/C 使用同一批预生成样本，分别执行标量分支与向量掩码 | 速度限幅、两条位置路径、六个目标状态字段的实际写回 | 仅说明算术内核收益 |
| 组合后是否改善完整 run | A/B/C 的完整 Runner 调用 | 创建、ResetForRun、采样、算术、Repair、Evaluate 和选择 | 是否接入生产 |

主维度为 32、128，诊断加入 1、2、7、8、15、16、31、33、127、129、1024。分支场景至少包括：全部接受且走扰动、全部接受且走速度、全部拒绝、两类判断均混合，以及运行前后 loudness/pulse 衰减形成的分布。混合场景使用固定 seed 的多候选输入集，记录输入规模，避免只循环同一小组掩码。相等频率与速度范围另测。

算术对照先验证同样本下的 Position、Velocity、Frequency、Loudness、PulseRate、InitialPulseRate 全部结果。pulse 的严格大于和 accept 的严格小于、拒绝时保留哪些字段、NaN/Infinity/有符号零与尾部都必须保留。不能只测位置更新或只统计分支次数。

Math.Exp 的代际因子是否移出逐维分支是另一项优化因素。算术隔离对照须让 B/C 使用相同的因子计算位置；若评估因子外提，则额外与未外提版本对照，避免将其收益记在 SIMD 名下。等长 Span.CopyTo 改动也应同时出现在全部对照版本中。

只有预采样内核更快时，仍不能判定 GenerateCandidate 或完整求解更快。若掩码路径未通过门槛，可仅保留通过门槛的批量标量路径；批量化也无收益时保留原实现。通过哪个候选再决定是否新增生产 scratch 或 VectorOps 内核，不按方案表预先落地。

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 随机状态与范围映射 | Core | Core | 直接消费现有 Fill |
| 角色布局、常量边界跳过 | Bat | Bat | 算法特有消费语义 |
| 候选公式 | Bat 逐维循环 | Bat/Algorithms 私有 VectorOps | 只分离无状态算术 |
| 缓冲及阶段顺序 | BatOptimizer | BatOptimizer | Group 独占与顺序复用 |

## 候选布局与数据流

以下是 B/C 的单候选布局，尚未决定接入生产。设维度 D、Core 批量宽度 L。候选使用一个 `double[3D]` scratch，固定划分为 pulse[D]、perturbation[D]、accept[D]，载荷 24D 字节，每 Optimizer 一份。当前 target.Frequency 直接作为频率输出，不另建频率缓存。若最终保留该方案，工作区只在 EnsureWorkspace 首次分配；每次使用先完全覆盖，run 间不依赖旧值。

每个 GenerateCandidate 在当前位置、源状态及本代 best 可用后，按以下顺序采样，随后才执行算术与 Repair：

1. 频率区间相等则 target.Frequency.Fill(constant)，不采样；否则 RandomSource.Fill(target.Frequency, lower, upper)。
2. RandomSource.Fill(pulse)。
3. RandomSource.Fill(perturbation, -1, 1)，包括未选中扰动分支的维度。
4. RandomSource.Fill(accept)。

后三步各自为一次公开 Fill，不合并为一次 3D Fill；非相等频率候选共消费 `4*ceil(D/L)` 轮批量状态，相等频率为 `3*ceil(D/L)`，不消费标量状态。扰动半开区间映射复用 Core，不手写近似范围公式。不会因 loudness=0、pulse=1 或分支预测而跳过本 Plan 固定的后三次采样。

算术严格保持：新频率→`sourceVelocity + ((best-sourcePosition)*frequency)`→Clamp；`pulse > sourcePulse` 决定 best 扰动或 source+clampedVelocity；`accept < sourceLoudness` 决定新位置、衰减 loudness 和 pulse。拒绝分支仍写目标新 Frequency/Velocity，同时复制源 Position/Loudness/PulseRate；InitialPulseRate 始终复制源。Math.Exp 的代际因子为候选内共享标量，保持原表达式和迭代编号，不采用向量 Exp。生成所有目标状态后才 Repair。

ResetForRun 作为独立可删除候选：每个 bat 的 Initializer→Repair 后，依次对 Velocity、Frequency、Loudness、PulseRate 做有界 Fill；相等区间只填常量；PulseRate 复制至 InitialPulseRate；然后 Evaluate。每个非相等区间消费 ceil(D/L) 轮，不跨下一个 bat 的 Initializer 预取。独立测量该初始化改动，收益不通过时不与候选融合捆绑保留。

跨候选 B-block 比较 K=1、4、8，K 是待 Plan 审阅和后续证据决定的内部参数，不新增公共配置。每块为本代接下来的 m<=K 个候选，使用 `double[4KD]`，载荷 32KD 字节；四个角色区各容量 KD，活跃区各 mD，按候选顺序切 D。按 frequency、pulse、perturbation、accept 顺序对活跃区调用 Fill，相等频率区间改为 Span.Fill(constant)，不抽样。非相等频率共消费 `4*ceil(mD/L)` 轮批量状态，相等频率为 `3*ceil(mD/L)`；不消费标量状态。

样本可先于前一候选 Repair 生成，但每个候选仍在原调用位置读取源状态、计算并写入目标全部字段，然后立即 Repair；全部候选生成后才 Evaluate 和选择。块不跨代，最后一块只填 m 个候选，不为剩余空槽抽样。取消/异常丢弃未使用样本，不推迟原取消检查；run 间仅复用存储。测试需覆盖策略消费两套随机状态与分块后的后续值。B/C 算术对照固定同一布局和 K，单候选 24D 与分块 32KD 载荷分别报告。上面的初始化候选暂选每 bat 一组，这是具体布局选择，不是 Spec 禁止跨 Initializer 预取。

私有 NextDouble 的常量语义保留，但判断移到整批调用外：相等端点直接填常量，否则直接调用 Core 有界 Fill；当初始化和候选路径都不再使用 helper 时删除它。单位样本加融合缩放仅可作为后续数值方案调查，不能直接用 `lower+(upper-lower)*u` 取代 Core 已有的半开区间、溢出和上界修正规则，也不在 Algorithms 复制通用范围映射。本稿以现有有界 Fill 为候选设计，不预设缩放融合收益。

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| Options/范围 | 原构造验证 | 每 Optimizer 一次 | 否 | 原异常与相等边界 |
| scratch/目标等长与隔离 | EnsureWorkspace/测试 | 首次与测试 | 不逐块验证 | 不污染 source 或邻接内存 |
| 分支与字段写入 | 同样本独立标量参考 | 全诊断矩阵 | 仅测试 | `>`/`<`、新频率、拒绝状态 |
| 批量状态与回调 | 记录策略和独立消费参考 | 每测试 run | 仅测试 | 显式预取布局、实际回调顺序、两套随机状态 |

## API 与行为变化

- 拟新增：仅最终通过门槛的 Bat 私有随机工作区及算术 helper。
- 修改：本候选/初始化的随机消费布局，固定 seed 旧轨迹允许变化；重复性条件包含有效向量宽度。
- 删除：被保留候选取代的逐维抽样；不删除必要的标量算术尾部。
- 公共签名、Options、Initializer/Repair/Evaluate 次序、整代选择、异常/取消契约保持；用户无组装方式迁移。

## 替代与清理计划

只把有实际生产消费者的内核加入 VectorOps.simd.cs；不扩展生成器 DSL、类型范围或 Core 引用。成功替代后删除仅用于旧 Bat 路径的私有 helper；若初始化仍使用 NextDouble，则保留其真实用途。Scalar 参考归测试/基准，不保留生产实验开关。

改写 BatOptimizerTests 中假定逐维单值消费的预期，保留半开范围、相等边界不消费以及拒绝候选不污染 source 的断言。检索 `NextDouble|GenerateCandidate|Frequency|InitialPulseRate` 的生产与测试引用，清理失败候选和临时 benchmark 分支。实现完成后同步架构摘要与轨迹兼容说明。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 否 | 仅使用现有 Fill | 原随机源测试 |
| Algorithms | 是 | Bat、共享私有模板新增 Bat 内核 | 差分与生成器测试 |
| Experiments | 实现否，验证是 | Group 调度不变 | Group 拆分隔离 |
| Examples | 签名否 | 旧 seed 数值不可作为兼容快照 | 示例编译 |
| Tests | 是 | Bat 抽样/阶段/状态测试 | Release tests |
| Benchmarks | 是 | 新候选内核及完整 run A/B/C | 共同验证计划 |
| XML 文档 | 是 | 必要的轨迹/环境说明 | 编译与 DocFX |
| 用户/API 文档 | 是 | 兼容摘要，无新 API | 文档验证 |
| ENGINEERING | 否 | 沿用现有规则 | 审查 |
| ADR | 否 | ADR-0025 覆盖算法融合 | 边界核查 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | 新频率、速度限幅、pulse/accept 等于阈值和两侧、拒绝候选、相等四类范围 | 内核与优化器 | 全字段差分，不只 Position |
| FR-002 | 单候选/分块四角色布局与尾轮、未选分支仍抽扰动、两类策略随机消费、固定 seed/Group | 优化器与 Experiment | 后续样本和事件符合对应布局 |
| FR-003 | 全体生成/Repair→全体 Evaluate→选择、复用、多维度拒绝/取消 | 优化器 | 事件表及 24D/32KD scratch 分配 |
| NFR-001 | B、C1、C2 的候选内核与完整 run；初始化单列 | BenchmarkDotNet | 共同门槛逐点通过或删除 |

数值遵循共同准则，额外测试 pulse=0/1、loudness=0/1/>1、iteration=0、衰减饱和、零宽速度和频率。主要性能负载使用默认参数；分支全选/全拒/混合以及相等边界作为诊断。

## 风险和回退

掩码选择容易遗漏拒绝路径字段；先锁定全字段参考。向量 Clamp 若不能保持特殊值与有符号零语义，使用保持标量判断的比较/选择，或拒绝该候选。内核收益不能抵消额外 scratch 带来的整体回退。显式跨候选样本预取已获 Spec 授权；改变实际阶段顺序、随机角色相关结构或增加 Core 能力时仍须退回 Spec。

## ADR 判断

不新增 ADR：延续 ADR-0019 与 ADR-0025 的 Algorithms 私有 double 级联及生成边界。Core 的 Vector<T>/ISA 旋转例外不扩展到算法内核。

## 批准记录

- 计划批准：—
- 批准日期：—
- 采样边界确认（2026-09-15）：项目作者批准显式跨回调预取与常量区间不采样；具体 K、性能门槛与整份 Plan 仍待批准。
