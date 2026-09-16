# SPEC-0014 实施任务

## 执行规则

- 只能实施已批准的 [Spec](./spec.md) 和 [Plan](./plan.md)；共同验证附件 [`../simd-plan.md`](../simd-plan.md) 是 Plan 的组成部分。
- 按依赖顺序执行；一个时间只能有一项任务处于 `InProgress`。每项任务同时完成其覆盖需求所需的测试和文档证据。
- 本文件创建时所有任务均为 `Pending`。未达到批准门槛的候选不得保留为运行时切换，发现新行为或架构选择时退回 Spec/Plan。

## T001：建立移动参考与语义锁定测试

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`
- 依赖：无
- 影响区域：`tests/Metaheuristics.Tests/Algorithms/FireflyOptimizerTests.cs` 及必要的 Firefly 测试辅助代码。
- 实施内容：
  - 建立独立的标量移动参考和事件记录，锁定 source.Evaluation 的吸引资格、原 attractor 顺序、当前 target.Position 的距离输入，以及每次移动后立即 Repair 的顺序。
  - 将随机顺序测试改为当前 B 布局的参考：每个合格的实际移动独立消费一次 `Fill(D)`，验证填满 D 个样本后再读取，`Fill(D)` 消费 `ceil(D/L)` 轮批量状态且不消费标量状态。
  - 覆盖没有合格 attractor 时不采样、不移动、不 Repair；`randomStep = 0` 或 attractiveness 为零时仍采样并执行移动公式和 Repair；相等 Evaluation 不构成吸引。
  - 增加会改变 target 的 Repair 记录器，使下一 attractor 的距离必然读取前一次 Repair 后的位置；记录 Initializer、Repair 和全体 Evaluate 的事件边界。
- 明确不做：不在此任务引入跨移动分块、预先收集 attractor 索引、种群布局变化、公共随机或 SIMD API，也不改变生产实现以迁就旧 seed 轨迹。
- 完成条件：参考模型、事件记录和边界用例能够独立表达批准的逐移动语义与随机状态规则，断言不放宽为“旧轨迹相同”。
- 验证命令：待执行 `dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.FireflyOptimizerTests`
- 验证结果：尚未执行

## T002：实现每次实际移动的 `Fill(D)` 与工作区复用

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`
- 依赖：`T001`
- 影响区域：`src/Metaheuristics.Algorithms/Firefly/FireflyOptimizer.cs` 及其私有移动调用路径。
- 实施内容：
  - 沿 sourcePopulation 原顺序逐个判断 attractor；每个合格 attractor 在该次移动内调用一次 `RandomSource.Fill` 填满已有 `_randomWalk[D]`，读取本次写入的 D 个样本并按原括号计算 random walk，再执行现有距离/位置算术。
  - 距离和 attractiveness 在当前 target（包含上一移动的 Repair 结果）上即时计算；位置更新后立即调用 Repair，保持 Initializer、全体 Evaluate 和选择阶段的原有时机。
  - `_randomWalk[D]` 在顺序 run 间复用，不清零、不额外重置；本次有效 D 个元素先写后读，尾部旧值不读、不清理，不新增持久游标或有效数量字段。
  - 保留无合格 attractor 的零消费路径；即使 random step 或 attractiveness 为零，合格移动仍完成自己的 Fill(D)、公式和 Repair；取消与异常沿用既有生命周期、传播和取消检查。
- 明确不做：不预取后续 attractor 的样本，不提前计算距离/吸引力，不跨 source、RunGroup 或 run 共享随机状态，不引入新的 scratch、公共 API 或运行时实现选择开关。
- 完成条件：T001 的逐移动参考和事件断言在生产 B 路径上通过；同一 Optimizer 顺序 run 保持工作区身份，且稳态实际移动路径无新增托管分配。
- 验证命令：待执行 `dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release --filter-class Anastasya.Metaheuristics.Tests.Algorithms.FireflyOptimizerTests`
- 验证结果：尚未执行

## T003：建立并验证算术候选

- 状态：`Pending`
- 覆盖需求：`FR-001`、`NFR-001`
- 依赖：`T002`
- 影响区域：`src/Metaheuristics.Algorithms/Firefly/FireflyOptimizer.cs`、已有 Algorithms 私有算术路径，以及 `benchmarks/Metaheuristics.Benchmarks/FireflyBenchmarks.cs`。
- 实施内容：
  - 使用同一组已生成的 D 个样本，优先评估直接 `TensorPrimitives` 操作；没有适用直接操作或未通过门槛时，才评估 Plan 授权的私有融合内核。
  - 保留既有 DistanceSquared 归约、源码括号、乘加次序、in-place 别名约束、标量尾部和特殊值分类；C 候选不得提前使用下一 attractor 的位置或跳过 Repair。
  - 先完成同样本数值差分；性能准入与生产取舍留到 T005。C1 无适用操作或未过门槛时才评估 C2，所需局部筛选证据在本任务记录。
- 明确不做：不自写超越函数、不主动改用 FMA 或倒数近似、不增加 ISA 特定公共 API，不把随机采样或移动数量差异计入算术收益。
- 完成条件：C 与同布局 B 在相同样本下通过共同数值预算、特殊值及别名测试；C1 适用性/筛选结论和转入 C2 的理由可追溯。最终局部与端到端门槛在 T005 判定。
- 验证命令：待执行 `dotnet run -c Release --project benchmarks/Metaheuristics.Benchmarks -- --filter "*Firefly*"`；`dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`，先通过数值差分再测候选。
- 验证结果：尚未执行

## T004：验证回调时序、状态隔离和边界

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`
- 依赖：`T003`
- 影响区域：Firefly 算法测试、Experiment/RunGroup 集成测试及必要的测试辅助代码。
- 实施内容：
  - 覆盖维度 `1、2、7、8、15、16、24、25、31、32、33、64、65、127、128、129、1024`，以及短维度、尾部、非对齐切片、精确 in-place 和特殊数值诊断场景。
  - 验证同一版本同一 seed 的重复结果、同一 Optimizer 的顺序 run 复用、独立实例并发、不同 RunGroup 拆分/调度，以及后续单值和批量状态的消费关系；不要求新轨迹等于旧版本轨迹。
  - 验证 Initializer、Repair、Evaluate 的调用次数和事件顺序，实际移动数量，零系数与无合格 attractor 的消费边界，以及取消/异常时既有传播和 Optimizer 生命周期规则。
- 明确不做：不为跨移动预取、跨用户回调预取或未批准的种群布局建立测试契约，不通过清理缓冲区来制造跨 run 等价性。
- 完成条件：所有批准的功能、数值、随机状态和隔离断言通过；失败项按需求编号记录，未通过前不得进入 `T005` 的最终验收。
- 验证命令：待执行 `dotnet test --project tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj -c Release`
- 验证结果：尚未执行

## T005：完成基准、残留清理和交付验证

- 状态：`Pending`
- 覆盖需求：`FR-001`、`FR-002`、`NFR-001`
- 依赖：`T003`、`T004`
- 影响区域：`benchmarks/Metaheuristics.Benchmarks/FireflyBenchmarks.cs`、相关实现/测试/XML 文档，以及本 Spec 的 `verification.md`。
- 实施内容：
  - 按共同附件锁定 H/A 源提交，A 包含当前生产 SIMD 与独立 CopyTo 改动；B（每次实际移动 `Fill(D)`）与 C 的算术对照使用相同随机样本和固定吸引事件，A/B 的采样对照不要求样本逐值相同；局部使用 `D=2、7、8、15、16、31、32、33、127、128、129`，主要/诊断负载包含 `D=24、25、64、65`，完整 run 使用 `D=32、128`。
  - 对 Sphere、Clamp(-5,5)、种群 64、seed `20260905`、`MaxIterations(10/100)` 分别测新建 Optimizer 首次 run 和同一 Optimizer 顺序复用 run，记录实际迭代、评估、Repair、吸引/移动数、分配、硬件支持、Runtime、源 hash、完整命令和实际 case 数。
  - 搜索并清理生产路径中的旧逐维 `NextDouble` 采样、被拒绝的 Bk/K/索引缓冲或融合候选、无消费者入口和运行时切换；保留仍有独立消费者的标量参考/基线，并更新必要的架构/随机轨迹摘要。
  - 完成 Release restore/build/test、生成器测试、文档验证器自测与验证、DocFX 和 `git diff --check`，将所有证据回填 `verification.md`。
- 明确不做：不把固定 10/100 代耗时称为达到目标精度的求解时间，不从局部微基准外推通用收益，不在未达门槛时宣称优化成功。
- 完成条件：共同附件的 B/A、C/B、最终保留方案相对 A 的门槛以及不超过 5% 诊断回退规则均有可复核结果；报告 H/A、A/B、B/C、A/C、H/C；B 单独无收益仍可按共同附件评估组合。最终只保留满足适用门槛的路径；所有工程检查和残留检查有明确结论，Verification 才可离开 `Pending`。
- 验证命令：待执行 `dotnet restore`；`dotnet build -c Release`；`dotnet test -c Release`；`pwsh -NoProfile -File ./eng/test-documentation-verifier.ps1`；`pwsh -NoProfile -File ./eng/verify-documentation.ps1`；`dotnet test --project tests/Metaheuristics.Simd.Generators.Tests/Metaheuristics.Simd.Generators.Tests.csproj -c Release`；`dotnet tool run docfx docfx.json`；`git diff --check`；基准命令见 T003，最终完整命令与 case 数记录于 Verification。
- 验证结果：尚未执行
