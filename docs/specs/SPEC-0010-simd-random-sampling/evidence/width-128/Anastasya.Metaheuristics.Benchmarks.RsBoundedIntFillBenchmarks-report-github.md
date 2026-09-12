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
| **BaselineFill**  | **32**     |  **31.95 ns** | **0.028 ns** | **0.018 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  29.21 ns | 0.242 ns | 0.160 ns |  0.91 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **121.89 ns** | **2.019 ns** | **1.335 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 113.19 ns | 0.718 ns | 0.519 ns |  0.93 |         - |          NA |
