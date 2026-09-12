```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **20.45 ns** |  **0.030 ns** |  **0.018 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |  27.64 ns |  0.012 ns |  0.009 ns |  1.35 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  80.72 ns |  0.612 ns |  0.405 ns |  3.95 |    0.02 |         - |          NA |
|                     |        |           |           |           |       |         |           |             |
| **BaselineFill**        | **128**    |  **78.45 ns** |  **0.147 ns** |  **0.097 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    | 108.83 ns |  0.066 ns |  0.051 ns |  1.39 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 331.01 ns | 16.413 ns | 12.814 ns |  4.22 |    0.16 |         - |          NA |
