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
| **BaselineFill**  | **32**     |   **256.5 ns** | **0.39 ns** | **0.29 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   233.8 ns | 0.17 ns | 0.13 ns |  0.91 |         - |          NA |
|               |        |            |         |         |       |           |             |
| **BaselineFill**  | **128**    | **1,008.6 ns** | **2.20 ns** | **1.59 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |   889.3 ns | 0.67 ns | 0.52 ns |  0.88 |         - |          NA |
