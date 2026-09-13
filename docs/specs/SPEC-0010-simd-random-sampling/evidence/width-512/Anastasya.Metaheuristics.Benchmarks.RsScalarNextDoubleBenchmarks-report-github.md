```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **BaselineNext**  | **32**     | **1.0356 ns** | **0.0476 ns** | **0.0371 ns** |  **1.00** |    **0.05** |         **-** |          **NA** |
| CandidateNext | 32     | 0.9966 ns | 0.0035 ns | 0.0028 ns |  0.96 |    0.03 |         - |          NA |
|               |        |           |           |           |       |         |           |             |
| **BaselineNext**  | **128**    | **0.9889 ns** | **0.0023 ns** | **0.0018 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateNext | 128    | 0.9891 ns | 0.0021 ns | 0.0016 ns |  1.00 |    0.00 |         - |          NA |
