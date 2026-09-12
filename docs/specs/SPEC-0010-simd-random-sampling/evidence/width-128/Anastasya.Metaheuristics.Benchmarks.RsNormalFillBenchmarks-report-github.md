```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean       | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |-----------:|--------:|--------:|------:|----------:|------------:|
| **BaselineFill**  | **32**     |   **255.3 ns** | **0.59 ns** | **0.42 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   424.9 ns | 0.18 ns | 0.12 ns |  1.66 |         - |          NA |
|               |        |            |         |         |       |           |             |
| **BaselineFill**  | **128**    | **1,000.9 ns** | **1.03 ns** | **0.81 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 1,680.2 ns | 0.18 ns | 0.13 ns |  1.68 |         - |          NA |
