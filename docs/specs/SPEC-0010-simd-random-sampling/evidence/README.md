# SPEC-0010 证据索引

本目录只保留 [`verification.md`](../verification.md) 直接引用的基准产物，没有中间脚本、临时工程或原始日志。

- `width-128/`：`DOTNET_MaxVectorTBitWidth=128`，实测 `Vector<ulong>.Count=2`。
- `width-256/`：默认宽度，实测 `Vector<ulong>.Count=4`。
- `width-512/`：`DOTNET_MaxVectorTBitWidth=512`，实测 `Vector<ulong>.Count=8`。

每个目录包含该宽度下全部对照场景的 `-report-github.md`（BenchmarkDotNet 汇总表，含均值、误差、比值与分配）和一份 `-asm.md`（反汇编诊断，用于 FR-006 的向量算术与正态向量数学证据）。

复现命令见 [`verification.md`](../verification.md) 的“证据清单与复现”。
