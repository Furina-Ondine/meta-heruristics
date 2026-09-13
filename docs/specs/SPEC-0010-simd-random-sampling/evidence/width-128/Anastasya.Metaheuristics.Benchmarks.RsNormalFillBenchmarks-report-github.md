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
| **BaselineFill**  | **32**     |   **256.9 ns** | **0.49 ns** | **0.32 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   423.9 ns | 0.21 ns | 0.14 ns |  1.65 |         - |          NA |
|               |        |            |         |         |       |           |             |
| **BaselineFill**  | **128**    | **1,011.4 ns** | **8.20 ns** | **5.93 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 1,676.4 ns | 1.03 ns | 0.75 ns |  1.66 |         - |          NA |
