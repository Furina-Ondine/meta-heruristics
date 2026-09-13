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
| **BaselineNext**  | **32**     | **1.003 ns** | **0.0022 ns** | **0.0017 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateNext | 32     | 1.002 ns | 0.0024 ns | 0.0019 ns |  1.00 |    0.00 |         - |          NA |
|               |        |          |           |           |       |         |           |             |
| **BaselineNext**  | **128**    | **1.042 ns** | **0.0342 ns** | **0.0267 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CandidateNext | 128    | 1.033 ns | 0.0056 ns | 0.0044 ns |  0.99 |    0.02 |         - |          NA |
