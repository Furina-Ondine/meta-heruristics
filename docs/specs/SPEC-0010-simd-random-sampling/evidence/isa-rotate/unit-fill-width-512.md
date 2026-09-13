```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean       | Error      | StdDev     | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |-----------:|-----------:|-----------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **20.586 ns** |  **0.1091 ns** |  **0.0789 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |   7.255 ns |  0.1598 ns |  0.1156 ns |  0.35 |    0.01 |         - |          NA |
| ScalarReferenceFill | 32     |  72.005 ns |  5.1839 ns |  3.7483 ns |  3.50 |    0.17 |         - |          NA |
|                     |        |            |            |            |       |         |           |             |
| **BaselineFill**        | **128**    |  **88.434 ns** | **10.5159 ns** |  **8.2101 ns** |  **1.01** |    **0.12** |         **-** |          **NA** |
| CandidateFill       | 128    |  19.952 ns |  0.1057 ns |  0.0764 ns |  0.23 |    0.02 |         - |          NA |
| ScalarReferenceFill | 128    | 313.924 ns | 47.6729 ns | 37.2199 ns |  3.58 |    0.51 |         - |          NA |
