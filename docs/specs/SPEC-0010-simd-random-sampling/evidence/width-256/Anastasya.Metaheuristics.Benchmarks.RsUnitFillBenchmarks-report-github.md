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
| **BaselineFill**        | **32**     |  **20.72 ns** | **0.162 ns** | **0.107 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  11.05 ns | 0.031 ns | 0.024 ns |  0.53 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  70.02 ns | 0.177 ns | 0.117 ns |  3.38 |    0.02 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **78.39 ns** | **0.117 ns** | **0.085 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  37.24 ns | 0.009 ns | 0.006 ns |  0.48 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 275.99 ns | 0.631 ns | 0.417 ns |  3.52 |    0.01 |         - |          NA |
