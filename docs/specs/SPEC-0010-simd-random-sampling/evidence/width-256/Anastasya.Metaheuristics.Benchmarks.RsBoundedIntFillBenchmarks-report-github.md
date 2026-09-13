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
| **BaselineFill**  | **32**     |  **33.54 ns** | **0.156 ns** | **0.122 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  32.66 ns | 0.417 ns | 0.325 ns |  0.97 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **127.27 ns** | **0.892 ns** | **0.645 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 122.89 ns | 1.802 ns | 1.407 ns |  0.97 |         - |          NA |
