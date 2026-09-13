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
| **BaselineNext**  | **32**     | **1.316 ns** | **0.0088 ns** | **0.0064 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext | 32     | 1.370 ns | 0.0092 ns | 0.0072 ns |  1.04 |         - |          NA |
|               |        |          |           |           |       |           |             |
| **BaselineNext**  | **128**    | **1.312 ns** | **0.0047 ns** | **0.0037 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext | 128    | 1.325 ns | 0.0047 ns | 0.0037 ns |  1.01 |         - |          NA |
