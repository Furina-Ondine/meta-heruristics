# ADR-0019: 标量 Reflect 与仅 Algorithms 使用 SIMD 生成

## 状态

Accepted

替代 [ADR-0017](0017-repository-private-simd-source-generation.md) 与 [ADR-0018](0018-repair-boundary-shape-specialization.md)。

## 背景

SPEC-0007 收窄了 Repair 边界 API，但其 Reflect SIMD 专用化在 Apple M4 Pro、.NET 10 Arm64 的同机 BenchmarkDotNet 对照中没有稳定优于独立的纯标量参考。扩展测量覆盖 2、7、8、31、32、33、127、128、129、1024 维、标量/标量和向量/向量形状，使用 10 次 warmup、30 次正式迭代及每迭代 100 万调用；多数主要点回退。

为无收益的 Core 热路径保留 intrinsic 模板、生成器引用、宽度级联和数值修补会扩大维护面。Algorithms 的固定宽度向量算术仍有独立消费者和既有性能证据，故不应随 Reflect 一并删除。

## 决策

- Clamp、Reflect 和 RandomReset 继续只公开标量/标量及向量/向量工厂，并在创建时分派到密封私有类型。
- 两种 Reflect Repair 都使用简单的逐元素标量循环和同一私有数值参考函数；不保留 SIMD、硬件探测、宽度级联、lane 修补、模板或 Core 对生成器的 analyzer/AdditionalFiles 引用。
- 删除 `CandidateRepairs.simd.cs` 及所有仅为 Reflect 服务的生成器关联。生成器项目、其测试和 Algorithms 的生成接入继续保留，且不新增对 Core 的间接依赖；生成器只保留 Algorithms 使用的硬件门宽度展开，删除未被生产模板使用的无硬件门展开及其关联测试。
- 旧 SIMD 设计和其历史性能记录保留在已被替代的 ADR/SPEC 中，不作为当前 Core 架构契约。

## 替代方案

- 保留 SIMD 以等待另一台机器的收益：不采用。缺少当前目标机的一致收益，不应以复杂度交换假设性收益。
- 仅保留 scalar/scalar SIMD：不采用。扩展对照中该形状在主要尺寸上回退，形成不对称实现没有依据。
- 删除 Algorithms 的生成器：不采用。它仍服务独立的算法热路径，删除超出 Reflect 回退范围。

## 后果

Reflect 恢复为容易审查、与标量参考直接一致的实现；Core 的编译输入和运行时代码减少。M4 Pro 测量结果只作为该机器、Runtime 和输入下拒绝候选的证据，不与 9800X3D 的绝对纳秒比较。后续若要重新引入 Reflect SIMD，必须有新的 Spec、数值验证和同机性能证据。

## 重新评估条件

若目标部署环境、Runtime 或 Reflect 输入分布发生实质变化，且一个新的候选在标量/标量与向量/向量主要维度上均有可确认收益，可新增 Spec 和 ADR 重新评估。
