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
| **BaselineFill**        | **32**     |  **22.37 ns** | **2.129 ns** | **1.662 ns** |  **1.01** |    **0.10** |         **-** |          **NA** |
| CandidateFill       | 32     |  24.99 ns | 0.083 ns | 0.065 ns |  1.12 |    0.08 |         - |          NA |
| ScalarReferenceFill | 32     |  76.92 ns | 1.785 ns | 1.290 ns |  3.46 |    0.26 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **73.06 ns** | **0.153 ns** | **0.111 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  97.90 ns | 0.253 ns | 0.183 ns |  1.34 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 300.39 ns | 0.614 ns | 0.406 ns |  4.11 |    0.01 |         - |          NA |
