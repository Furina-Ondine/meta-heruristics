# SPEC-0011 实施任务

按 [`spec.md`](./spec.md)、[`plan.md`](./plan.md) 和[共同验证计划](../simd-plan.md)执行。全部任务已于 2026-09-17 完成；实现、候选取舍和验收证据见 [`verification.md`](./verification.md)。

## 执行规则

- 只能实施 Approved Spec 和 Approved Plan 中已有的行为。
- 一个时间只能有一项任务处于 `InProgress`；候选取舍按共同附件执行，不保留生产双路径开关。B 单独无收益时不单独保留或宣称 B 成功，仍可评估满足最终 A/C 与 B/C 门槛的组合。
- B（批量采样加标量分支）、C1（直接 TensorPrimitives）和 C2（私有掩码融合）必须使用同一批已写入的样本分别对照；不能把采样、`Math.Exp` 外提或分支额外计算的收益归给 SIMD。C1/C2 若通过既定数值门槛及共同附件调整后的性能门槛则接入生产；不得因存在逐维 `if` 而预先拒绝，也不得把一次连续速度更新拆成更小子表达式计时。
- 所有随机 scratch 均由单个 Optimizer 持有，按“本次有效区间先完整写入、再读取”使用；跨 run 复用时不清零、不增加 run 级游标或计数状态。
- 完成条件中的性能结果必须记录于 [`verification.md`](./verification.md)；结果未满足门槛时，任务仍可通过“删除候选并记录原因”收尾，不能把未达标写成通过。

## T001：建立 Bat 基线与独立差分夹具

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 依赖：无
- 影响区域：Bat 测试、基准夹具、验证报告。
- 实施内容：锁定 H 的历史源提交与 A 的当前逐维标量参考，统一纳入独立 Span.CopyTo 改动；建立独立的同样本公式参考和事件记录夹具。参考必须比较 Position、Velocity、Frequency、Loudness、PulseRate、InitialPulseRate 六个目标字段，以及 source 在拒绝候选前后的状态。加入 pulse/accept 两侧、等于阈值、相等频率端点、零宽速度、NaN/Infinity/有符号零和各尾部维度的输入矩阵；记录 Initializer、Repair、Evaluate 的次序和次数，并能分别观察标量与批量随机状态及其后续值。
- 明确不做：不修改 Bat 生产路径；不把静态分支计数或单一微基准当作收益证据；不改变 Core 随机 API。
- 完成条件：独立参考不复用待测公式；B/C 算术对照可以注入同一组 pulse、perturbation、accept 和 frequency 样本；测试夹具覆盖 D=1、2、7、8、15、16、24、25、31、32、33、64、65、127、128、129、1024 的尾部形状与共同验证计划的分支场景。
- 验证命令：`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class Anastasya.Metaheuristics.Tests.Algorithms.BatOptimizerTests`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T002：实现单候选随机布局与 Optimizer 工作区

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`NFR-001`
- 依赖：T001
- 影响区域：`src/Metaheuristics.Algorithms/Bat/BatOptimizer.cs`、Bat 状态工作区及对应测试。
- 实施内容：按 Plan 实现单候选随机布局。对频率相等端点直接填常量且不消费样本，否则将频率写入目标 Frequency；随后依次将 pulse、`[-1,1)` perturbation、accept 写入一份 `double[3D]` scratch 的三个连续区域。每个候选使用四次独立 Fill（相等频率时三次），Core 批量宽度为 L 时分别核对 `4*ceil(D/L)` 或 `3*ceil(D/L)` 轮。ResetForRun 中保留每个 bat 的 Initializer→Repair，再依次填充 Velocity、Frequency、Loudness、PulseRate，初始化不跨 bat 预取。
- 明确不做：不预取跨候选样本；不建立整数 Fill、频率缓存、额外索引、run 级清零或跨 Optimizer 共享缓冲；不手写范围映射替代 Core 有界 Fill。
- 完成条件：scratch 只在 EnsureWorkspace 首次分配，载荷为 24D 字节；每次读取前完整覆盖本次 D 个有效元素且只读取这些元素，取消或异常后的旧内容不被读取；稳态候选调用无新增托管分配；相等边界、尾轮、两个随机状态路由和后续策略随机值的测试结果与布局一致。
- 验证命令：`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class Anastasya.Metaheuristics.Tests.Algorithms.BatOptimizerTests`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T003：接入 B 标量分支并锁定生命周期语义

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`
- 依赖：T002
- 影响区域：Bat 候选生成、Advance 阶段测试、Initializer/Repair/Evaluate 记录测试。
- 实施内容：在已写入的单候选 scratch 上执行 B 的标量分支。按“新频率→目标速度限幅→pulse 严格大于 source PulseRate 选择 best 扰动或 source+新速度→accept 严格小于 source Loudness 决定字段写回”的顺序生成目标；拒绝时仍写目标 Frequency/Velocity 并复制其余源字段，InitialPulseRate 始终复制。保持所有候选生成并 Repair 后，才统一 Evaluate，最后按原顺序选择并更新 best；源状态在选择前不得写入。保留原取消检查、异常边界和必要的 scalar tail。
- 明确不做：不在本任务引入 C1/C2 向量算术；不把逐候选 Evaluate、提前 Repair、改变比较方向或改变 selection 顺序作为优化；不在本任务合并 `Math.Exp` 外提收益。
- 完成条件：独立参考在分支两侧、阈值、尾轮及特殊值下的六字段结果符合共同数值预算，分支选择及被复制字段完全一致；回调事件次序/次数、拒绝候选源状态、固定 seed 同版本重复、顺序复用、不同 RunGroup 划分、并发隔离、取消和异常复用测试通过。新旧固定 seed 轨迹变化仅按批准的样本布局解释。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class Anastasya.Metaheuristics.Tests.Algorithms.BatOptimizerTests`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class "*Bat*"`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T004：隔离评估 `Math.Exp` 代际因子外提

- 状态：`Completed`
- 覆盖需求：`FR-001`、`NFR-001`
- 依赖：T003
- 影响区域：Bat 候选算术、独立算术测试、基准夹具与验证报告。
- 实施内容：在 B 的同一随机样本、同一分支和同一工作量下，单独比较当前逐维计算位置与候选内共享标量因子两种实现。保持原表达式、迭代编号、乘法顺序和字段更新；单独记录因子外提对局部和完整 run 的贡献，之后才为 C1/C2 选定共同的因子位置。
- 明确不做：不使用向量 `Exp`；不把因子外提的耗时变化计入 C1/C2 的 SIMD 收益；不因性能结果改变衰减公式、Options 或特殊值分类。
- 完成条件：两种实现的六字段结果、分支结果及 NaN/Infinity/有符号零分类符合批准数值预算；只有在独立局部、完整 run 和最终 A/C 门槛允许时才保留外提，否则删除外提路径并在 Verification 记录未采用原因。B/C 的后续对照使用同一已选因子位置。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class Anastasya.Metaheuristics.Tests.Algorithms.BatOptimizerTests`；`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Bat*Benchmarks*"`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T005：评估 C1 直接 TensorPrimitives 算术路径

- 状态：`Completed`
- 覆盖需求：`FR-001`、`NFR-001`
- 依赖：T004
- 影响区域：Algorithms 私有候选算术、Bat 测试和基准。
- 实施内容：先检查并实现批准范围内适用的直接 TensorPrimitives 操作，仅替换无状态的批量算术；随机采样、分支语义、Repair、Evaluate、selection、工作区所有权和 scalar tail 仍由 BatOptimizer 管理。用 T001 的同一批样本对照速度限幅、best/source 两条位置路径及六个字段，并覆盖全接受、全拒绝和混合分支，显式计入未选路径的额外计算和掩码/写回成本。
- 明确不做：不复制 Core 范围映射；不引入公开 SIMD 后端、ISA 专属 API、运行时切换、公共批量 Evaluate 或种群布局变化；无适用操作或未过门槛时不保留 C1。
- 完成条件：C1 的特殊值、限幅端点、有符号零、尾部和分支结果在批准容差内；稳态无新增托管分配；若直接路径未通过共同门槛，删除生产候选并保留诊断结论，C2 才可按条件继续。
- 验证命令：`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class "*Bat*"`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T006：按条件评估 C2 私有掩码融合内核

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-003`、`NFR-001`
- 依赖：T005
- 影响区域：Algorithms 私有 `VectorOps`、Bat 候选生成、定向测试和基准。
- 实施内容：仅当 C1 没有适用直接操作或未通过门槛时，评估 512→256→128→scalar tail 的通用固定宽度级联。向量块可计算两条位置路径并使用掩码选择，但必须完整写回目标字段，不能提前修改 source；使用同一 B 样本和已选 `Math.Exp` 因子位置，分别测量全选、全拒、混合和相等边界，包含额外分支计算、目标多字段写回、scratch 读写和尾部成本。
- 明确不做：不把 C2 作为预定生产结论；不使用 ISA 专属 intrinsic、公共 VectorOps、跨 Optimizer 状态、跨候选 scratch 或运行时后端开关；C1 已通过时不额外引入 C2。
- 完成条件：所有有效宽度和标量尾部均通过独立参考的六字段差分、分支和特殊值分类；VectorOps 只保留实际生产消费者；若局部或完整 run 门槛失败，删除 C2 生产路径并记录失败证据。
- 验证命令：`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class "*Bat*"`；在进入性能阶段后按实际保留的基准 filter 运行固定宽度矩阵。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T007：完成 A/B/C 性能门槛与完整 run 验收

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`NFR-001`
- 依赖：T006
- 影响区域：Bat BenchmarkDotNet 基准、性能证据、分配分析与 Verification。
- 实施内容：按共同验证计划比较 A 原逐维采样、B 单候选批量采样标量分支、C 最终算术候选；分别测量 B-init、B 候选采样/消费、C 算术内核和 A/B/C 完整 Runner。完整 run 必须计入新建 Context/RandomSource（含 Jump）、ResetForRun、采样、算术、Repair、Evaluate、selection 和固定迭代工作量，并分别测量新建 Optimizer 首次 run 与同实例顺序复用 run。主要 D=32、128；补充 D=24、25、64、65；诊断覆盖所有 Plan 规定长度、全接受/全拒绝/混合分支、相等频率和短维度。
- 明确不做：不以静态调用数、单一维度、单一最快轮次、只计 Advance 或只计内核替代完整证据；不将 `Math.Exp` 外提、CopyTo 小优化、随机布局或 Repair/Evaluate 数量变化归给 SIMD。
- 完成条件：按共同门槛逐点记录均值、置信区间、实际 case 数、分配、源 hash、环境和完整命令：B 相对 A 局部至少 1.10×且完整 run 至少 1.02×；C 相对 B 局部至少 1.10×且完整 run 至少 1.02×；最终保留版本相对 A 完整 run 至少 1.02×；主要与补充诊断均不超过 5% 回退；稳态候选调用无新增托管分配。未达标的算术候选删除；B 单独无收益时按共同附件评估组合，不单独保留 B 或宣称其成功。已确认失败记录为 Failed，未测项保持 Pending；报告 H/A、A/B、B/C、A/C、H/C。
- 验证命令：`dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Bat*Benchmarks*"`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore --filter-class "*Bat*"`。实际运行前确认 `Program.cs` 使用 `BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args)`，并在 Verification 记录预计及实际 benchmark 数量。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T008：清理未采用候选并完成文档与工程验证

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 依赖：T007
- 影响区域：Algorithms、Bat 测试/基准、私有 VectorOps、Spec/Plan/Tasks/Verification、架构摘要和开发者说明。
- 实施内容：依据 T007 的逐点结果只保留通过门槛且有实际消费者的路径；删除未采用的随机/算术 helper、临时基准分支和无消费者抽象。补齐轨迹兼容边界、随机角色切片、scratch 先写后读和数值回退说明；完成测试、性能、残留搜索和文档证据，并只在所有需求和门槛通过后把 Spec 推进到 `Implemented`。
- 明确不做：不保留生产双算法开关；不修改公共 API、Core 批量评估、种群布局、并行模型或未批准的回调/随机语义；本任务完成前不得把 Verification 标为 Passed。
- 完成条件：Release restore/build/test、生成器测试、文档验证器自测与验证、DocFX、格式检查和 `git diff --check` 均有可复核结果；Verification 的每个 FR/NFR 行均有实现、测试/基准、文档与结论，所有任务终态明确。若性能、数值或架构门槛有一项失败，则保留 Spec 为 `Approved`，记录失败和回退，不标记实现完成。
- 验证命令：`dotnet restore Metaheuristics.NET.slnx`；`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test --solution Metaheuristics.NET.slnx -c Release --no-restore`；`pwsh -NoProfile -File eng/test-documentation-verifier.ps1`；`pwsh -NoProfile -File eng/verify-documentation.ps1`；`dotnet format --verify-no-changes`；`dotnet tool run docfx docfx.json`；`git diff --check`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。
