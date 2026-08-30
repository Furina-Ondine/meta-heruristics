# SPEC-0006 实施任务

## 执行规则

- 只能实施 Approved Spec 和 Approved Plan 中已有的行为。
- 发现遗漏、冲突、重复概念或新架构选择时停止，退回 Spec/Plan。
- 完成一项需求时同时完成对应测试和必要文档。
- 不得因为现有公共 API 而保留 Plan 已要求删除的抽象。
- 一个时间只能有一项任务处于 `InProgress`。
- 每次 BenchmarkDotNet 测量前必须展示待测代码和完整命令，获得项目作者反馈后再运行。

## T001：记录手写 SIMD 基线

- 状态：`Completed`
- 覆盖需求：`NFR-001`、`NFR-003`
- 依赖：无
- 影响区域：Benchmarks、BenchmarkDotNet artifacts、Verification
- 实施内容：在提交 `ed2de0d3` 的当前手写生产路径上记录 Reflect、PSO、Firefly 的局部与端到端耗时、分配、环境和 JIT 反汇编；保存完整命令与结果位置，作为生成后同配置比较基线。
- 明确不做：不修改生产 SIMD、基准参数、公式或准入门槛；不使用 SPEC-0004/0005 的历史数字代替本次同配置基线。
- 完成条件：五项生产路径的基线报告和 JIT artifacts 完整，硬件宽度与分配已记录；任何失败或零 benchmark 运行均明确报告而不视为证据。
- 验证命令：完整命令见 [`verification.md`](./verification.md#完整命令)。
- 验证结果：五项 timing 与三项 Dry disassembly 均退出 0，执行数量分别为 Reflect 40/40、PSO 11/11、Firefly 11/11；环境、分配、统计值与 Reflect 短迭代限制已记录。

## T002：建立受限增量生成器和诊断

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-004`、`FR-005`、`NFR-004`
- 依赖：`T001`
- 影响区域：eng、生成器测试项目、解决方案、集中包版本
- 实施内容：新增 `IIncrementalGenerator`、受限 AdditionalFiles 模板解析/语法节点替换、稳定 hint name、相对行映射和明确 diagnostics；新增 512/256/128 展开、0/1/2/7/8/15/16、确定性、增量、冲突和无效能力测试。
- 明确不做：不接入 Core/Algorithms 生产代码，不解析任意 C# 宏，不访问网络/环境状态，不生成运行时策略类型。
- 完成条件：生成器与测试项目 Release build 通过；相同输入生成文本逐字节一致；虚拟 `double`/`float`/`int` Count 展开和不支持能力负例符合 Plan。
- 验证命令：`dotnet test tests/Metaheuristics.Simd.Generators.Tests/Metaheuristics.Simd.Generators.Tests.csproj -c Release`
- 验证结果：Release 测试 23/23 通过，覆盖 `double`/`float`/`int`、512→256→128 顺序、生成的三层 `IsHardwareAccelerated` 门、0/1/2/7/8/15/16 尾部、方法级三宽度展开、确定性、真实增量缓存、稳定/不冲突 hint name、重复目标和无效模板/能力诊断；完整解决方案 Release build 为 0 warning、0 error。

## T003：迁移 Algorithms 私有 VectorOps

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`FR-006`、`NFR-001`、`NFR-002`
- 依赖：`T002`
- 影响区域：Algorithms、VectorOps 模板、Algorithms 项目引用、算法测试
- 实施内容：先迁移 `UpdateFireflyPosition` 并审查生成源、IL/JIT 和行为；通过后迁移 `ComputePsoVelocity` 与 `DistanceSquared`。生成完整内部方法，保持公式、归约顺序、Span 重叠、特殊值和标量尾部。
- 明确不做：不改变 Optimizer、随机工作区、Repair/Evaluate 时机、TensorPrimitives 路径或算法公开 API；不保留旧手写生产方法体。
- 完成条件：Algorithms 只保留模板权威和生成 SIMD 路径；VectorOps/PSO/Firefly 全部定向测试通过；API 与运行时依赖无变化；无新增调用/分派/分配。
- 验证命令：定向 `VectorOpsTests`、`PsoOptimizerTests`、`FireflyOptimizerTests`，随后 Release build 和完整测试。
- 验证结果：模板已替代三个原手写方法，硬件门也由生成器统一产生；既有行为测试 144/144 通过。PSO 与 Firefly Dry disassembly 分别执行 11/11，code size 保持 2004 B 与 2753 B；归一化地址后与 T001 手写基线反汇编逐字节相同，无新增调用、控制流或分配。

## T004：迁移 Core 私有 Reflect SIMD

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-006`、`NFR-001`、`NFR-002`
- 依赖：`T003`
- 影响区域：Core CandidateRepairs、Reflect 模板、Core 项目引用、Repair 测试
- 实施内容：生成 `Repair` 外层级联和仅因位宽不同而重复的 load、Reflect block 与 mask 机械展开；保留手写边界数据、标量 `Reflect`、`RepairLargeOffsetLanes` 和创建时派生。
- 明确不做：不改变 Clamp/RandomReset/DoNothing、边界形状、ULP 契约、危险 lane 判定、公共 Repair API 或新增 Core `VectorOps`。
- 完成条件：四种边界形状、特殊/危险 lane 和全部指定尾部长度通过；生成路径无新增分配/运行时依赖；旧三宽度机械副本删除。
- 验证命令：定向 CandidateRepair 差分/特殊值/尾部测试，随后 Release build 和完整测试。
- 验证结果：Core 模板生成外层硬件门、Repair 块和三宽度 helper，旧机械副本已删除；完整 Release 测试 167/167 通过。Reflect Dry disassembly 执行 40/40，代表 code size 保持 3938/4390/4406 B；两侧均为 550 个方法实例、28 种签名，归一化地址和反汇编方法序号后结构差异为 0。

## T005：执行生成后零开销与性能门槛

- 状态：`InProgress`
- 覆盖需求：`NFR-001`、`NFR-003`
- 依赖：`T004`
- 影响区域：Benchmarks、JIT artifacts、Verification
- 实施内容：先展示生成后的待测代码和与 T001 完全相同的命令；获项目作者反馈后运行 Reflect、PSO、Firefly 局部/端到端 BenchmarkDotNet 与 JIT 反汇编，对比调用、控制流、边界检查、分配和统计结果。
- 明确不做：不放宽门槛、不用不同配置历史数据替代、不保留失败候选或新旧运行时开关。
- 完成条件：每项生成路径 JIT 结构等价、无额外分派/分配且主要基准无可确认回退；失败项按 Plan 删除生成接入并恢复唯一手写生产路径。
- 验证命令：与 T001 相同，执行前再次获得项目作者反馈。
- 验证结果：尚未执行

## T006：清理、架构同步与完整验证

- 状态：`Pending`
- 覆盖需求：`FR-002`、`FR-005`、`FR-006`、`NFR-002`、`NFR-004`
- 依赖：`T005`
- 影响区域：全仓库、架构概览、Spec package、ADR index
- 实施内容：删除旧展开、迁移壳和无消费者生成能力；更新架构概览与 Verification；审查生成器不进入发布包、无公开 API 漂移、无 ISA 专属或运行时 kernel 抽象。
- 明确不做：不提交生成 `.cs`、Benchmark artifacts、兼容壳或未获准的 `float`/`int`/对齐生产路径。
- 完成条件：restore、Release build、全部测试、格式、DocFX、文档门禁、API/引用/残留检查和 `git diff --check` 通过；Verification 覆盖全部 FR/NFR 并记录局部/端到端比率。
- 验证命令：按 ENGINEERING 执行完整工程验证，并记录确切命令与结果。
- 验证结果：尚未执行
