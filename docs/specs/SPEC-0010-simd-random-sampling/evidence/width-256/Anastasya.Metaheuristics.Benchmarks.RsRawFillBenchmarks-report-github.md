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
| **BaselineFill**        | **32**     |  **19.82 ns** | **1.177 ns** | **0.919 ns** |  **1.00** |    **0.06** |         **-** |          **NA** |
| CandidateFill       | 32     |  14.10 ns | 0.104 ns | 0.082 ns |  0.71 |    0.03 |         - |          NA |
| ScalarReferenceFill | 32     |  65.42 ns | 2.154 ns | 1.682 ns |  3.31 |    0.16 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **73.28 ns** | **0.895 ns** | **0.647 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 128    |  49.06 ns | 0.158 ns | 0.114 ns |  0.67 |    0.01 |         - |          NA |
| ScalarReferenceFill | 128    | 254.48 ns | 1.769 ns | 1.170 ns |  3.47 |    0.03 |         - |          NA |
