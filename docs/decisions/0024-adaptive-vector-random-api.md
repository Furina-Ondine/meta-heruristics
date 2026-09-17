# ADR-0024: 自适应 Vector 随机状态与向量采样 API

## 状态

Accepted

2026-09-08，项目作者明确要求统一使用自适应 Vector API、删除 BatchCursor、以四个 ref vector 推进状态，并新增两个向量返回采样方法供 StandardNormal.Fill 使用。替代 [ADR-0023](0023-separate-scalar-and-batch-random-state.md)。本记录确认用户指定方向；具体方法命名及完整行为见本次批准的 [SPEC-0010 修订稿](../specs/SPEC-0010-simd-random-sampling/spec.md)，尚未实现。

## 背景

固定 Vector512 状态和手写硬件分组不符合用户新要求。BatchCursor 增加了一层状态封装，用户要求与原标量四 ref 结构一致。StandardNormal 需要直接取得批量随机向量。

## 决策

- 保留一个 run 私有 RandomSource 的两套状态：四个 ulong 单值状态字，四个 System.Numerics.Vector<ulong> 批量状态向量。
- 批量 lane 数由 Vector<T>.Count 决定；状态算术使用该 API，不使用固定宽度 intrinsic 或手写级联。软件执行仍使用 Vector<T>。
- 内核直接接收四个 ref Vector<ulong>；不引入 BatchCursor、BeginBatch/EndBatch、状态句柄或持久输出缓存。
- 新增两个公开向量采样方法，分别返回 Vector<ulong> 原始样本和 Vector<double> 单位样本；与全部 Fill 共用批量状态。原单值入口仍只推进标量状态。
- StandardNormal.Fill 通过单位向量采样方法取得批量输入，Sample 保留标量路线。正态公式和分布职责仍属于 StandardNormal。
- 保留既有 PRNG、内部构造、显式 seed、Core 所有权、run/Group 生命周期及数值/异常规则；不新增公共子流、Jump 或后端配置。
- 有效向量宽度属于执行环境，不能再承诺固定八 lane 或不同宽度的批量完整序列一致。

## 替代方案

- 固定 Vector512 状态和 512/256/128 级联：用户要求改为自适应 Vector API。
- ref struct BatchCursor：用户明确要求删除，直接传四个状态 ref。
- StandardNormal.Fill 继续调用单值 NextDouble：违反指定批量入口和状态隔离。

## 后果

API 返回的样本数与运行时向量宽度相关，测试与基准必须记录实际 Count。不同宽度的初始化成本、状态体积和批量序列可能变化。需要重新审查规格、Plan 和公共 XML/API 文档，旧批准不能覆盖新增细节。

## 重新评估条件

更换 PRNG、改变状态隔离、恢复固定宽度/游标、增加向量范围重载或引入其他公开随机能力时重新审查规格与决策。

2026-09-08 批准补充：项目作者同意自适应向量规格，并明确 StandardNormal.Fill 获取单位随机向量后，Sin/Cos 仍直接使用 Vector API。对应规格要求完整 Box–Muller 向量数学及向量尾块，禁止改回逐 lane Math 超越函数；Sample 的标量路线保持。

2026-09-13 项目作者补充，2026-09-14 批准：允许批量状态推进的循环左移在宽度匹配且指令集支持时使用 ISA 专属旋转指令（AVX-512F 的 512 位 `vprolq`、AVX-512VL 与 AVX10.1 的 256/128 位变体，经 `Vector<T>.AsVector512/256/128()` 与 `.AsVector()` 转换），其余平台保留“两次移位 + 或”回退。该补充只放开旋转指令这一处固定宽度用法；其他 Vector128/256/512 或 ISA 专属 API、以及手写级联仍然禁止。
