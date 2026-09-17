# ADR-0022: 批量采样下的算法私有 SIMD 与融合

## 状态

Superseded

由 [ADR-0025](0025-direct-tensor-primitives-and-benchmark-execution.md) 替代。2026-09-14 项目作者修订直接 TensorPrimitives 优先和基准执行安排；以下保留历史决策，不作为现行测量批准门。

替代 [ADR-0016](0016-algorithm-fixed-width-simd-cascade.md)。2026-09-06 项目作者批准联合规格；具体实现受 Plan 和性能门槛约束。

## 背景

PSO 已有速度 SIMD，但限幅和位置更新是额外遍历。四种算法的随机能力已迁移到 Core，当前获批进一步批量化，并评估 PSO 完整候选融合。

## 决策

- Algorithms 先审查直接 TensorPrimitives 调用和无分配组合；有具体性能依据时，私有 VectorOps 可使用通用 Vector512/256/128。
- 算法无状态算术继续按剩余长度和硬件支持依次执行 512、256、128 位完整块，最后标量尾部；模板展开仍仅服务 Algorithms。
- PSO 可按 SPEC-0013 比较融合速度、限幅和位置更新的私有内核与当前 Clamp/Add 组合；通过数值及内核、端到端门槛后才替代。不再强制这两步永久使用 TensorPrimitives。
- 不使用 ISA 专属 API，不引入公开 SIMD 后端、配置、种群 SoA 布局、共享缓冲或跨 Optimizer 状态。
- SPEC-0011 至 SPEC-0014 授权各自范围内的批量抽样和旧轨迹变化，不授权改变随机变量的含义或相关结构。PSO 每粒子仍共享一对随机系数；Firefly 保持吸引者顺序和逐次移动后 Repair。
- 保持公共 API、Repair/Evaluate 时点、状态隔离与数值分类行为。不同环境的数值承诺不扩大；相同目标环境内以差分和固定 seed 测试验证。
- 每次 BenchmarkDotNet 前展示待测代码与完整命令并获得项目作者反馈；同机内核和端到端门槛决定保留。记录各宽度硬件支持，覆盖 2、7、8、15、16 和主要长度。

## 替代方案

- 仅维持现有 PSO 三次遍历：作为生产基线保留，若融合无收益则继续使用。
- 一律使用 private intrinsic：不采用，简单 TensorPrimitives 路径仍优先。
- 单一 Vector<T> 宽度与长标量尾部：不采用，保留已有固定宽度级联。

## 后果

允许有证据的融合和批量化，同时保持算法公式、随机时机及工作区的唯一所属层。拒绝候选应删除；生产代码不保留实验切换开关。

## 重新评估条件

需要公共后端、ISA 专属 API、种群布局变化、跨回调预取或改变算法语义时，新增 Spec/ADR。
