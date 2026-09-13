```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean       | Error   | StdDev  | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |------- |-----------:|--------:|--------:|------:|--------:|----------:|------------:|
| **BaselineFill**  | **32**     |   **262.8 ns** | **3.29 ns** | **2.57 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill | 32     |   426.3 ns | 0.40 ns | 0.31 ns |  1.62 |    0.02 |         - |          NA |
|               |        |            |         |         |       |         |           |             |
| **BaselineFill**  | **128**    | **1,031.3 ns** | **7.91 ns** | **6.17 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill | 128    | 1,682.3 ns | 2.61 ns | 1.89 ns |  1.63 |    0.01 |         - |          NA |
