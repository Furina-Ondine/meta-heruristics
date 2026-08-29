# SPEC-0005 实施任务

## 执行规则

- 只能实施 Approved Spec 和 Approved Plan 中已有的行为。
- Spec 与 Plan 已获批准；任务按依赖顺序执行。

## T001：建立 PSO 基线与 TensorPrimitives 定向测试

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-003`、`NFR-002`、`NFR-003`
- 依赖：无
- 影响区域：Algorithms 项目文件、Algorithms Tests、Algorithms Benchmarks。
- 实施内容：添加 Algorithms 的直接 Tensors 依赖；建立 PSO 标量基准和端到端基准；增加计划中实际使用的 TensorPrimitives 与 Span 重叠定向测试。
- 明确不做：不修改 PSO 生产热路径；不开始 Firefly 迁移。
- 完成条件：基准可运行，定向测试定义了拟用操作的别名与特殊值边界。
- 验证命令：Release restore、build、相关测试和 BenchmarkDotNet。
- 验证结果：已建立 PSO 标量/直接 TensorPrimitives/VectorOps 内核与端到端基准，并由 `TensorPrimitivesPsoTests` 定义原位目标与特殊值边界；详见 `verification.md`。

## T002：迁移 PSO 候选更新

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-003`、`NFR-001`、`NFR-002`、`NFR-003`
- 依赖：T001
- 影响区域：Algorithms 的 PSO 实现与对应测试。
- 实施内容：优先以 TensorPrimitives 直接调用或无分配组合替换标量速度/位置更新；保留随机调用、Repair 时点和工作区复用；只有明确的 API/测量缺口才创建 Algorithms 私有 `VectorOps`。
- 明确不做：不引入 intrinsics、公开抽象、运行时后端切换或跨平台数值承诺。
- 完成条件：PSO 契约/复现/分配测试通过，生产路径无每次 `Advance` 新托管分配。
- 验证命令：Release build、Algorithms Tests、分配检查和 PSO 基准。
- 验证结果：`PsoOptimizer` 保持随机调用和 Repair 时点，以 `TensorPrimitives.Clamp`/`Add` 加实际调用的内部 `VectorOps.ComputePsoVelocity` 更新；定向及既有契约测试通过。

## T003：评审 PSO 性能门槛

- 状态：`Completed`
- 覆盖需求：`FR-003`
- 依赖：T002
- 影响区域：PSO 基准报告与 Verification。
- 实施内容：在 32、128 维分别运行内核/近内核和端到端 `Advance` 比较，记录标量基线、修改后耗时与分配。
- 明确不做：不以静态调用计数、单一维度或单一微基准代替四项门槛。
- 完成条件：四项均优于标量基线且无新增分配，或删除候选路径并记录未通过原因。
- 验证命令：BenchmarkDotNet Release 运行。
- 验证结果：PSO 的 32、128 维内核和端到端四项比较均快于同机标量基线，且无新增分配；详见 `verification.md`。

## T004：首次 Firefly 候选（历史退出）

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`NFR-001`、`NFR-002`、`NFR-003`
- 依赖：T003 的全部 PSO 门槛通过
- 影响区域：Algorithms 的 Firefly 实现、Tests、Benchmarks。
- 实施内容：复用差向量/随机步长工作区，以直接归约或 `Subtract` + `Dot` 实现距离，并按既有吸引者顺序完成向量更新和每次 Repair。
- 明确不做：不在未通过 PSO 门槛时修改 Firefly；不批量化吸引者或重排随机调用。
- 完成条件：Firefly 的语义/分配测试和四项性能门槛均通过；否则删除候选路径。
- 验证命令：Release build、Algorithms Tests、分配检查和 Firefly BenchmarkDotNet。
- 验证结果：首次尝试已在 PSO 门槛通过后测量直接 TensorPrimitives 与融合向量候选；32 维端到端均未快于标量，故按当时的退出条件删除候选生产路径。结果保留在 `verification.md`，本任务不代表本次重新尝试。

## T005：记录 Bat/Cuckoo 诊断并完成验证

- 状态：`Completed`
- 覆盖需求：`FR-004`、全部 NFR
- 依赖：T003；若 T004 执行则依赖 T004；T006
- 影响区域：Verification、基准记录、文档门禁。
- 实施内容：记录 Bat/Cuckoo 热点及不迁移理由；汇总依赖、正确性、分配、性能和文档验证证据。
- 明确不做：不修改 Bat/Cuckoo 生产实现。
- 完成条件：`verification.md` 填满可复现证据，所有任务终态明确，文档门禁通过。
- 验证命令：Release restore/build/test、文档验证、差异检查。
- 验证结果：已完成 Bat/Cuckoo 审查；PSO 与 Firefly 的固定宽度级联均已完成构建、测试、内核/端到端基准、文档门禁和差异检查，最终证据汇总于 `verification.md`。

## T006：PSO 运行时长度回退与诊断输入

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-003`、`NFR-001`、`NFR-002`、`NFR-003`
- 依赖：T002
- 影响区域：Algorithms 私有 `VectorOps`、Algorithms Tests、PSO Benchmarks、Plan 与 Verification。
- 实施内容：使用 `Vector512<double>`、`Vector256<double>`、`Vector128<double>` 逐级处理当前剩余长度，最后标量收尾；新增 15 元素的定向公式测试；把 15、16 等长度和各固定宽度硬件配置加入 PSO 内核诊断。
- 明确不做：不公开后端选择、不使用 ISA 专属 API、不修改 PSO 随机/Repair/Evaluate 时点、不运行未经项目作者审阅的基准。
- 完成条件：项目作者审阅具体修改后，先运行已确认的构建/测试，再逐项确认 BenchmarkDotNet 命令并记录各长度、512/256/128 位向量可用性和结果。
- 验证命令：`dotnet test Metaheuristics.NET.slnx -c Release --no-restore`；候选更新和端到端的两个 MediumRun BenchmarkDotNet 命令。
- 验证结果：T006 阶段当时 121/121 测试通过；AMD Ryzen 7 9800X3D 的 V512/V256/V128（8/4/2 double lane）上，候选更新 33/33 项、端到端 4/4 项均完成。32、128 维的内核和端到端均快于标量，且没有新增分配；最终累计测试为 144/144，详见 `verification.md`。

## T007：按通用 VectorOps 边界重新尝试 Firefly

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`NFR-001`、`NFR-002`、`NFR-003`
- 依赖：T003、T006
- 影响区域：Algorithms 的 Firefly 实现、Algorithms 私有 `VectorOps`、Tests、Benchmarks、Verification。
- 实施内容：保留 Firefly 的严格 attractor 顺序、随机调用顺序和每次移动后的 Repair；生产候选使用 `VectorOps` 的 512→256→128→标量级联，基准夹具同时比较标量、TensorPrimitives `MultiplyAdd` 组合和 VectorOps 级联。
- 明确不做：不增加公开 SIMD 后端、运行时路径开关、ISA 专属 API、批量化 attractor 或 Bat/Cuckoo 生产迁移。
- 完成条件：Firefly 两个主要维度的内核与端到端比较均满足性能门槛且无新增 `Advance` 分配；否则删除生产候选并保留失败证据。
- 验证命令：先执行 Release build/test；经项目作者确认后执行 Firefly 内核和完整 `Advance` 的 BenchmarkDotNet 命令。
- 验证结果：Release 构建和 144/144 测试通过；Firefly 内核 33/33、完整 `Advance` 6/6 完成。VectorOps 在 32、128 维的内核和端到端均快于标量基线，且没有新增 `Advance` 分配，因此保留 VectorOps 生产路径；详见 `verification.md`。
