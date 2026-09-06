# JIT 证据

输出来自 .NET 10 Arm64 FullOpts（`COMPlus_TieredCompilation=0`）。`production-nextulong-*` 和 `production-fill-*` 是成员实现编译期间取得的生产输出；`production-wrapper-*` 是恢复 wrapper/ref 唯一生产实现后取得的生产输出；`member-*` 与 `wrapper-ref-*` 是同样结构的临时诊断类型，用来隔离转换体和 wrapper 的内联/别名影响，诊断类型不进入生产程序集。

- [`production-nextulong-fullopts.txt`](./production-nextulong-fullopts.md)：生产成员 `NextULong()` 的实际调用体，成员 `NextRaw()` 内联后为 76 bytes。
- [`production-fill-fullopts.txt`](./production-fill-fullopts.md)：生产成员 `Fill(Span<ulong>)` 的实际循环，成员 `NextRaw()` 内联后为 104 bytes。
- [`production-wrapper-nextulong-fullopts.txt`](./production-wrapper-nextulong-fullopts.md)：恢复后的生产 wrapper/ref `NextULong()` 调用体，`NextRawFromFields()` 和静态转换内联后为 76 bytes。
- [`production-wrapper-fill-fullopts.txt`](./production-wrapper-fill-fullopts.md)：恢复后的生产 wrapper/ref `Fill(Span<ulong>)` 实际循环；四状态在循环外装载、循环内推进、循环后写回，输出为 112 bytes。
- [`member-call-fullopts.txt`](./member-call-fullopts.md) 与 [`wrapper-ref-call-fullopts.txt`](./wrapper-ref-call-fullopts.md)：两种标量内部形状均为 76 bytes；成员有 1 个 inlinee，wrapper/ref 有 2 个。
- [`member-fill-fullopts.txt`](./member-fill-fullopts.md) 与 [`wrapper-ref-fill-fullopts.txt`](./wrapper-ref-fill-fullopts.md)：两种 Fill 内循环均为 104 bytes；成员有 1 个 inlinee，wrapper/ref 有 2 个。

两种标量输出都显示状态字段直接成对读取、转换和成对写回，最终机器码均为 76 bytes；Fill 输出则直接显示成员方案在循环内读写字段，而 wrapper/ref 在循环外装载和写回字段。JIT 没有留下随机源接口、虚成员、delegate 或 factory 分派。差异主要是 inlinee 数量、字段读写位置和指令调度顺序，因此不能只以“wrapper 有 helper”推断其一定较慢。
