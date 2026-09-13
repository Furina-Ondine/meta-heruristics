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
| **BaselineFill**        | **32**     |  **19.103 ns** | **0.0469 ns** | **0.0310 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   9.115 ns | 0.0062 ns | 0.0041 ns |  0.48 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  64.485 ns | 0.2717 ns | 0.1965 ns |  3.38 |    0.01 |         - |          NA |
|                     |        |            |           |           |       |         |           |             |
| **BaselineFill**        | **128**    |  **73.277 ns** | **0.4672 ns** | **0.3378 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 128    |  33.279 ns | 0.0348 ns | 0.0271 ns |  0.45 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 255.452 ns | 0.8417 ns | 0.5567 ns |  3.49 |    0.02 |         - |          NA |
