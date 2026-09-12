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
| **BaselineFill**  | **32**     |   **254.5 ns** | **0.17 ns** | **0.13 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   188.6 ns | 0.21 ns | 0.16 ns |  0.74 |         - |          NA |
|               |        |            |         |         |       |           |             |
| **BaselineFill**  | **128**    | **1,001.2 ns** | **1.12 ns** | **0.87 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |   910.7 ns | 0.19 ns | 0.15 ns |  0.91 |         - |          NA |
