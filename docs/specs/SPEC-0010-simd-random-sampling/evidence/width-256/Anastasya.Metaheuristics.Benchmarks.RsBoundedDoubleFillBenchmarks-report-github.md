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
| **BaselineFill**  | **32**     |  **33.56 ns** | **0.514 ns** | **0.340 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  12.60 ns | 0.165 ns | 0.129 ns |  0.38 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **129.63 ns** | **1.922 ns** | **1.390 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |  45.95 ns | 0.342 ns | 0.267 ns |  0.35 |         - |          NA |
