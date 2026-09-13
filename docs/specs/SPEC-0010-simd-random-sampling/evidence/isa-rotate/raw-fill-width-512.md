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
| **BaselineFill**        | **32**     |  **23.412 ns** | **0.0284 ns** | **0.0188 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   6.187 ns | 0.0049 ns | 0.0035 ns |  0.26 |         - |          NA |
| ScalarReferenceFill | 32     |  64.022 ns | 0.1229 ns | 0.0889 ns |  2.73 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **94.423 ns** | **0.1508 ns** | **0.1178 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  18.594 ns | 0.0146 ns | 0.0114 ns |  0.20 |         - |          NA |
| ScalarReferenceFill | 128    | 251.382 ns | 0.7168 ns | 0.4266 ns |  2.66 |         - |          NA |
