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
| **BaselineFill**  | **32**     |  **33.31 ns** | **0.236 ns** | **0.184 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |  33.08 ns | 0.407 ns | 0.294 ns |  0.99 |         - |          NA |
|               |        |           |          |          |       |           |             |
| **BaselineFill**  | **128**    | **126.01 ns** | **0.756 ns** | **0.547 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    | 122.80 ns | 2.095 ns | 1.636 ns |  0.97 |         - |          NA |
