# SPEC-0006 技术计划

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`351e57d057604b2a9637e9a9ee883e32edd91ceb`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`FR-006`、`NFR-001`、`NFR-002`、`NFR-003`、`NFR-004`
- 批准人：项目作者
- 批准日期：2026-08-30

## 当前实现调查

- 当前相关类型和入口：Core 的 `CandidateRepairs.ReflectCandidateRepair.Repair` 手写 512→256→128→scalar 外层级联，并为 Position、四类边界派生值、Reflect 算术和危险 lane 修补分别手写三个宽度的方法；Algorithms 的 `VectorOps.ComputePsoVelocity`、`DistanceSquared` 和 `UpdateFireflyPosition` 各自手写同一外层级联及块内公式。
- 当前调用链：`PsoOptimizer` 与 `FireflyOptimizer` 直接调用 Algorithms `internal VectorOps`；算法只通过 `OptimizationRunContext.Repair` 进入 Core 的公开 Repair 契约，不接触 Reflect 内核。生成后必须保持该调用链和程序集职责不变。
- 当前职责所属层：Core 拥有 Repair、边界、特殊 Position 和 Reflect 标量参考；Algorithms 拥有 PSO/Firefly 公式、归约、工作区和私有 Span 算术；固定宽度机械展开目前随各消费方手写。
- 已存在的相似或重复概念：四个外层级联共享硬件门、完整块条件、宽度缩窄、索引推进和标量尾部；Reflect 的 load、block kernel 与 mask 处理还存在 512/256/128 三宽度机械重复。各算法的公式、Reflect 数值分支和标量参考不是重复职责。
- 当前测试、示例、文档和基准：`VectorOpsTests` 覆盖公式、特殊值和尾部；CandidateRepair 测试覆盖边界形状、特殊 lane 和长度；PSO、Firefly、Repair BenchmarkDotNet 已记录局部与端到端基线。SPEC-0004/0005 Verification 是语义和性能基线。
- 当前项目与工具链：运行时项目目标为 `net10.0`；仓库由 `global.json` 固定 .NET SDK 10.0.400；当前没有 analyzer/source-generator 项目或 Roslyn 包引用。
- 与 Spec 或 ADR 的已知冲突：SPEC-0004 将 Reflect intrinsic 限定为 Core 私有，SPEC-0005/ADR-0016 将算法级联限定为 Algorithms 私有。生成结果仍分别私有，不改变运行时所有权；跨项目编译期生成策略由 [ADR-0017](../../decisions/0017-repository-private-simd-source-generation.md) 补充 ADR-0016。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：Core 运行时 Helper 加 delegate、接口或静态抽象 kernel | 普通 C#，实现直接。 | 每块需要状态传递；delegate/接口有明确分派，静态抽象是否完全内联依赖 JIT；Core 会承载算法无关但又被 Algorithms 消费的 SIMD 机制。 | 不能证明零开销，并污染 Core 运行时职责。 | 不采用。 |
| B：将共享 `.cs` 以 `Compile Link` 编译进 Core 与 Algorithms | 无新增运行时程序集；简单 Helper 可各自内联。 | C# 缺少跨 `Vector512/256/128` 类型的零成本高阶抽象；要么仍复制块体，要么引入泛型/调用边界。 | 只能共享普通 Helper，不能同时满足完整展开与统一公式模板。 | 不采用。 |
| C：构建前脚本或模板工具生成 `.cs` | 能生成与手写同形代码；模板自由。 | 必须处理增量构建、IDE、并行构建、陈旧文件、清理和跨平台调用；提交生成文件会形成双权威，不提交则 IDE 体验和诊断较弱。 | 构建阶段副作用和陈旧产物容易漂移。 | 不采用。 |
| D：仓库私有 Roslyn 增量源码生成器读取受限 AdditionalFiles 模板 | 编译器原生增量输入；只添加当前编译源；生成输出不进入源码控制；可在消费程序集内直接完全展开；诊断可定位模板。 | 新增构建项目、Roslyn 编译期依赖、受限模板语法、生成器/快照测试和调试路径。 | 若模板变成任意 C# 宏系统会超过收益，必须限制语法和调用者。 | 采用。 |

采用方案 D。源码生成器只进行一项受限变换：在一个完整消费方方法模板中找到唯一的固定宽度模板块，将该块按 512、256、128 位依次内联复制并替换平台无关占位符，然后保留消费方显式写出的标量尾部和返回逻辑。它不重写普通 `Compile` 源文件，不扫描整个 Compilation 寻找约定，不生成运行时策略对象。

## 生成输入与输出设计

### 项目与引用

- 新增 `eng/Metaheuristics.Simd.Generators/Metaheuristics.Simd.Generators.csproj`，目标 `netstandard2.0`，不打包，使用集中锁定且与固定 SDK 兼容的 `Microsoft.CodeAnalysis.CSharp` 编译期依赖，并实现 `IIncrementalGenerator`。
- Core 与 Algorithms 以 `ProjectReference` 的 analyzer 形态引用生成器，设置 `OutputItemType="Analyzer"` 与 `ReferenceOutputAssembly="false"`；该引用不得出现在运行时依赖或发布包中。
- 新增 `tests/Metaheuristics.Simd.Generators.Tests`，目标 `net10.0`，以普通测试引用验证生成器输出、诊断与增量确定性；运行时行为测试继续位于现有 `Metaheuristics.Tests`。
- 将生成器与其测试项目加入 `Metaheuristics.NET.slnx`；在 `Directory.Packages.props` 集中锁定 Roslyn 包版本，不在消费项目重复版本。

### 受限模板

- 每个消费程序集在对应源码附近保存显式 `AdditionalFiles` 模板；首期只有 Algorithms `VectorOps` 与 Core `ReflectCandidateRepair` 两组实际调用者。
- 模板使用 C# 词法与语法以保持公式可读，但不是任意宏：每个文件只能声明批准的命名空间、partial 容器和完整方法；每个生成方法必须包含恰好一个固定宽度模板块。模板块可包含局部变量、完整块循环、加载/存储、向量算术、归约和消费方私有方法调用。
- 模板块只允许以下生成占位符：当前向量泛型类型、对应静态向量 API、位宽后缀、元素计数和硬件可用性。生成器按语法节点替换标识符，不做无边界文本替换，不解释公式、不推导 Span 长度、不改变算术顺序。
- 方法签名、初始化、标量尾部、返回值以及 Reflect/算法特有语义由消费方在模板中显式书写。模板是该方法的唯一手写权威；生成器把完整方法直接加入对应 partial 类型，因此不增加 helper 调用层。
- `CandidateRepairs`、其嵌套 `ReflectCandidateRepair` 与 Algorithms `VectorOps` 改为所需的 `partial` 声明。生成方法保持当前可见性；不新增公共类型或成员。
- 生成结果使用稳定 hint name、`GeneratedCode`/`CompilerGenerated` 标记、nullable 上下文和仓库相对 `#line` 映射。输出默认只在编译器/`obj` 中存在，不提交生成 `.cs`，避免模板与产物形成双权威；测试保存小型 golden 快照而不复制整份生产输出。

### 首期迁移边界

- Algorithms 迁移 `ComputePsoVelocity`、`DistanceSquared` 和 `UpdateFireflyPosition` 的完整方法展开。公式、归约顺序和标量尾部逐项保持现状。
- Core 迁移 `ReflectCandidateRepair.Repair` 的外层级联，以及当前仅因位宽不同而重复的 load、Reflect block 和危险 lane mask 机械展开。标量 `Reflect`、`RepairLargeOffsetLanes`、边界所有权和创建时派生保持手写。
- 只生成现有 `double` specialization。生成器测试以最小虚拟模板证明 `float`/`int` 的元素计数替换能力，并以负面诊断拒绝未声明的浮点/整数能力；不在 Core 或 Algorithms 生成这些生产路径。
- 不实现对齐加载、前导循环或新加载策略。模板模型只为未来新增显式策略保留版本化能力字段；字段未声明时生成当前 `LoadUnsafe`/`StoreUnsafe` 结构，不能增加运行时探测。

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 512→256→128→scalar 机械展开 | Core/Algorithms 各自手写 | `eng` 生成器 | 单一编译期权威，消除跨项目漂移。 |
| 运算模板和标量尾部 | 普通 Core/Algorithms `.cs` | 对应消费方的受限 AdditionalFile | 公式仍由业务所属层显式维护，生成器不解释语义。 |
| Reflect 边界、mask、危险 lane 与标量参考 | Core | Core | 保持 Repair 数值职责。 |
| PSO/Firefly 公式、归约和工作区 | Algorithms | Algorithms | 保持算法职责。 |
| 生成后的私有方法 | Core/Algorithms 手写 | 各自编译中的生成源 | 不形成运行时共享后端或反向依赖。 |
| 生成正确性与诊断 | 不存在 | 生成器测试项目 | 隔离构建工具测试，不污染运行时测试职责。 |

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| 模板结构、唯一宽度块和允许的占位符 | 增量生成器 | 每次相关模板变化 | 否，编译期 | 无效模板产生带位置 diagnostic，不生成部分生产方法。 |
| 元素类型和能力集合 | 增量生成器 | 每次相关模板变化 | 否，编译期 | 未批准类型/能力编译失败，不加入运行时检查。 |
| 生成源确定性 | 生成器测试/CI | 每次变更 | 否 | 相同输入逐字节相同，hint name 无冲突。 |
| Span 长度与边界 | 现有消费方 | 保持现有次数 | 是 | 保持现有 `ArgumentException` 和越界保护。 |
| 特殊值、归约、mask 与标量参考 | 现有行为测试 | 每次 CI | 是 | 保持 SPEC-0004/0005 数值和确定性边界。 |
| 零运行时开销 | 实施审查、JIT/BDN | 替换前后 | 是 | 额外调用、分派、分配或可确认回退均阻止替换。 |

## API 与行为变化

- 新增：仅仓库内部的生成器项目、生成器测试项目、受限模板文件、编译诊断和生成源。
- 修改：Core/Algorithms 内部容器成为 `partial`；被迁移的私有/内部方法由模板生成；项目增加 analyzer 形态的编译期引用。
- 删除：通过门槛后删除相应普通 `.cs` 中的重复手写级联及三宽度机械副本。
- 破坏性变化：无公开 API 或运行时行为破坏；构建仓库需要恢复新增的集中锁定 Roslyn 编译期包。
- 调用方迁移方式：库用户无需迁移；仓库维护者修改 SIMD 热路径时编辑受限模板和对应测试。
- 明确保持不变的行为：所有公开签名、包依赖、Repair/算法职责、公式、算术顺序、特殊值、随机调用、状态、取消、Repair/Evaluate 时机、硬件宽度顺序和标量尾部。

## 替代与清理计划

- 被取代的类型、接口和入口：不取代运行时类型或入口；只取代现有方法内的重复手写展开。
- 必须删除的转发层、兼容壳和重复抽象：不得新增 `SimdKernel` 接口、delegate adapter、Core `VectorOps`、运行时模板类型或新旧选择开关；候选通过后删除旧方法体和重复宽度方法。
- 必须删除或改写的旧测试：不删除行为测试；生成器测试新增机械输出/诊断覆盖。若测试直接依赖源码成员布局，只改为断言同一可观察结果。
- 必须更新的示例和文档：用户示例/API 文档无需变化；更新架构概览以报告构建期生成和运行时私有归属；新增 ADR-0017；完成时同步 Spec index 和 Verification。
- 全仓库残留搜索方式：搜索手写 `Vector512.IsHardwareAccelerated`、三宽度循环、生成占位符、运行时 generator 引用、delegate/kernel 抽象、旧方法体和公开 API 差异。
- 保留旧结构时的真实消费者、期限和删除条件：实施测量期间以基线提交和 JIT/BDN artifact 比较，不在生产或 benchmark 长期保留复制的手写实现；任一门槛失败则删除候选生成接入并完整保留原实现。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 是 | partial 容器、Reflect 模板、analyzer 引用；运行时职责不变。 | Core 行为测试、API/发布产物、JIT/BDN。 |
| Algorithms | 是 | partial `VectorOps`、三项模板、analyzer 引用；算法公式不变。 | VectorOps/算法测试、固定 seed、JIT/BDN。 |
| Experiments | 否 | 不引用生成器或 SIMD 类型，运行调用链不变。 | 项目引用和完整测试。 |
| Examples | 否 | 公开组装方式不变。 | Release build。 |
| Tests | 是 | 新生成器测试项目；现有行为测试继续作为语义门。 | 全部测试。 |
| Benchmarks | 是 | 运行基线提交与生成提交的既有局部/端到端基准和反汇编。 | BenchmarkDotNet artifacts、Verification。 |
| XML 文档 | 否 | 不新增公共 API；现有公开 XML 来源不变。 | DocFX、API diff。 |
| 用户/API 文档 | 否 | 用户无新入口或配置。 | 文档审查。 |
| ENGINEERING | 否 | 现有 SDD、性能和项目边界规则已覆盖。 | 文档审查。 |
| ADR | 是 | 新增 ADR-0017，补充 ADR-0016 的编译期实现策略，不替代运行时宽度与性能准入决定。 | ADR index 和文档门禁。 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | 512/256/128 硬件组合与 0/1/2/7/8/15/16 golden 输出；现有尾部行为测试 | Generator/Unit | 每个完整块只在剩余长度足够时生成，顺序唯一。 |
| FR-002 | analyzer 引用、API diff、发布目录与 IL 调用审查 | Build/Integration | 无生成器运行时依赖、公开符号或共享 Helper。 |
| FR-003 | PSO/Firefly/Reflect 差分、特殊值、归约和危险 lane 测试 | Unit/Integration | 公式和所属层不漂移。 |
| FR-004 | `double` 正例、虚拟 `float`/`int` Count 正例、能力缺失负例 | Generator | 只显式扩展，错误在编译期失败。 |
| FR-005 | 重复运行 generator driver、hint name、相同输入 hash、诊断位置 | Generator/CI | 输出确定且可审查。 |
| FR-006 | 残留搜索、调用图、候选失败清理测试 | Review/Build | 每项生产操作只有生成 SIMD 路径与独立标量参考。 |
| NFR-001 | 替换前后 IL/JIT 结构、MemoryDiagnoser、BenchmarkDotNet | Performance | 无额外分派/分配，代码结构等价且无确认回退。 |
| NFR-002 | SPEC-0004/0005 全部行为、固定 seed、状态/调用次数测试 | Unit/Integration | 无新数值、随机、状态或时机差异。 |
| NFR-003 | Reflect、PSO、Firefly 局部与端到端基准 | Benchmark | 记录基线/生成结果、比率、环境和分配。 |
| NFR-004 | 受限语法负例、增量缓存、Release/CI 构建 | Generator/Build | 不演化为通用宏系统，失败可定位。 |

## 性能测量停点

1. Plan 批准后、修改生产路径前，先在基线提交 `ed2de0d3` 上运行并保存当前 Reflect、PSO、Firefly 的指定局部/端到端 BenchmarkDotNet 和 JIT 反汇编。
2. 遵守 ADR-0016：每次实际 BenchmarkDotNet 测量前，先向项目作者展示待测代码、完整命令、筛选器、Job、输入和环境，等待反馈后再运行。
3. 生成候选完成行为验证后，在同一机器、Runtime、JIT、配置和输入上运行相同命令；不以不同配置的历史数字直接判定零开销。
4. 任一新增调用/分派/分配、JIT 结构不等价或主要比较出现可确认回退时，停止对应迁移并删除其候选生成接入；不得以放宽门槛或保留运行时开关继续。

## 风险和回退

- 最大实现风险：受限模板为了覆盖归约、原位写回和 Reflect lane 修补而逐渐变成通用 C# 宏系统；其次是生成后的源码形态改变 JIT 内联、边界检查或局部变量生命周期。
- 可能产生的行为漂移：错误替换向量静态 API/Count、改变归约发生位置、标量尾部漏执行、`#line` 或 partial 容器定位错误、模板与生成方法可见性不一致。
- 如何及早发现：先只实现 generator golden/diagnostic 测试，再用不进入生产的最小虚拟模板覆盖三宽度；随后迁移最简单的 `UpdateFireflyPosition`，比较生成源、IL/JIT 和行为；只有通过后依次迁移 PSO、DistanceSquared 和 Reflect。
- 退回 Spec 澄清的条件：必须解析任意 C#、需要运行时 callback/接口、无法直接生成完整方法、需要提交双份生成源、需要新增生产 `float`/`int`/对齐策略、改变数值容差或必须保留新旧生产路径。

## ADR 判断

- 是否触发 ADR：是。
- 判断依据：新增跨 Core/Algorithms 的编译期 SIMD 生成策略、构建项目和长期维护入口，改变了 ADR-0016 所记录的手写私有级联实现方式，但不改变其运行时所有权、宽度顺序和性能准入。
- 新 ADR 或被替代 ADR：[ADR-0017](../../decisions/0017-repository-private-simd-source-generation.md)“仓库私有 SIMD 增量源码生成”，状态 `Accepted`；它补充而不替代 ADR-0016。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-08-30
