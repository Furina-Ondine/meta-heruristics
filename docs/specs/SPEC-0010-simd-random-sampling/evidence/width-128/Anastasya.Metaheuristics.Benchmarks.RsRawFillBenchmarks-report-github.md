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
| **BaselineFill**        | **32**     |  **19.80 ns** | **0.181 ns** | **0.120 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  25.24 ns | 0.083 ns | 0.065 ns |  1.27 |    0.01 |         - |          NA |
| ScalarReferenceFill | 32     |  74.89 ns | 0.276 ns | 0.199 ns |  3.78 |    0.02 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **72.90 ns** | **0.129 ns** | **0.093 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  97.60 ns | 0.341 ns | 0.267 ns |  1.34 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 298.83 ns | 0.595 ns | 0.430 ns |  4.10 |    0.01 |         - |          NA |
