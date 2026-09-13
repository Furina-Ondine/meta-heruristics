```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **BaselineFill**  | **32**     |  **32.05 ns** | **0.040 ns** | **0.027 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill | 32     |  32.94 ns | 0.671 ns | 0.524 ns |  1.03 |    0.02 |         - |          NA |
|               |        |           |          |          |       |         |           |             |
| **BaselineFill**  | **128**    | **128.13 ns** | **1.348 ns** | **1.052 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill | 128    | 121.73 ns | 1.262 ns | 0.835 ns |  0.95 |    0.01 |         - |          NA |
