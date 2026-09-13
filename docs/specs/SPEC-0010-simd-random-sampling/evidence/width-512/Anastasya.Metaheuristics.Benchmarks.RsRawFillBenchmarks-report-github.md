```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------- |------- |-----------:|----------:|----------:|------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **19.018 ns** | **0.0167 ns** | **0.0121 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   5.857 ns | 0.0077 ns | 0.0060 ns |  0.31 |         - |          NA |
| ScalarReferenceFill | 32     |  64.134 ns | 0.1259 ns | 0.0911 ns |  3.37 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **72.891 ns** | **0.0588 ns** | **0.0389 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  17.873 ns | 0.0160 ns | 0.0116 ns |  0.25 |         - |          NA |
| ScalarReferenceFill | 128    | 252.023 ns | 0.3416 ns | 0.2260 ns |  3.46 |         - |          NA |
