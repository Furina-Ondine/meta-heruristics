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
| **BaselineFill**  | **32**     | **252.75 ns** | **2.147 ns** | **1.676 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  45.09 ns | 0.155 ns | 0.121 ns |  0.18 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **986.07 ns** | **1.468 ns** | **0.971 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 169.02 ns | 0.261 ns | 0.173 ns |  0.17 |         - |          NA |
