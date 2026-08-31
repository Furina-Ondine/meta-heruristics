# SPEC-0007 验证报告

## 元数据

- Spec：[`spec.md`](./spec.md)
- Plan：[`plan.md`](./plan.md)
- Tasks：[`tasks.md`](./tasks.md)
- 验证日期：2026-08-31
- 最终结果：`Passed`

## 需求覆盖

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | `CandidateRepairs` 的两种同形状 factory | Core 测试、仓库残留搜索 | ADR-0019、用户指南 | Passed |
| FR-002 | 六个同形状私有 Repair 类型 | Core 测试、实现审查 | architecture overview | Passed |
| FR-003 | `ClampValue`、`ReflectValue` 与 RandomReset 循环 | 位置、特殊值、复制、长度与固定 seed 测试 | SPEC-0007 | Passed |
| FR-004 | `ScalarReflectCandidateRepair`、`VectorReflectCandidateRepair` 的直接 `for` 循环 | Core/`eng` 残留搜索、差分测试 | ADR-0019 | Passed |
| FR-005 | `RepairBenchmarks` 的纯标量参考与历史 SIMD 候选 | BenchmarkDotNet 长测、MemoryDiagnoser | 本报告 | Passed |
| NFR-001 | public factory、ADR 替代关系 | Release build、文档审查 | ADR-0019 | Passed |
| NFR-002 | Core 无 SIMD/生成器接入 | 残留搜索、MemoryDiagnoser | architecture overview | Passed |
| NFR-003 | Repair 调用链与数值/随机测试 | 完整 Release test | SPEC-0007 | Passed |
| NFR-004 | Generator 仅服务 Algorithms | generator 测试、项目引用审查 | ADR-0019 | Passed |

## 性能证据与回退

在 Apple M4 Pro、.NET 10.0.11 Arm64 RyuJIT、BenchmarkDotNet 0.15.8 上，对历史 Reflect SIMD 候选与独立纯标量参考进行了同机长测：10 次 warmup、30 次正式迭代、每迭代 100 万调用，覆盖 2、7、8、31、32、33、127、128、129、1024 维以及标量/标量、向量/向量。

候选没有稳定收益：标量/标量只有 7 维较快，32/128/1024 维分别慢约 18%/18%/23%；向量/向量仅 7、8、31 维略快，32/128/1024 维分别慢约 2%/2%/3%。因此按 FR-005 删除候选，而非保留没有可确认收益的复杂路径。完整 CSV、Markdown 报告与日志位于本机临时基线 checkout 的 `extended-reflect` 产物中；这些 M4 Pro 数值不与 9800X3D 的历史绝对纳秒比较。

同一环境和长测参数下，`ScalarClamp` 与生产 `TensorClamp` 的 40 个点都已完成。除 2、7 维的固定开销主导区间外，`TensorPrimitives.Clamp` 在 31 维及以上稳定获益：32 维约快 3.0×（标量端点）/3.2×（向量端点），128 维约快 7.0×/5.2×，1024 维约快 11.5×/6.6×；所有比较均为 0 B 分配。完整产物位于本机临时目录 `spec-0007-clamp-extended`。

## 工程验证

- 残留审查：`CandidateRepairs.simd.cs` 已删除；Core 项目不再引用生成器或 AdditionalFiles；`eng` 中无 `CandidateRepairs`/Core 关联。
- Release Build：`dotnet build Metaheuristics.NET.slnx -c Release --no-restore --disable-build-servers -m:1 -v:minimal`，0 warning / 0 error。
- Tests：`dotnet test Metaheuristics.NET.slnx -c Release --no-build -v:minimal`，166 passed / 0 failed。
- 差异检查：`git diff --check` 通过。
