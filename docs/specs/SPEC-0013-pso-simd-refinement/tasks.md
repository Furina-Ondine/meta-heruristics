# SPEC-0013 实施任务

## 执行规则

- 来源：[Approved Spec](./spec.md)、[Approved Plan](./plan.md)及[共同验证附件](../simd-plan.md)，于 2026-09-15 按用户授权拆分任务。
- 全部任务已于 2026-09-17 完成；实现、失败的独立 B 候选和最终组合验收见 [`verification.md`](./verification.md)。
- 执行时一个 package 同时只允许一项任务为 `InProgress`。发现新行为、冲突、重复概念或需要改变批准布局时，退回 Spec/Plan。
- 每项实现同时补齐对应测试；候选是否进入生产由已批准的数值和性能门槛决定。失败候选可记录评估完成，但不能标为优化成功。
- 下列命令是已执行的验证入口；基准 filter、环境开关、实际 case 数和结果记录于 Verification。

## T001：锁定增量基线与独立参考

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 依赖：无
- 影响区域：PsoOptimizerTests、VectorOpsTests、TensorPrimitivesPsoTests、PsoBenchmarks、[验证报告](./verification.md)。
- 实施内容：按共同附件锁定 H/A 源提交和构建适配；A 使用当前已有 SIMD、Clamp/Add 的生产实现，并将独立 Span.CopyTo 改动统一纳入各候选。保存同两系数的独立标量公式、当前生产组合参考和回调事件参考；检索 ComputePsoVelocity 全部消费者。建立 B-init、B-coeff、二者组合以及 C 的分离对照。
- 明确不做：不以旧纯标量速度公式作为 A，不引入生产切换配置，不把两系数改为逐维独立样本。
- 完成条件：基线、源 hash、参考来源、预计 case 清单可追溯；旧生产参考的差分与事件测试通过。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`；基准枚举入口与精确 filter 在此任务记录，确认实际数量。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T002：实现并验证初始化批量采样候选 B-init

- 状态：`Completed`
- 覆盖需求：`FR-002`、`NFR-001`
- 依赖：`T001`
- 影响区域：PsoOptimizer.ResetForRun、初始化测试、ResetForRun 基准。
- 实施内容：每粒子按 Initializer→Repair→速度 Fill→Evaluate 顺序执行；非相等范围直接有界 Fill 到既有 Velocity，相等范围常量填充且不消费随机源。以实际 L 验证 ceil(D/L) 批量状态轮数、尾部和后续两类随机状态。
- 明确不做：不新增初始化缓冲，不跨粒子预取初始化样本，不移动策略调用，不用单值采样补尾。
- 完成条件：范围、相等端点、短维度和尾部测试通过；策略分别不采样、单值采样、批量采样时角色及后续状态符合新布局；新增持久载荷为 0。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T003：实现并验证整代系数候选 B-coeff

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 依赖：`T002`
- 影响区域：PsoOptimizer.EnsureWorkspace/Advance/GenerateCandidate、采样与回调测试。
- 实施内容：建立 Optimizer 私有 double[2P]；每代在既有初始化检查、惯性计算之后且首个候选之前一次 Fill。按 [cognitive0,social0,...] 消费，每粒子一对跨维度共享；零系数仍消费。保留候选生成/Repair、personal best 复制、全体 Evaluate、随后 best 更新的原顺序。每代先完整写入 2P 再读取，跨 run 复用。
- 明确不做：不拆成两个 P 长 Fill、不跨代缓存、不新增清零或额外重置、不提前计算位置或 best 快照、不移动取消检查。
- 完成条件：Plan 的种群大小矩阵及 2P 跨 L 边界测试通过；交错角色、ceil(2P/L) 推进、两类状态隔离、每粒子共享系数成立；首次 16P 字节载荷和稳态无新增调用级分配可核实。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T004：验证直接 TensorPrimitives 与私有融合候选

- 状态：`Completed`
- 覆盖需求：`FR-001`、`NFR-001`
- 依赖：`T003`
- 影响区域：PsoOptimizer 算术调用、VectorOps.simd.cs、双输出内核测试与基准。
- 实施内容：先审查直接 TensorPrimitives C1 的适用性并验证；没有适用操作或未通过既定门槛时评估 C2 速度→Clamp→位置双输出级联。保留括号与乘加次序，使用相同随机输入对照独立标量及当前生产组合。局部候选按多次读入、完整速度公式、Clamp、位置计算和最终输出写回的连续数据流计为一次操作，不拆分计时。验证 512/256/128 完整块和手写标量尾部；Clamp 端点、NaN/Infinity、零符号及双工作区源数据保护须单独断言。
- 明确不做：不显式 FMA，不新增公开 Span/重叠契约，不凭 Min/Max 替换推定 Clamp 特殊值一致，不重写惯性或 best 更新。
- 完成条件：共同附件全部适用的维度、切片、哨兵和数值预算通过；只测试生产调用方允许的别名情形，不以额外别名支持扩大契约；C1 选择或转入 C2 的理由留证。
- 验证命令：`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`；`dotnet test --project tests/Metaheuristics.Simd.Generators.Tests/Metaheuristics.Simd.Generators.Tests.csproj -c Release`；局部门槛测量按 T001 确定的完整基准命令执行。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T005：生命周期、回调与实际执行路径回归

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`
- 依赖：`T004`
- 影响区域：优化器/Experiment 集成测试、生成器与硬件路径证据。
- 实施内容：覆盖固定 seed 重复、同 Optimizer 顺序 run 复用、独立实例并发、不同 Group 划分/调度；分别让策略使用两类随机状态，验证先写后读、回调和 best 更新事件。覆盖首个/中间粒子 Repair 及随后 Evaluate 的取消/异常 fixture，保持传播、评估计数和异常后不可复用规则。独立进程记录实际 Core L 和 Algorithms 各宽度支持。
- 明确不做：不要求与旧轨迹相同，不检查缓冲旧内容已清除，不把 Core 宽度开关视为 Algorithms 路径证据。
- 完成条件：实际可运行的硬件及软件路径正确性通过；不支持路径明确标为未实测；零系数、相等范围、衰减惯性和边界测试均有证据。
- 验证命令：Release 测试命令同 T004；独立进程的具体开关核实后逐条记录于 Verification。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T006：增量性能、分配及候选取舍

- 状态：`Completed`
- 覆盖需求：`NFR-001`
- 依赖：`T005`
- 影响区域：PsoBenchmarks、基准专用参考、Verification。
- 实施内容：B-init 测 ResetForRun，B-coeff 测整代采样/缓冲读写/候选消费并报告每代与每粒子成本；二者各自对 A 再组合。C 与相同采样布局 B 对照。按共同附件执行 D=32/128 主负载及全部诊断负载、首次与复用 Optimizer 的完整 Runner、JIT 与分配记录，提供 H/A、A/B、B/C、A/C、H/C 耗时比。
- 明确不做：不将 16P 缓冲写入成本排除，不用整体收益掩盖无收益的融合，不把固定迭代耗时称作收敛时间。
- 完成条件：各保留 B、C 满足其局部 ≥1.10×、主要完整 run ≥1.02× 及诊断回退限制，最终 C/A 同样达标；受限 Vector128 路径按共同附件相对同公式标量参考 ≥1.00× 单列判定。置信区间不足以支持结论时记录不确定。首次载荷、数组数量、稳态分配与工作量全部记录；各候选有保留/删除/不确定结论，不能仅填平均值。
- 验证命令：T001 建立的 BenchmarkDotNet 命令；默认 2 launches、8 warmups、20 measured iterations、MemoryDiagnoser，完整 filter、实际 case 数、环境与产物写入 Verification。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。

## T007：生产收敛、清理与最终验证

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 依赖：`T006`
- 影响区域：已选生产路径、消费者测试、XML/开发者文档、架构概览、Verification。
- 实施内容：接入通过门槛的候选；成功融合后删除无生产消费者的旧 ComputePsoVelocity，历史对照留在基准专用参考。失败候选内核、无消费者缓冲和 helper 删除；仍承担有效参考职责的测试保留。同步实际路径及必要轨迹兼容说明，填写逐需求证据和残留检索。
- 明确不做：不为基准保留无生产消费者的 runtime 内核，不改 Options 示例或公共签名，不把未通过候选标为优化成功。
- 完成条件：Release restore/build/test、生成器测试、文档自测/验证、DocFX 与 diff 检查通过；结果支持后才更新完成状态；新冲突退回上游。
- 验证命令：`dotnet restore Metaheuristics.NET.slnx`；`dotnet build Metaheuristics.NET.slnx -c Release --no-restore`；T004 的两项测试；`pwsh -NoProfile -File ./eng/test-documentation-verifier.ps1`；`pwsh -NoProfile -File ./eng/verify-documentation.ps1`；`dotnet tool run docfx docfx.json`；`git diff --check`。
- 验证结果：完成；实现、定向测试、正式基准及最终工程验证见 [`verification.md`](./verification.md)。
