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
| **BaselineFill**        | **32**     |  **19.068 ns** | **0.0386 ns** | **0.0301 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   8.992 ns | 0.0067 ns | 0.0048 ns |  0.47 |         - |          NA |
| ScalarReferenceFill | 32     |  64.421 ns | 0.1487 ns | 0.1161 ns |  3.38 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **73.417 ns** | **0.0672 ns** | **0.0444 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  32.849 ns | 0.0199 ns | 0.0156 ns |  0.45 |         - |          NA |
| ScalarReferenceFill | 128    | 253.460 ns | 0.3872 ns | 0.2800 ns |  3.45 |         - |          NA |
