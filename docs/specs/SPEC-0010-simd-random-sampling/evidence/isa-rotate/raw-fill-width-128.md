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
| **BaselineFill**        | **32**     |  **20.52 ns** | **0.115 ns** | **0.083 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  17.05 ns | 0.031 ns | 0.022 ns |  0.83 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  82.81 ns | 1.425 ns | 1.113 ns |  4.04 |    0.05 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **76.13 ns** | **0.419 ns** | **0.327 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 128    |  64.48 ns | 0.113 ns | 0.082 ns |  0.85 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 300.43 ns | 2.698 ns | 1.951 ns |  3.95 |    0.03 |         - |          NA |
