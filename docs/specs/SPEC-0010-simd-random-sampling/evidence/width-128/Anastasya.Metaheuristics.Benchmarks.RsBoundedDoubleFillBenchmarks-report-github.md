```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **BaselineFill**  | **32**     |  **32.26 ns** | **1.410 ns** | **1.020 ns** |  **1.00** |    **0.04** |         **-** |          **NA** |
| CandidateFill | 32     |  30.30 ns | 0.055 ns | 0.043 ns |  0.94 |    0.03 |         - |          NA |
|               |        |           |          |          |       |         |           |             |
| **BaselineFill**  | **128**    | **123.64 ns** | **0.655 ns** | **0.474 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CandidateFill | 128    | 121.52 ns | 0.144 ns | 0.113 ns |  0.98 |    0.00 |         - |          NA |
