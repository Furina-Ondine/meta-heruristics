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
| **BaselineFill**        | **32**     |  **19.27 ns** | **0.234 ns** | **0.183 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  25.00 ns | 0.059 ns | 0.043 ns |  1.30 |    0.01 |         - |          NA |
| ScalarReferenceFill | 32     |  74.61 ns | 0.121 ns | 0.088 ns |  3.87 |    0.04 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **73.01 ns** | **0.229 ns** | **0.166 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  97.15 ns | 0.286 ns | 0.223 ns |  1.33 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 318.07 ns | 5.903 ns | 4.269 ns |  4.36 |    0.06 |         - |          NA |
