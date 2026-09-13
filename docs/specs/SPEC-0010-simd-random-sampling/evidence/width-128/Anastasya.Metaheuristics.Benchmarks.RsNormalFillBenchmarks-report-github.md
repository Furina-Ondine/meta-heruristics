```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |---------:|--------:|--------:|------:|----------:|------------:|
| **BaselineFill**  | **32**     | **250.9 ns** | **0.18 ns** | **0.13 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     | 112.5 ns | 0.07 ns | 0.05 ns |  0.45 |         - |          NA |
|               |        |          |         |         |       |           |             |
| **BaselineFill**  | **128**    | **987.1 ns** | **1.34 ns** | **1.04 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 449.3 ns | 0.75 ns | 0.59 ns |  0.46 |         - |          NA |
