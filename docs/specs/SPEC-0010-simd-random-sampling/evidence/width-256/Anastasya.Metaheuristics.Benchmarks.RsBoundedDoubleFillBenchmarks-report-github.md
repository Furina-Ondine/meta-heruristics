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
| **BaselineFill**  | **32**     |  **32.81 ns** | **0.751 ns** | **0.586 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CandidateFill | 32     |  11.90 ns | 0.117 ns | 0.091 ns |  0.36 |    0.01 |         - |          NA |
|               |        |           |          |          |       |         |           |             |
| **BaselineFill**  | **128**    | **122.17 ns** | **1.321 ns** | **1.031 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill | 128    |  45.66 ns | 0.081 ns | 0.058 ns |  0.37 |    0.00 |         - |          NA |
