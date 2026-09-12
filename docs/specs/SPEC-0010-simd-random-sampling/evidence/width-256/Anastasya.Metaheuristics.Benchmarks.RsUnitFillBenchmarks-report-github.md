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
| **BaselineFill**        | **32**     |  **20.69 ns** | **0.165 ns** | **0.129 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  14.11 ns | 0.015 ns | 0.011 ns |  0.68 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  70.72 ns | 1.519 ns | 1.186 ns |  3.42 |    0.06 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **78.11 ns** | **0.135 ns** | **0.080 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  54.72 ns | 0.060 ns | 0.047 ns |  0.70 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 277.94 ns | 4.626 ns | 3.612 ns |  3.56 |    0.04 |         - |          NA |
