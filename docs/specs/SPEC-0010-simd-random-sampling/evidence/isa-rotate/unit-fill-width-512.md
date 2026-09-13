```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean       | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |-----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **20.550 ns** | **0.0459 ns** | **0.0304 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   6.862 ns | 0.0220 ns | 0.0172 ns |  0.33 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  68.214 ns | 0.9441 ns | 0.6244 ns |  3.32 |    0.03 |         - |          NA |
|                     |        |            |           |           |       |         |           |             |
| **BaselineFill**        | **128**    |  **84.379 ns** | **1.8420 ns** | **1.3319 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CandidateFill       | 128    |  19.971 ns | 0.0438 ns | 0.0342 ns |  0.24 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 287.750 ns | 4.5366 ns | 3.5418 ns |  3.41 |    0.06 |         - |          NA |
