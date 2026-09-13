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
| **BaselineFill**  | **32**     | **251.38 ns** | **0.288 ns** | **0.225 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  64.20 ns | 0.124 ns | 0.090 ns |  0.26 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **986.81 ns** | **0.919 ns** | **0.665 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 244.90 ns | 0.384 ns | 0.300 ns |  0.25 |         - |          NA |
