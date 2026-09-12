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
| **BaselineNext**  | **32**     | **0.9788 ns** | **0.0074 ns** | **0.0054 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateNext | 32     | 1.0638 ns | 0.0265 ns | 0.0207 ns |  1.09 |    0.02 |         - |          NA |
|               |        |           |           |           |       |         |           |             |
| **BaselineNext**  | **128**    | **0.9991 ns** | **0.0203 ns** | **0.0147 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CandidateNext | 128    | 0.9895 ns | 0.0096 ns | 0.0075 ns |  0.99 |    0.02 |         - |          NA |
