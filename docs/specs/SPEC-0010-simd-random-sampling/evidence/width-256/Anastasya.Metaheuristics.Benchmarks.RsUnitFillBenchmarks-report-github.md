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
| **BaselineFill**        | **32**     |  **20.67 ns** | **0.292 ns** | **0.228 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  11.06 ns | 0.009 ns | 0.007 ns |  0.54 |    0.01 |         - |          NA |
| ScalarReferenceFill | 32     |  68.87 ns | 0.131 ns | 0.078 ns |  3.33 |    0.04 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **79.11 ns** | **0.068 ns** | **0.049 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  36.86 ns | 0.017 ns | 0.014 ns |  0.47 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 273.05 ns | 0.679 ns | 0.530 ns |  3.45 |    0.01 |         - |          NA |
