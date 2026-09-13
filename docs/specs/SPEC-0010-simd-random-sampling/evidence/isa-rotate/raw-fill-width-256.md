```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **19.82 ns** | **0.336 ns** | **0.263 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CandidateFill       | 32     |  13.96 ns | 0.194 ns | 0.151 ns |  0.70 |    0.01 |         - |          NA |
| ScalarReferenceFill | 32     |  73.67 ns | 5.954 ns | 4.649 ns |  3.72 |    0.23 |         - |          NA |
|                     |        |           |          |          |       |         |           |             |
| **BaselineFill**        | **128**    |  **74.97 ns** | **2.343 ns** | **1.830 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CandidateFill       | 128    |  49.01 ns | 0.230 ns | 0.179 ns |  0.65 |    0.02 |         - |          NA |
| ScalarReferenceFill | 128    | 254.99 ns | 0.466 ns | 0.308 ns |  3.40 |    0.08 |         - |          NA |
