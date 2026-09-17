```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **20.46 ns** | **0.021 ns** | **0.014 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |  18.90 ns | 0.005 ns | 0.003 ns |  0.92 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  81.21 ns | 2.063 ns | 1.492 ns |  3.97 |    0.07 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **80.04 ns** | **0.621 ns** | **0.449 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 128    |  73.29 ns | 0.060 ns | 0.047 ns |  0.92 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 325.37 ns | 3.847 ns | 3.004 ns |  4.07 |    0.04 |         - |          NA |
