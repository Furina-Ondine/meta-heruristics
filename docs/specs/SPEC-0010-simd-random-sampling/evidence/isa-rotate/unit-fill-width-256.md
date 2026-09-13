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
| **BaselineFill**        | **32**     |  **20.47 ns** | **0.029 ns** | **0.021 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |  11.06 ns | 0.025 ns | 0.019 ns |  0.54 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  68.90 ns | 0.144 ns | 0.096 ns |  3.37 |    0.01 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **78.02 ns** | **0.041 ns** | **0.030 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  36.95 ns | 0.010 ns | 0.007 ns |  0.47 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 278.24 ns | 5.026 ns | 3.924 ns |  3.57 |    0.05 |         - |          NA |
