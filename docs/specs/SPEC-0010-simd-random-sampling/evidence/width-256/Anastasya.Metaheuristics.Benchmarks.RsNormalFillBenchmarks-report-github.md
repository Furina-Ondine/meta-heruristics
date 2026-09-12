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
| **BaselineFill**  | **32**     | **255.0 ns** | **0.34 ns** | **0.26 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     | 228.0 ns | 0.11 ns | 0.09 ns |  0.89 |         - |          NA |
|               |        |          |         |         |       |           |             |
| **BaselineFill**  | **128**    | **975.7 ns** | **1.70 ns** | **1.33 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 911.0 ns | 0.11 ns | 0.08 ns |  0.93 |         - |          NA |
