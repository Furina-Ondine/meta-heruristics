```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |-----------:|----------:|----------:|------:|----------:|------------:|
| **BaselineFill**  | **32**     |  **31.092 ns** | **0.1093 ns** | **0.0853 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   8.464 ns | 0.0232 ns | 0.0181 ns |  0.27 |         - |          NA |
|               |        |            |           |           |       |           |             |
| **BaselineFill**  | **128**    | **123.387 ns** | **0.1845 ns** | **0.1440 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |  30.696 ns | 0.0411 ns | 0.0321 ns |  0.25 |         - |          NA |
