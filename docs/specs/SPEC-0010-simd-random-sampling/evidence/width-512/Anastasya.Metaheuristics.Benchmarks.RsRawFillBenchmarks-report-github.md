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
| **BaselineFill**        | **32**     |  **23.445 ns** | **0.0421 ns** | **0.0304 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   6.208 ns | 0.0042 ns | 0.0033 ns |  0.26 |         - |          NA |
| ScalarReferenceFill | 32     |  64.301 ns | 0.1362 ns | 0.0985 ns |  2.74 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **94.453 ns** | **0.0684 ns** | **0.0495 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  17.850 ns | 0.0172 ns | 0.0134 ns |  0.19 |         - |          NA |
| ScalarReferenceFill | 128    | 254.895 ns | 2.0078 ns | 1.3280 ns |  2.70 |         - |          NA |
