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
| **BaselineFill**  | **32**     |   **258.4 ns** | **1.26 ns** | **0.83 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   110.9 ns | 0.45 ns | 0.35 ns |  0.43 |         - |          NA |
|               |        |            |         |         |       |           |             |
| **BaselineFill**  | **128**    | **1,013.8 ns** | **7.17 ns** | **4.74 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |   449.6 ns | 1.80 ns | 1.41 ns |  0.44 |         - |          NA |
