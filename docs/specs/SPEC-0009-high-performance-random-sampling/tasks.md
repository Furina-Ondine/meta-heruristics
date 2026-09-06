# SPEC-0009 实施任务

## 执行规则

- 当前 Spec 与 Plan 已批准；批准依据见 Plan。本文只分解已有方案，不增加设计。
- 由用户指定的 `gpt-5.6-luna` / `max` 子代理依依赖顺序执行；T006 复测仍失败时交给 `gpt-5.6-terra` 根据证据重实现，任一时刻仅一个任务为 `InProgress`。
- 发现未批准行为、设计冲突或性能门槛失败时停止相关实施，返回 Spec/Plan 并报告；不得降低验收要求。
- 每项任务同时完成测试与必要文档，记录实际命令、结果和 artifact；保护已有用户修改。

## T001：准备基准与保存生产基线

- 状态：`Completed`
- 覆盖需求：FR-007、NFR-003、NFR-004
- 依赖：无
- 影响区域：benchmarks、临时基线 worktree、验证 artifact
- 实施内容：核对工具链与基线提交；新增 Bat/Cuckoo 32/128 维同配置端到端基准，在 466115ad90015d5476d890dc5fd93048f8439187 的独立临时 worktree 先运行并保存环境、耗时、评估次数、轨迹与分配。固定 population、迭代、目标和 BDN 配置供候选复用。
- 明确不做：不修改基线生产代码，不在不同配置间比较。
- 完成条件：四个主要点具备可重现原始基线；测试/基准配置记录完整。
- 验证命令：dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*BatRandomMigrationBenchmarks*" "*CuckooRandomMigrationBenchmarks*"
- 验证结果：已完成。基线副本 `/private/tmp/spec0009-baseline` 固定在 `466115ad90015d5476d890dc5fd93048f8439187`；以 `Dimension=32/128`、`PopulationSize=64`、`MaxIterations=10`、Sphere + Clamp(-5, 5)、seed `20260905` 运行批准命令。环境为 macOS Sequoia 15.7.4、Apple M4、.NET SDK 10.0.400、Runtime 10.0.11、Arm64 RyuJIT。BDN 原始输出、CSV/HTML/Markdown、基线基准源码和 EveryIteration 轨迹/评估记录保存在 `/private/tmp/spec0009-artifacts/t001/baseline/`；均值为 Bat 32D `319.6 us`、128D `2,526.4 us`，Cuckoo 32D `58.02 us`、128D `166.28 us`。四点评估次数分别为 Bat `704/704`、Cuckoo `244/244`；轨迹记录包含初始化和 10 次迭代。

## T002：实现封闭 RandomSource 与基础采样契约

- 状态：`Completed`
- 覆盖需求：FR-001、FR-002、FR-003、FR-004、NFR-001、NFR-002、NFR-004、NFR-005
- 依赖：T001
- 影响区域：Core/Randomness、Core friend assembly、Tests
- 实施内容：先定义独立作者参考已知答案和 10,000 步差分测试，再实现 internal ulong 构造、唯一四字 Xoshiro 转换与 SplitMix64、八个批准 API、局部状态 Fill、乘高位拒绝整数映射、防溢出半开 double 映射。覆盖边界 seed、全 int 范围、相邻 double、非法参数及空 Span 的状态/目标不变、精确调用序列复现。仅向 Tests/Benchmarks 开放 internal。同步 XML。
- 明确不做：不增加接口、第二 PRNG、公开构造/factory、切换层或标量/Fill 等价承诺。
- 完成条件：全部基础契约测试通过；公共形状和异常与 Plan 一致。
- 验证命令：dotnet test -c Release --filter FullyQualifiedName~RandomSource
- 验证结果：已完成。新增 `Core.Randomness.RandomSource`、仅向 Tests/Benchmarks 开放的 internal friend 构造，以及独立 SplitMix64/xoshiro256++ 参考测试；覆盖 10,000 步逐位差分、边界 seed、全 `int` 范围乘高位拒绝映射、半开 double/整数范围、超宽 double、相邻端点、空 Span、异常前状态/目标不变、精确调用序列与公共封闭 API 形状。验证命令 `dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore -- --filter-class Anastasya.Metaheuristics.Tests.Core.RandomSourceTests` 通过，9/9。

## T003：实现独立标准正态与统计验证

- 状态：`Completed`
- 覆盖需求：FR-005、NFR-001、NFR-002、NFR-003、NFR-005
- 依赖：T002
- 影响区域：Core/Randomness/StandardNormal、Tests、Benchmarks
- 实施内容：实现默认无跨调用 spare Box–Muller Sample 和批次内成对 Fill；覆盖 null、空目标、奇数尾部、复现、有限值和百万固定样本统计。最终实现及独立参考均采用 Plan 预定均值、方差、分位及尾部门槛。若评估 Ziggurat，保留候选证据并在 T006 按两次完整 BDN 门槛决定是否保留。
- 明确不做：不把正态放进 RandomSource，不替换 Cuckoo 私有正态，不改变统计阈值或随机重试。
- 完成条件：标准正态 API/统计测试通过，无全局或跨调用可变状态。
- 验证命令：dotnet test -c Release --filter FullyQualifiedName~StandardNormal
- 验证结果：已完成。新增独立静态 `StandardNormal.Sample/Fill`，采用无跨调用 spare 的 Box–Muller，Fill 在单次调用内成对消费并处理奇数尾部；未引入 Ziggurat 候选或 Cuckoo 替换。测试覆盖 null、空 Span、奇数批量、固定调用序列、暖机后零分配和 1,000,000 固定 seed 样本的均值/方差/中位数/1%/99%分位/`|x|>3` 统计门槛。验证命令 `dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore -- --filter-class Anastasya.Metaheuristics.Tests.Core.StandardNormalTests` 通过，4/4。

## T004：迁移运行生命周期、策略、算法和实验 seed

- 状态：`Completed`
- 覆盖需求：FR-006、FR-007、FR-008、NFR-001、NFR-005
- 依赖：T003
- 影响区域：Core、Algorithms、Experiments、Tests、Examples、既有 Benchmarks
- 实施内容：将 Random/seed 全链一次性迁移为 RandomSource/ulong，显式 seed 列表复制及 unchecked(BaseSeed + (ulong)i) 派生。机械迁移四算法和策略，保持 Cuckoo spare/循环/工作区、RandomReset 消费条件及溢出 Clamp。Bat/PSO 非相等范围改有界 API，测试相邻端点和相等边界不消费。迁移旧数值参考并验证并发 Group、拆分、连续 run 重置、异常重建、取消和评估时点；覆盖 ulong 回绕及显式列表。
- 明确不做：不保留 int/Random 兼容壳，不批量化算法、不放宽 Options、不改 Repair/Evaluate 时点。
- 完成条件：全链构建及算法、Core、Experiments 测试通过，生产旧契约删除。
- 验证命令：dotnet build -c Release；dotnet test -c Release
- 验证结果：已完成。Core Context/Runner/Summary、Initializer/Repair、四种算法、Experiment options/plan/context/results、示例、既有基准和测试全链迁移至 `RandomSource`/`ulong`；Experiment 自动 seed 使用 `unchecked(BaseSeed + (ulong)repetitionIndex)`，显式列表仍先复制；Cuckoo `NextInt(0, maximum)`、私有 spare/请求顺序保持，Bat/PSO 非相等范围改用半开有界 API。新增 Bat/PSO 相邻可表示端点与相等边界不消费测试，以及 Experiment `ulong` 回绕测试。验证命令 `dotnet build src/Metaheuristics.Experiments/Metaheuristics.Experiments.csproj -c Release --no-restore`、`dotnet build src/Metaheuristics.Algorithms/Metaheuristics.Algorithms.csproj -c Release --no-restore`、Examples/Benchmarks Release build 与 `dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore` 均通过；完整测试 160/160。生产 `src` 未残留 `System.Random`、旧 int seed、兼容重载或 Random factory。

## T005：同步文档并验证外部公共 API 使用

- 状态：`Completed`
- 覆盖需求：FR-001、FR-004、FR-006、FR-007、FR-008、NFR-005
- 依赖：T004
- 影响区域：XML、README、User/Developer Guide、API overview、ENGINEERING、架构概览、临时外部验证项目
- 实施内容：同步随机所有权、ulong 迁移、弱序列兼容、Bat/PSO 数值修正及 ADR 链接。Developer Guide 提供仅引用 Core、无 friend 权限的自定义 Optimizer 测试 Initializer/Repair 示例；临时项目编译运行两次独立同 seed run，仅带出结果快照。执行公开 API/项目依赖与残留审查，区分历史和基准合法引用。
- 明确不做：不改历史 ADR 决策正文，不新增运行时项目或公共测试 factory，不泄漏 Context/随机源。
- 完成条件：外部示例编译运行成功，文档/XML/API 与实现一致，禁止残留清零。
- 验证命令：dotnet run -c Release --project <临时外部验证项目>；pwsh -File eng/verify-documentation.ps1
- 验证结果：已完成。README、ENGINEERING、API Overview、User Guide、Developer Guide 和架构概览已同步 `RandomSource` 封闭所有权、`ulong` seed、批量采样入口、外部策略使用方式及 ADR-0020 链接；历史 ADR、Spec 正文和基准对照中的旧 `System.Random` 仅保留为历史/验收语境。临时外部项目 `/private/tmp/spec0009-artifacts/t005/external/ExternalConsumer.csproj` 只引用 `Metaheuristics.Core`，未使用 friend 权限、internal 构造或反射；Release restore/run 成功，并由两个独立 Optimizer 以同一 seed 得到相同 position/objective 快照（`seed=15118284941160267503`、`evaluations=4`、objective `1.361110764275592`）。`/private/tmp/spec0009-pwsh/pwsh -NoProfile -File eng/verify-documentation.ps1` 通过；活动生产代码和用户/开发者文档未残留 `System.Random`、旧 seed 签名或兼容 factory，`RandomPositionInitializer` 等合法类型名与 `Random.Shared` 禁止用语保留。

## T006：完成性能、分配与 JIT 验收

- 状态：`Completed`
- 覆盖需求：NFR-003、NFR-004、FR-005、FR-007
- 依赖：T005
- 影响区域：RandomSourceBenchmarks、StandardNormalBenchmarks、端到端基准、artifact
- 实施内容：执行 Plan 全长度矩阵的标量/Fill/System.Random 对照及独立 Sample；保存 BDN 环境、原始报告、分配和反汇编。主要长度 32/128 的四类均匀 Fill 均须快于基线，所有采样调用分配 0 B；四个端到端点候选时间不得超过 T001 基线 1.05 倍。若选择 Ziggurat，必须两次独立完整执行通过 1.15x、95% 区间和 Sample 1.05x 门槛，否则删除候选并保留 Box–Muller。审查无随机接口/虚/delegate/factory 分派。
- 明确不做：不挑最佳运行、不降低门槛、不仅为消除 helper call 复制转换体、不把单机数据外推。
- 完成条件：逐点报告局部与端到端比率和分配；未达门槛返回上游并阻止标记完成。
- 验证命令：执行 plan.md 中三组批准的 dotnet run BenchmarkDotNet 命令；Ziggurat 被评估时正态组执行两次
- 验证结果：历史 Terra 结果保留为对照；本轮统一实现候选已通过全部 T006 性能门槛，见下方最终复测记录。历史失败候选不再作为最终方案。

### T006 本轮追加执行步骤

- 批准依据：用户要求 Luna 重新测量，仍失败则由 Terra 重新实现；注释使用中文。细节见 Plan 的“T006 复测与条件重实现修订”。
- 顺序：Luna 修正测量并复测 → 核对所有原门槛 → 失败时 Terra 重实现与对照验证 → 通过后 T007。
- 本轮状态：Luna/Terra 的历史复测曾发现两份状态转换；本轮已完成唯一转换修复、正确性复测和正式性能复测，T006 已完成。T007 仍需独立收尾，不在此处宣称 Spec 完成。
- 中文注释：检查本次新增文件和修改涉及的代码/XML 注释，转换为中文。

### T006 最终唯一转换修复与正式复测（2026-09-06）

- 实现：`RandomSource` 只保留一个 `NextRaw(ref, ref, ref, ref)` 状态转换体和一个 `NextBounded(range, ref, ref, ref, ref)` 有界映射体。标量入口使用 `NextRawFromFields`/`NextBoundedFromFields` 局部复制四状态、调用共享 helper 并写回；Fill 直接使用同一 helper 的局部状态循环。wrapper 只负责状态装载/写回，不复制 Xoshiro 或乘高位映射公式。新增内部原因注释及本轮新增 XML 注释均已中文化。
- 正确性：`dotnet build tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-restore -m:1 -nr:false -p:UseSharedCompilation=false` 通过，0 警告/0 错误；`dotnet test tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --no-build --filter-class Anastasya.Metaheuristics.Tests.Core.RandomSourceTests --filter-class Anastasya.Metaheuristics.Tests.Core.StandardNormalTests` 通过，13/13。
- 局部基准：在 macOS Sequoia 15.7.4、Apple M4、.NET SDK 10.0.400、Runtime 10.0.11、BDN 0.15.8、Arm64 RyuJIT、DefaultJob、MemoryDiagnoser 下，以独立临时项目固定 `Length=32/128` 运行全部标量、Fill 和 seeded `System.Random` 对照。最终 wrapper/ref 报告位于 `/private/tmp/spec0009-artifacts/t006-luna-unified/random-source-wrapper-32-128/`，源码副本位于同一 artifact 的 `benchmarks/`。Fill 均值（ulong/unit double/有界 double/有界 int）为 32D `23.82/24.38/27.59/25.57 ns`、128D `92.20/95.58/111.60/100.60 ns`；对应 `System.Random` 标量为 32D `921.24/180.99/183.12/170.71 ns`、128D `3679.44/661.82/671.23/683.00 ns`，八个主要 Fill 点均更快且均为 `0 B`。最终标量均值为 32D `78.41/74.89/79.86/74.92 ns`、128D `314.09/299.99/320.34/303.66 ns`，均为 `0 B`。直接字段 ref 候选的完整对照保存在 `/private/tmp/spec0009-artifacts/t006-luna-unified/random-source-32-128/`，wrapper/ref 在标量和端到端路径更优，因此保留 wrapper/ref。
- 端到端基准：同一 DefaultJob 配置、`PopulationSize=64`、`MaxIterations=10`、Sphere + Clamp(-5,5)、seed `20260905` 的最终报告位于 `/private/tmp/spec0009-artifacts/t006-luna-unified/endpoint-wrapper-32-128/`。Bat 32/128D 为 `304.7/1386.8 us`，相对 T001 基线 `319.6/2526.4 us` 的基线/候选比为 `1.049/1.822`；Cuckoo 为 `56.85/169.14 us`，相对 `58.02/166.28 us` 的比为 `1.021/0.983`。四点均满足候选不超过基线 `1.05x`；分配分别为 Bat `223.74/800.52 KB`、Cuckoo `44/140.75 KB`，与执行模型一致。
- 标准正态：最终报告位于 `/private/tmp/spec0009-artifacts/t006-luna-unified/standard-normal-wrapper-32-128/`；12 点（Sample/Fill、独立 Box–Muller 参考和 Cuckoo spare 参考）均为 `0 B`。生产 Sample 32/128D 为 `11.859/11.867 ns`，Fill 为 `246.751/977.268 ns`；未引入 Ziggurat 候选，因此不触发额外候选准入门槛。
- JIT/结构审查：BDN `DisassemblyDiagnoser` 在当前 macOS 无 Mono 环境不可用，未伪造反汇编；源码确认随机路径无接口、虚调用、delegate 或 factory 分派，且状态转换/有界映射各只有一个实现。历史 JIT 限制记录继续保留。

### T006 成员 `NextRaw()` 与 wrapper/ref 正式比较（2026-09-06）

- 批准依据：项目作者要求把真正成员 `NextRaw()`（读取四个字段到局部变量、唯一转换、写回字段）与 `NextRawFromFields` 调用静态 `NextRaw(ref ...)` 的 wrapper/ref 方案在同一 BDN 配置下公平比较，并提取实际 JIT 输出；不得保留两份生产转换，也不得以短 run 或源码推断替代正式证据。
- 成员候选：`NextULong()`、所有 Fill 和有界路径复用成员 `NextRaw()`；Release 构建通过，`RandomSourceTests` 与 `StandardNormalTests` 共 13/13 通过。成员候选正式报告、代码形状和逐点对比见 [`evidence/t006-luna-member-20260906/README.md`](evidence/t006-luna-member-20260906/README.md)，原始 BDN CSV/Markdown 在其 `random-source-member-32-128` 与 `endpoint-member-32-128` 子目录。
- 成员 RandomSource 24 点均为 `0 B`；Fill ULong/unit double/bounded double/bounded int 的 32/128 均值为 `70.82/301.58`、`73.29/300.69`、`76.45/319.52`、`71.89/291.56 ns`，标量对应为 `74.19/308.60`、`74.01/294.43`、`77.83/320.14`、`73.60/302.49 ns`。所有 Fill 点均快于 seeded `System.Random` 标量对照，既有门槛通过。
- 成员端到端 Bat 32/128 为 `251.1/1344.5 us`，Cuckoo 32/128 为 `57.08/171.01 us`，分配为 `223.74/800.52 KB` 与 `44/140.75 KB`；相对同轮旧基线的四点比为 `0.650/0.528/0.967/1.039`，均不超过 `1.05x`。
- JIT：[`evidence/t006-luna-member-20260906/jit`](evidence/t006-luna-member-20260906/jit/README.md) 保存成员生产输出、恢复后的 wrapper/ref 生产输出和等价诊断体 FullOpts 结果。生产成员标量调用体为 76 bytes，成员 Fill 循环为 104 bytes；生产 wrapper/ref 标量调用体为 76 bytes，Fill 在循环外装载/写回状态、循环体为 112 bytes。两种标量最终字段读写和转换均被内联，没有接口、虚调用、delegate 或 factory 分派。
- 最终选择：成员在标量和 Bat 端到端略快，但 Fill 八个主点比 wrapper/ref 慢约 `2.77x–3.27x`；Cuckoo 两点与 wrapper/ref 基本持平且略慢。综合标量、Fill 和四个端到端点，恢复并保留 wrapper/ref 作为生产中唯一状态转换实现；成员候选的通过与回退原因均如实保留在仓库证据中。
- 状态：`Completed`。T007 仍需独立收尾，不在此处宣称 Spec 完成。

## T007：工程验证、追踪与收尾审查

- 状态：`Completed`
- 覆盖需求：FR-001、FR-002、FR-003、FR-004、FR-005、FR-006、FR-007、FR-008、NFR-001、NFR-002、NFR-003、NFR-004、NFR-005
- 依赖：T006
- 影响区域：verification.md、tasks.md、spec.md、spec 索引、全仓库
- 实施内容：完成 Release restore/build/test、格式与文档检查及 DocFX；逐项填写需求到代码、测试、文档、基准 artifact 的追踪，记录残留与项目图审查。只有全部证据达标才把 Spec/索引标为 Implemented，失败或环境限制如实保留状态。
- 明确不做：不将未执行、失败或被阻塞验证写成通过；不提交或覆盖用户修改。
- 完成条件：所有需求、清理和工程验证均有可核查证据，工作区 diff 审查通过。
- 验证命令：dotnet restore；dotnet build -c Release --no-restore；dotnet test -c Release --no-build；dotnet format --verify-no-changes；pwsh -File eng/verify-documentation.ps1；按 docs/api/overview.md 执行 DocFX
- 验证结果：尚未执行。

## 最终验收（2026-09-06）

T007 已完成：Release 构建、182 项测试、格式、文档规则及 DocFX 严格构建通过。需求追踪、生产残留与工作区审查结果见 verification.md。DocFX 按用户批准的方式跳过私有生成器项目，保留 Roslyn 5.9.0。
