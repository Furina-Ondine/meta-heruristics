```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |---------:|----------:|----------:|------:|----------:|------------:|
| **BaselineNext**  | **32**     | **1.310 ns** | **0.0061 ns** | **0.0044 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext | 32     | 1.310 ns | 0.0061 ns | 0.0044 ns |  1.00 |         - |          NA |
|               |        |          |           |           |       |           |             |
| **BaselineNext**  | **128**    | **1.302 ns** | **0.0062 ns** | **0.0048 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext | 128    | 1.304 ns | 0.0049 ns | 0.0038 ns |  1.00 |         - |          NA |
