```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------- |------- |-----------:|----------:|----------:|------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **23.387 ns** | **0.0135 ns** | **0.0097 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   7.969 ns | 0.0196 ns | 0.0153 ns |  0.34 |         - |          NA |
| ScalarReferenceFill | 32     |  63.802 ns | 0.1006 ns | 0.0727 ns |  2.73 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **94.136 ns** | **0.0524 ns** | **0.0347 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  25.887 ns | 0.0718 ns | 0.0519 ns |  0.27 |         - |          NA |
| ScalarReferenceFill | 128    | 254.434 ns | 0.7585 ns | 0.5485 ns |  2.70 |         - |          NA |
