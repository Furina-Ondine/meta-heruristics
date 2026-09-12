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
| **BaselineFill**  | **32**     |  **31.01 ns** | **0.044 ns** | **0.032 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  15.60 ns | 0.006 ns | 0.005 ns |  0.50 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **120.16 ns** | **0.146 ns** | **0.106 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |  60.69 ns | 0.045 ns | 0.035 ns |  0.51 |         - |          NA |
