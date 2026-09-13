```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **20.61 ns** |  **0.225 ns** |  **0.149 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill       | 32     |  18.77 ns |  0.009 ns |  0.007 ns |  0.91 |    0.01 |         - |          NA |
| ScalarReferenceFill | 32     |  81.61 ns |  1.772 ns |  1.282 ns |  3.96 |    0.07 |         - |          NA |
|                     |        |           |           |           |       |         |           |             |
| **BaselineFill**        | **128**    |  **79.86 ns** |  **1.920 ns** |  **1.499 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CandidateFill       | 128    |  73.26 ns |  0.038 ns |  0.030 ns |  0.92 |    0.02 |         - |          NA |
| ScalarReferenceFill | 128    | 335.73 ns | 18.472 ns | 14.422 ns |  4.21 |    0.19 |         - |          NA |
