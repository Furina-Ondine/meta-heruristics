```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean       | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |-----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **23.979 ns** | **0.2442 ns** | **0.1766 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |   6.633 ns | 0.1227 ns | 0.0958 ns |  0.28 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  70.896 ns | 4.5413 ns | 3.5456 ns |  2.96 |    0.14 |         - |          NA |
|                     |        |            |           |           |       |         |           |             |
| **BaselineFill**        | **128**    |  **94.585 ns** | **0.2574 ns** | **0.1861 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  17.946 ns | 0.0317 ns | 0.0248 ns |  0.19 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 252.747 ns | 0.4990 ns | 0.3300 ns |  2.67 |    0.01 |         - |          NA |
