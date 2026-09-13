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
| **BaselineFill**        | **32**     |  **20.469 ns** | **0.0232 ns** | **0.0181 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   6.927 ns | 0.0023 ns | 0.0017 ns |  0.34 |         - |          NA |
| ScalarReferenceFill | 32     |  67.884 ns | 0.1039 ns | 0.0811 ns |  3.32 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **78.771 ns** | **0.0824 ns** | **0.0545 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  19.720 ns | 0.0095 ns | 0.0063 ns |  0.25 |         - |          NA |
| ScalarReferenceFill | 128    | 265.406 ns | 0.5193 ns | 0.3755 ns |  3.37 |         - |          NA |
