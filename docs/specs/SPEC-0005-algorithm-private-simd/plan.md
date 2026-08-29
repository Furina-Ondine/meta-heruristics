# SPEC-0005 技术计划

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`1f4b98f`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`NFR-001`、`NFR-002`、`NFR-003`
- 批准人：项目作者
- 批准日期：2026-08-29

## 调查结论

- `ParticleSwarmOptimization<TProblem>` 目前逐粒子、逐维计算惯性、认知和社会速度项，逐元素 Clamp 速度后加到源位置。每个候选抽取两个随机标量。
- `FireflyAlgorithm<TProblem>` 从源位置开始，按既有顺序遍历每个更优萤火虫；先标量计算平方距离，再逐维加入吸引项和随机步长，并在每次吸引后调用 Repair。
- 两条算法都通过运行器/上下文复用候选工作区。随机数、求值和修复属于既有生命周期，迁移时不能改变调用次数、次序或 Repair 时点。
- `Metaheuristics.Core` 已直接引用集中管理版本的 `System.Numerics.Tensors`；`Metaheuristics.Algorithms` 当前仅引用 Core。现有基准主要覆盖 Repair/Bat，现有测试已覆盖复现性、修复/排序和工作区复用。
- [`SPEC-0003`](../SPEC-0003-simd-repairs/spec.md) 的「仅 Core 可直接引用 Tensors」限制由本 Spec 的 `FR-001` 局部替代；其 Repair 行为不受影响。`SPEC-0004` 与本工作无实施耦合。

## 方案与边界

| 方案 | 决定 | 原因 |
| --- | --- | --- |
| 直接 `TensorPrimitives` | 优先级 1 | 最小化新代码和抽象；优先验证具体 Span 重叠组合。 |
| 无分配的 `TensorPrimitives` 操作组合 | 优先级 2 | 在没有单一直接 API 时仍使用 BCL 向量化；临时量必须来自复用工作区。 |
| Algorithms 程序集的 `internal VectorOps` | 优先级 3，条件采用 | 仅在前两者不能同时满足正确性、可读性和测量门槛时，为实际缺口提供最小私有包装。 |
| 通用 `Vector512<T>`/`Vector256<T>`/`Vector128<T>` | 任一实际算法的 `VectorOps`，优先级 3 的实现细节 | 按 `TensorPrimitives` 式的 512→256→128→标量级联处理所有长度，避免单一运行时宽度的大尾部。 |
| ISA 专属 Intrinsics、Core/公开 VectorOps、公共后端或位置布局变更 | 不采用 | 超出本补充的明确范围，需要新的完整 SDD。 |

| 范围 | 计划后的职责 | 明确不做 |
| --- | --- | --- |
| `Metaheuristics.Algorithms` | 直接消费 `System.Numerics.Tensors`；实现算法私有向量内核 | 向 Core 或其他程序集暴露 SIMD 抽象 |
| PSO | 在已分配的目标位置/速度 Span 上完成更新 | 改变状态机、随机序列或 Repair 时点 |
| Firefly | 复用差向量/随机步长工作区，计算距离和位置更新 | 批量化吸引者、合并多次 Repair 或重排迭代 |
| 可选 `internal VectorOps` | 封装已验证、无分配且确有调用者的操作；实际算法可按固定宽度级联 | 成为通用库、ISA 专属内在函数层或公共后端 |

不改变公开 API、XML 文档、Core 的生产代码或集中包版本；Algorithms 仅添加不带版本号的直接 `PackageReference`。

## 保持的语义

1. PSO 每候选仍抽取两个随机标量，调用顺序不变。
2. Firefly 按现有吸引者/维度顺序产生随机步长；如使用随机缓冲区，填充次序须逐一等同原标量循环。
3. PSO 在完整速度/位置更新后 Repair；Firefly 在每个更优吸引者更新后 Repair。求值、约束和排序不变。
4. 临时向量仅在 `EnsureWorkspace` 或首次工作区创建时分配并复用；`Advance` 热路径禁止新托管数组、`List<T>`、闭包和装箱。
5. 每个拟用 API 的源/目标 Span 重叠语义，必须用定向测试和编译验证确认。不能安全重叠时使用另一块预分配缓冲区，不作未验证的就地写入假设。
6. 仅保证相同 OS、体系结构、运行时和 JIT 组合内的确定性。归约可在不同平台发生不同浮点累加顺序；不承诺跨平台逐位一致，也不维护跨平台结果金样。

## 实施补充：固定宽度级联（2026-08-29）

项目作者指出原始 `VectorOps` 只基于单一运行时向量宽度处理尾部，未像 `TensorPrimitives` 一样按固定宽度继续处理。例如 15 个 double 在 512 位机器上不应退为 8 位向量加 7 个标量，而应依次处理 8（512 位）、4（256 位）、2（128 位）和 1 个标量。

- PSO 的内部实现改为：在 `Vector512.IsHardwareAccelerated` 时尽可能处理 8 个 double；剩余长度再分别由 `Vector256`、`Vector128` 和标量路径处理。每层只在硬件支持且不会越界时进入。
- 这不是公开后端、用户配置或 ISA 专属 API；选择仅影响同一公式的私有实现，随机、Repair、求值、比较和工作区责任不变。
- 补充诊断长度为 2、7、8、15、16、31、32、33、127、128、129，并在报告中记录 512、256、128 位向量的硬件可用性与 lane 数。
- 按项目作者指示，任何后续 BenchmarkDotNet 测量前必须先展示修改后的具体代码和命令，并等待明确反馈；项目作者已确认 Firefly 代码和两条命令，随后完成构建、测试及基准验证。

## 实施路径

### 阶段 A：基线与基准设施

1. 在 `Metaheuristics.Algorithms.Benchmarks` 添加 PSO 和 Firefly 的标量基线。记录 .NET/运行时、CPU/OS、种子、问题、种群、迭代和 BenchmarkDotNet 输出。
2. 每个算法提供两类测量：
   - 预分配工作区、简单目标、无额外 Repair 成本的内核/近内核；
   - 经正常 `Advance` 生命周期、使用 Sphere 求值和 Clamp Repair 的端到端。
3. 两类测量均在维度 32、128 下运行；候选实现必须与同配置的同一标量基线比较。

### 阶段 B：PSO（先行门槛）

1. 在 Algorithms 添加 `System.Numerics.Tensors` 的直接包引用。
2. 为实际会使用的 `TensorPrimitives.Subtract`、`Multiply`、`Add`、`Clamp` 和每种重叠组合添加定向测试，覆盖长度边界、非有限值、负零和普通向量。
3. 首先以候选已有的 `Position`/`Velocity` 为工作区，按下列链路使用直接 API 或无分配组合：
   - 个人最优减源位置，乘认知系数与已抽取随机标量；
   - 源速度乘惯性并累加认知项；
   - 全局最优减源位置，乘社会系数与已抽取随机标量并累加；
   - Clamp 速度，再将它加到源位置得到候选位置。
4. 只有上述直接组合因重叠规则、可读性或测量无法成立时，才引入 Algorithms 私有的最小 `VectorOps`；不引入公开开关或公共后端。任何采用它的算法都按当前剩余长度和 `Vector512`、`Vector256`、`Vector128` 的硬件可用性依次处理，不得使用 ISA 专属 API，也不得改变随机、Repair、求值时点。
5. 标量实现可仅作为测试和基准参考路径保留。
6. 仅当 PSO 在 32、128 维的内核与端到端共四项比较**全部**快于标量基线，并且每次 `Advance` 无新增托管分配，才进入阶段 C。

### 阶段 C：Firefly（受 PSO 门槛约束）

1. 在 PSO 四项门槛全部通过后，扩展/复用私有工作区以容纳差向量和随机步长，吸引者循环中不得分配。
2. 每个更优吸引者优先使用直接归约；无合适直接 API 时，以 `TensorPrimitives.Subtract` 写入预分配差向量，再以 `TensorPrimitives.Dot(difference, difference)` 计算平方距离。
3. 使用直接 TensorPrimitives 调用或无分配组合完成吸引和随机步长的逐元素更新；每次吸引者处理后仍调用既有 Repair。
4. 增加差向量、平方距离、重叠、特殊浮点、吸引者次序、随机调用数和 Repair 次数的定向测试。若直接组合不满足正确性或性能，按阶段 B 同一规则启用最小私有 `VectorOps`，并比较固定宽度级联路径。
5. Firefly 同样必须在 32、128 维的内核和端到端四项比较均胜过自身标量基线，且 `Advance` 无新增分配；否则删除候选 SIMD 路径并在 Verification 记录结论。

### 阶段 D：Bat / Cuckoo 诊断

只调查和记录热点、可能的 TensorPrimitives 映射及测量结果；不修改生产实现。若值得迁移，另起或扩展经批准的 Spec/Plan。

## 验证矩阵

| 需求 | 验证证据 |
| --- | --- |
| `FR-001` | 项目引用检查；PSO TensorPrimitives/组合的行为和重叠测试；API 差异检查确认无公开 VectorOps。 |
| `FR-002` | Firefly 差向量、距离、吸引者次序、随机调用、每次 Repair 的测试；仅在 PSO 门槛后执行。 |
| `FR-003` | 四项（2 维度 × 内核/端到端）BenchmarkDotNet 对比；按 `Advance` 的托管分配检查。 |
| `FR-004` | Bat/Cuckoo 热点诊断与未迁移结论写入 `verification.md`。 |
| `NFR-001` | 同一目标环境重复运行的复现性测试；不执行跨平台数值金样断言。 |
| `NFR-002` | 工作区复用、Span 重叠、非有限值/负零和修复边界测试。 |
| `NFR-003` | 审查确认仅使用通用固定宽度向量、无 ISA 专属 intrinsics、无 Core/公共 VectorOps、无第二份包版本。 |

基准报告必须保留原始 BenchmarkDotNet 输出和分配数据；静态调用计数不构成性能证据。

## 影响矩阵

| 项目 | 影响 | 动作 |
| --- | --- | --- |
| `Metaheuristics.Core` | 无生产代码影响 | 维持既有 Tensors 依赖和 Repair 契约。 |
| `Metaheuristics.Algorithms` | 有 | 添加直接依赖，先迁移 PSO，条件迁移 Firefly。 |
| `Metaheuristics.Algorithms.Tests` | 有 | 新增语义、随机、复用和特殊浮点测试。 |
| `Metaheuristics.Algorithms.Benchmarks` | 有 | 新增标量、内核和端到端基准。 |
| Experiments / 示例 | 无 | 不修改。 |
| ADR | 有 | 以 [ADR-0016](../../decisions/0016-algorithm-fixed-width-simd-cascade.md) 记录固定宽度级联和测量暂停约束。 |
| 公共文档 | 无 | 不修改公开 API 或用户调用方式。 |
| Spec / Verification | 有 | 记录逐项证据、基准和诊断结论。 |

## 风险与退出条件

| 风险 | 应对与退出条件 |
| --- | --- |
| Span 重叠不受 API 支持 | 先测试；改用预分配缓冲区，绝不引入热路径分配。 |
| 多次操作的内存往返抵消收益 | 比较直接调用、无分配组合和实际需要时的私有包装；任一主比较不优即删除候选路径。 |
| Firefly 归约末位变化 | 同环境复现性测试保护；跨平台变化是已批准范围，不加稳定性承诺。 |
| 随机/Repair 语义漂移 | 记录调用顺序与次数；发现差异即停止并修正，不能以基准收益覆盖语义变化。 |
| 范围蔓延 | Bat/Cuckoo 仅诊断；需要新算法、公开抽象或新数值契约时回到 Spec/Plan。 |

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-08-29
- 实施补充批准：项目作者
- 实施补充日期：2026-08-29
- 实施补充内容：PSO 以通用 `Vector512<T>`、`Vector256<T>`、`Vector128<T>` 的 512→256→128→标量级联处理长度；每次 BenchmarkDotNet 测量前等待项目作者审阅代码和命令。
