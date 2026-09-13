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
| **BaselineFill**        | **32**     |  **19.44 ns** | **0.460 ns** | **0.359 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CandidateFill       | 32     |  16.98 ns | 0.018 ns | 0.012 ns |  0.87 |    0.02 |         - |          NA |
| ScalarReferenceFill | 32     |  84.02 ns | 0.241 ns | 0.174 ns |  4.32 |    0.08 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **73.12 ns** | **0.133 ns** | **0.096 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  64.54 ns | 0.046 ns | 0.036 ns |  0.88 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 301.06 ns | 0.715 ns | 0.473 ns |  4.12 |    0.01 |         - |          NA |
