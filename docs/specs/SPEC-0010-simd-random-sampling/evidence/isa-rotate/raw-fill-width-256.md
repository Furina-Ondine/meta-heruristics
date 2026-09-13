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
| **BaselineFill**        | **32**     |  **19.091 ns** | **0.0193 ns** | **0.0139 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   8.999 ns | 0.0082 ns | 0.0064 ns |  0.47 |    0.00 |         - |          NA |
| ScalarReferenceFill | 32     |  64.855 ns | 1.3775 ns | 1.0755 ns |  3.40 |    0.05 |         - |          NA |
|                     |        |            |           |           |       |         |           |             |
| **BaselineFill**        | **128**    |  **73.212 ns** | **0.7344 ns** | **0.5733 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 128    |  32.834 ns | 0.0220 ns | 0.0172 ns |  0.45 |    0.00 |         - |          NA |
| ScalarReferenceFill | 128    | 254.168 ns | 1.0393 ns | 0.7515 ns |  3.47 |    0.03 |         - |          NA |
