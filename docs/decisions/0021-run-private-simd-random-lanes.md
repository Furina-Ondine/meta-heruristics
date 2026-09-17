# ADR-0021: run 私有 SIMD 随机 lane 与封闭执行契约

## 状态

Superseded

2026-09-08：由 [ADR-0023](0023-separate-scalar-and-batch-random-state.md) 替代。以下保留历史决策，不再授权旧共享状态方案。

替代 [ADR-0020](0020-core-owned-random-source-and-run-execution.md)。2026-09-06 项目作者批准 SPEC-0010 至 SPEC-0014 联合规格；实现仍待 Approved Plan。

## 背景

SPEC-0010 要求原始状态推进本身 SIMD 化。单流连续状态存在依赖，因此采用 run 私有的多个 xoshiro256++ lane，并通过内部 Jump 派生起点。

## 决策

- Randomness 仍由 Core 独占，不新增项目、运行时包或反向依赖。公开 sealed RandomSource，构造仍为 internal；不存在用户随机源 factory、接口、第二 PRNG 或兼容层。
- 内部唯一 PRNG 为 xoshiro256++ 1.0，每 lane 拥有四个 64 位状态字。SplitMix64 从 run seed 初始化首 lane，其余 lane 由作者参考 Jump 派生，起点间隔为 2^128 次推进。Jump 不公开，不宣称不同 seed 的流互不重叠或 lane 统计独立。
- 标量、Fill 和分布调用共同消费一组 lane 状态及必要的原始输出缓存，不保留独立标量流。lane 数和排放规则由 Approved Plan 固定，不随线程、Group 或调用时间选择。
- Core 私有随机内核可使用通用 Vector128/256/512，不使用 ISA 专属 API，不接入源码生成器。无硬件加速时标量执行同一 lane 模型。ADR-0019 的标量 Reflect 和生成器边界保持。
- 所有权和生命周期完整保留：Factory 为每 Group 创建独占有状态 IOptimizer；Optimizer 直接持有工作区，不可并发使用，顺序 run 只复用物理存储，ResetForRun 完整重置逻辑状态；不继承 IDisposable；运行异常后丢弃并由 Factory 重建。
- Context 每 run 创建，统一持有 Problem、seed、RandomSource、取消、Repair 和评估计数；不得缓存到后续 run。每 Group 使用独立 Problem/Optimizer，只能共享明确不可变的数据；禁止全局随机流和隐式播种。
- Runner 显式接受 ulong seed；Experiment 只排程 seed，默认 unchecked(BaseSeed + repetitionIndex)，不感知 lane 和 Jump。Case、Group、取消、结果矩阵和统计保持不变。
- RandomSource 的空 Fill 不消费状态，无效参数在状态推进或目标写入前失败。标准正态仍为独立静态分布，无跨调用 spare。
- 仅保证相同版本、目标环境、seed、方法序列及参数重复一致；不保证切分、标量/批量、跨版本或跨平台一致。外部策略测试继续通过公共 Runner 回调获取随机能力。

## 替代方案

- 仅向量化输出转换：不满足已批准的原始状态 SIMD 目标。
- 独立标量流和批量流：增加两个状态消费模型，不采用。
- 更换 PRNG 或公开多流：超出本次需求。
- 新建 Randomness 包或向 Algorithms 迁移随机职责：无独立消费者，且破坏既定职责边界。

## 后果

多 lane 与缓存增加每 run 内存和播种成本，必须在内核和完整 run 基准中分别计量。旧完整序列不保留；逐 lane 及交织流均需正确性和统计验证。实现验收前不将批准设计描述为已上线能力。

## 重新评估条件

出现公开子流、其他 PRNG、GPU/随机访问、用户构造与注入，或超出初始化 Jump 的状态管理需求时新增 Spec/ADR。成本无法通过批准门槛时退回 Plan 或 Spec。
