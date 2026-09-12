```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean      | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |----------:|---------:|---------:|------:|----------:|------------:|
| **BaselineFill**  | **32**     |  **31.88 ns** | **0.016 ns** | **0.012 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  29.48 ns | 0.038 ns | 0.030 ns |  0.92 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **121.47 ns** | **0.070 ns** | **0.046 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 116.23 ns | 0.277 ns | 0.200 ns |  0.96 |         - |          NA |
