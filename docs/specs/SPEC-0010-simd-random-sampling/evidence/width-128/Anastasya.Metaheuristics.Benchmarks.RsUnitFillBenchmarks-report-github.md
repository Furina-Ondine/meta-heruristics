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
| **BaselineFill**        | **32**     |  **21.16 ns** | **0.904 ns** | **0.706 ns** |  **1.00** |    **0.04** |         **-** |          **NA** |
| CandidateFill       | 32     |  18.77 ns | 0.004 ns | 0.003 ns |  0.89 |    0.03 |         - |          NA |
| ScalarReferenceFill | 32     |  81.13 ns | 0.753 ns | 0.545 ns |  3.84 |    0.12 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **78.20 ns** | **0.084 ns** | **0.056 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  73.21 ns | 0.025 ns | 0.018 ns |  0.94 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 324.51 ns | 4.614 ns | 3.602 ns |  4.15 |    0.04 |         - |          NA |
