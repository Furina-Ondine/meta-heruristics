# SPEC-0012 实施任务

按已批准的 [`spec.md`](./spec.md) 与 [`plan.md`](./plan.md) 实施。全部任务已于 2026-09-17 完成；采用、否决与验收证据见 [`verification.md`](./verification.md)。

- 若发现未批准的行为、抽样布局、数值前提或公共 API 变化，暂停当前任务并退回 Spec/Plan。
- B 的单候选批量采样、I-map 和 C2/C3 算术候选分别保留独立对照；不能用一个候选的收益掩盖另一个候选的回退。
- C1 先检查直接 TensorPrimitives 的适用性；C2/C3 只有在其前置条件、数值验证和性能门槛满足时才可进入生产实现。C3 不是通用 Pow 替代。
- 候选取舍沿用共同附件：B 单独无收益时仍可评估组合，不宣称 B 单独达标；强制正态职责迁移若与门槛冲突，退回 Spec。
- 工作区按 Plan 跨 run 复用有效区间，不清零、不增加 run 级游标或有效数量；每次读取前完整写入本次有效区间。

## T001：锁定生产基线与独立参考

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 依赖：无
- 影响区域：Algorithms/Cuckoo、Tests、Benchmarks、验证基线
- 实施内容：锁定历史 H 源提交与包含 SPEC-0010 随机源完成态及既有数组复制优化的 A 基线源 hash、SDK/runtime、Options、Initializer、Repair、Objective、seed 和固定迭代工作量。建立不复用生产实现的标量参考，覆盖正态角色切片、单位样本、Lévy 分母与位置公式、遗弃公式、原相等索引重抽及 I-map；为 C2/C3 准备同一批输入样本和独立的特殊值分类参考。
- 明确不做：不把参考实现、历史私有 Gaussian 或基准切换开关放进生产 Optimizer；不以旧 seed 轨迹逐值相等作为迁移条件。
- 完成条件：基线可从记录的提交重建；参考明确记录运算顺序、epsilon、乘后除、0.8/0.2 加法、索引分布和特殊值分类；B/C 所有算术对照可使用相同样本。
- 验证命令：`dotnet build Metaheuristics.NET.slnx -c Release`；基线与候选的批准 BenchmarkDotNet 命令按 [`plan.md`](./plan.md) 执行并将完整命令写入 [`verification.md`](./verification.md)。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T002：迁移正态职责并建立单候选 3D 工作区

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-003`、`NFR-001`
- 依赖：T001
- 影响区域：`src/Metaheuristics.Algorithms/Cuckoo/CuckooOptimizer.cs`、算法测试、工作区分配
- 实施内容：将每个 Optimizer 的候选采样工作区收敛为一块 `double[3D]`，按 `normalNumerator[D]`、`normalDenominator[D]`、`unit[D]` 切片。每个 Lévy 候选一次 `StandardNormal.Fill` 写入前 `2D`，一次单位 `Fill` 写入后 `D`；每个遗弃候选按 Plan 写入单位区间。第一次使用时分配，后续 run 复用；每次读取前覆盖本次有效区间，不清零、不保留跨候选样本、不创建索引缓冲。
- 实施内容（续）：删除 Cuckoo 私有 `NextGaussian`、`_hasSpareGaussian`、`_spareGaussian` 及其 run 重置逻辑；保留 Core `StandardNormal` 的职责和标量/批量状态路由。不能把 `Fill(Span<int>, int, int)` 恢复到 Core 或算法层。
- 明确不做：不拆分单次 `2D` 正态 Fill，不跨候选缓存正态/单位样本，不预取索引，不引入公共随机或 SIMD API，不清理数组余量。
- 完成条件：奇数维、向量尾部、空目标和连续 run 均只读取当前 Fill 已写入的角色区间；稳态候选不产生调用级托管分配；源码残留检索找不到私有 Gaussian/spare 实现或整数 Fill 依赖。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.CuckooOptimizerTests`；残留检索和分配检查结果写入 [`verification.md`](./verification.md)。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T003：实现 Lévy 候选的逐候选采样与生命周期

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`
- 依赖：T002
- 影响区域：Cuckoo 候选生成、Repair/Evaluate 调用顺序、best 更新
- 实施内容：按 candidateIndex 生成对应 Lévy 候选。每个候选完成正态/单位采样后，读取当时的 source 与 best，按 `Abs(normal)+1e-10`、分子缩放、乘后除和 `source + 0.8*step + 0.2*guidance` 的原顺序计算；随后立即执行 `Repair`、`Evaluate`、条件替换并更新 best，再开始下一个候选。全部 Lévy 候选完成后才调用一次 `FindWorstIndices`。
- 明确不做：不缓存下一候选的 population/best 快照，不把后续候选改成同一代静态 best，不改变候选数量、候选到 population 的对应关系或取消检查。
- 完成条件：记录型 fixture 能证明后一个 Lévy 候选读取前一个候选替换后的 population/best；每个候选的 Repair/Evaluate/替换/更新顺序和计数与 Plan 一致；位置特殊值分类保持参考结果。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.CuckooOptimizerTests`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T004：实现并独立评估逐候选不同索引对映射

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`NFR-001`
- 依赖：T003
- 影响区域：遗弃候选索引选择、随机消费测试、算法基准
- 实施内容：保留原相等索引重抽作为 A/I-map 对照，并独立评估 `P>1` 的映射：`first=NextInt(0,P)`，`second=NextInt(0,P-1)`，若 `second>=first` 则 `second++`。P=1 返回 `(0,0)` 且不消费索引；遗弃数量为 0 时不进入遗弃路径。索引选定后实时读取当时的 population，单位扰动按单候选 Fill，Repair/Evaluate/替换/best 顺序不变。
- 明确不做：不做跨候选索引预取，不增加索引数组或整数 Fill，不复制 Core 无偏整数映射，不把 I-map 的随机消费差异计入正态批量化或算术 SIMD 收益。
- 完成条件：对小 P 穷举 `first` 与 `P-1` 个 `second` 输入，证明映射无相等索引且为有序不同对上的双射；原映射与 I-map 的候选差异可在独立输入下比较；后续策略得到的两套随机状态值和回调事件均有记录。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.CuckooOptimizerTests`；I-map 独立基准按 [`plan.md`](./plan.md) 执行。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T005：审查直接 TensorPrimitives 并建立 C2 算术候选

- 状态：`Completed`
- 覆盖需求：`FR-002`、`NFR-001`
- 依赖：T004
- 影响区域：Algorithms 私有算术候选、TensorPrimitives 适用性记录、算术基准
- 实施内容：先检查目标框架中直接 TensorPrimitives 对当前 Lévy/遗弃公式的适用性、数值分类和性能；不适用或未通过门槛时，建立 C2 候选：Lévy 分母保留 `Math.Pow`，其余分子缩放、步长、引导和位置写回使用现有受限 double 512/256/128/标量级联；遗弃差分和扰动另行融合。C2 与 B 使用完全相同的采样、索引和输入。
- 明确不做：不在本任务把 `Math.Pow` 改成 Log/Exp，不使用自写超越函数、不显式 FMA、不改除法为倒数乘法、不增加通用或公共 SIMD helper。
- 完成条件：直接 TensorPrimitives 的结论、C2 的输入/输出契约、标量尾部被写入 Verification；算术参考能逐元素检查有限值、零/Infinity/NaN 分类及位置误差；候选可单独计时且不包含随机布局收益。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.CuckooOptimizerTests`；C2 局部对照按 [`plan.md`](./plan.md) 执行。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T006：评估 C3 受限 Log→乘法→Exp 与数值回退域

- 状态：`Completed`
- 覆盖需求：`FR-002`、`NFR-001`
- 依赖：T005
- 影响区域：Lévy 分母私有算术、VectorOps 级联、数值测试与维护文档
- 实施内容：只针对 Lévy 分母评估 C3。按原顺序构造 `x=Abs(normal)+1e-10`、`p=1/LevyExponent`，在私有 double 512/256/128 级联尝试现有向量 Log、乘法和 Exp，再继续 C2 的后续算术。初始快路径至少要求 x、p 为正有限；按数值结果确定更窄的可验证域，异常 lane 或整块回退 `Math.Pow`，标量尾部使用 `Math.Pow`。合法但使 p 为 Infinity 的极小正 LevyExponent 必须回退，不能拒绝 Options。
- 实施内容（续）：覆盖 x 接近 epsilon、等于/紧邻 1、极大有限值，p 很大但有限和倒数溢出，及零、次正规、正常有限、Infinity、负数、NaN 输入的分类。若 C3 未同时满足误差、分类和性能门槛，删除候选；只有独立通过门槛的 C2 才能作为生产算术方案。
- 明确不做：不把 Log→Exp 当作通用 Pow，不新增 generic Pow helper、公开入口或运行时开关，不通过放宽误差预算保留失败候选。
- 完成条件：逐 lane/块回退谓词和实际快路径范围被固定；C3 与 Math.Pow 的误差预算为有限结果 `1e-14` 绝对加 `1e-12` 相对，特殊值分类一致且不允许一侧有限、一侧溢出；记录数值准入或删除决定；最终采用在 T008 完成性能准入后判定。若采用，在维护的 `VectorOps.simd.cs` 模板源、输入构造/范围判断处及开发者指南 Cuckoo 数值说明中注明适用域、非通用限制、误差预算、回退条件，并链接 FR-002、测试和 Verification。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.CuckooOptimizerTests`；C3 数值矩阵和局部基准按 [`plan.md`](./plan.md) 执行。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T007：完成算法语义、数值和隔离测试

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 依赖：T006
- 影响区域：CuckooOptimizerTests、Core/Algorithms 隔离测试、回调 fixture
- 实施内容：为最终保留候选补充维度 `1、2、7、8、15、16、24、25、31、32、33、64、65、127、128、129、1024`，覆盖奇数/尾部、空 Span、非对齐切片、哨兵、精确 in-place、P=1/2/小 P、遗弃率 0/1、指数 0.5/1/1.5/1.99 及合法端点附近配置。验证正态角色切片、后续策略分别消费标量/批量状态、先写后读、跨 run 复用、取消/异常、独立实例并发与 RunGroup 隔离。
- 实施内容（续）：用同一抽样输入对照标量公式和所选算术候选；记录动态 best、冻结 worst 目标索引、实时 population 读取、评估/Repair 计数及后续随机事件。保持有限差分、零/次正规/Infinity/NaN 分类，检查位置误差预算和 C3 回退域。
- 明确不做：不要求新旧版本固定 seed 逐值相同，不清零工作区来掩盖读取越界；别名只验证生产调用允许的情形，不新增别名支持契约，不把未支持硬件路径伪装成已覆盖。
- 完成条件：所有语义、分类、分配和状态隔离测试通过；失败项按需求阻止进入性能准入，不把候选失败写成通过。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`，并在 128/256/512 可用路径及无硬件路径下分别记录实际覆盖。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T008：执行局部与完整 run 性能准入

- 状态：`Completed`
- 覆盖需求：`NFR-001`、`FR-001`、`FR-002`
- 依赖：T007
- 影响区域：Cuckoo 基准、BenchmarkDotNet artifact、性能 Verification
- 实施内容：按共同验证计划测 A/B/C：单候选正态/单位批量采样、Lévy 与遗弃内核、I-map 独立对照、C2/C3 分母增量及组合；C2/C3 计入完整 scratch、范围判断、回退、尾部和写回成本。报告 H/A、A/B、B/C、A/C、H/C。完整 run 使用 Sphere、Clamp(-5,5)、P=64、seed `20260905`、MaxIterations=10/100，并分别测新建 Optimizer 首次 run 和复用工作区顺序 run。
- 明确不做：不只测最后几条算术、不让随机或 I-map 收益掩盖算术回退、不挑选最快一轮、不把固定迭代耗时称为达到精度的求解时间。
- 完成条件：局部 B/A 与 C/B 达到至少 `1.10×`，主要完整 run 达到至少 `1.02×`，最终候选相对 A 的完整 run 仍达到至少 `1.02×`；受限 Vector128 路径相对同公式标量参考至少 `1.00×`；诊断维度/特殊边界回退不超过确认的 5%；稳态候选无新增调用级分配。记录 Mean、置信区间、分配、Vector/ISA 支持、case 数、命令和源 hash。未达门槛的候选删除或退回 Spec/Plan。
- 验证命令：按 [`plan.md`](./plan.md) 中批准的 BenchmarkDotNet 配置与命令执行，并将完整结果写入 [`verification.md`](./verification.md)。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T009：文档、残留与工程收尾验证

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`
- 依赖：T008
- 影响区域：源码 XML/维护注释、Developer Guide、Spec/Plan/Verification、全仓库工程验证
- 实施内容：同步 StandardNormal 所属职责、轨迹变化、单候选 3D 工作区、逐候选 NextInt I-map 及整数 Fill 非范围说明。若 C3 采用，完成 T006 要求的模板源注释、输入判断注释和开发者数值说明；若未采用，只在 Verification 记录候选结论，不宣称已有向量 Pow 能力。搜索并记录私有 Gaussian/spare、错误的 int Fill 依赖、索引预取及未批准公共 API 残留。
- 明确不做：不为未采用候选保留生产切换开关，不修改无关算法或布局，不把历史/基准参考误当成生产依赖。
- 完成条件：需求到代码、测试、文档、基准 artifact 的追踪完整；Release restore/build/test、格式、文档验证器、DocFX 与 `git diff --check` 均通过，或将环境限制和既有缺口单列；只有全部证据满足时才允许更新 Spec/索引为 `Implemented`。
- 验证命令：`dotnet restore`；`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build`；`dotnet format --verify-no-changes --no-restore`；`pwsh -NoProfile -File ./eng/test-documentation-verifier.ps1`；`pwsh -NoProfile -File ./eng/verify-documentation.ps1`；`dotnet test --project tests/Metaheuristics.Simd.Generators.Tests/Metaheuristics.Simd.Generators.Tests.csproj -c Release`；`dotnet tool run docfx docfx.json`；`git diff --check`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。
