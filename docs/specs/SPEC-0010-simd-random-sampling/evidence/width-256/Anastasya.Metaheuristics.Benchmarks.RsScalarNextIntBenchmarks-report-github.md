```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **BaselineNext**  | **32**     | **1.422 ns** | **0.1768 ns** | **0.1380 ns** |  **1.01** |    **0.13** |         **-** |          **NA** |
| CandidateNext | 32     | 1.261 ns | 0.0054 ns | 0.0036 ns |  0.89 |    0.08 |         - |          NA |
|               |        |          |           |           |       |         |           |             |
| **BaselineNext**  | **128**    | **1.274 ns** | **0.0407 ns** | **0.0317 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CandidateNext | 128    | 1.315 ns | 0.0067 ns | 0.0052 ns |  1.03 |    0.02 |         - |          NA |
