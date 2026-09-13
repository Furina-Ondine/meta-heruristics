# SPEC-0010 证据索引

本目录只保留 [`verification.md`](../verification.md) 直接引用的 BenchmarkDotNet 汇总表（`-report-github.md`），没有中间脚本、临时工程、原始日志或反汇编文件（反汇编可由报告末尾的命令随时再生）。

- `width-128/`：`DOTNET_MaxVectorTBitWidth=128`，实测 `Vector<ulong>.Count=2`。
- `width-256/`：默认宽度，实测 `Vector<ulong>.Count=4`。
- `width-512/`：`DOTNET_MaxVectorTBitWidth=512`，实测 `Vector<ulong>.Count=8`。

每个目录包含该宽度下全部对照场景的 `-report-github.md`（含均值、误差、比值与分配）。

复现命令见 [`verification.md`](../verification.md) 的“证据清单与复现”。
