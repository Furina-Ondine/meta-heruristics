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
| **BaselineFill**  | **32**     |  **31.89 ns** | **0.018 ns** | **0.013 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  29.08 ns | 0.051 ns | 0.040 ns |  0.91 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **121.20 ns** | **0.090 ns** | **0.065 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 110.47 ns | 0.104 ns | 0.075 ns |  0.91 |         - |          NA |
