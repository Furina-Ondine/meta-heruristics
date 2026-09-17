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
| **BaselineFill**        | **32**     |  **23.38 ns** | **0.010 ns** | **0.007 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |  16.95 ns | 0.009 ns | 0.007 ns |  0.73 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  80.22 ns | 0.606 ns | 0.473 ns |  3.43 |    0.02 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **94.12 ns** | **0.032 ns** | **0.025 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  64.73 ns | 0.042 ns | 0.028 ns |  0.69 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 296.87 ns | 0.463 ns | 0.361 ns |  3.15 |    0.00 |         - |          NA |
