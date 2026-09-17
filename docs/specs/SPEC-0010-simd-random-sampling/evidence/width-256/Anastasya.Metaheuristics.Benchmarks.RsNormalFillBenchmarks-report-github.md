```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean        | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |------------:|---------:|---------:|------:|----------:|------------:|
| **BaselineFill**  | **32**     |   **257.28 ns** | **2.008 ns** | **1.452 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |    58.07 ns | 0.191 ns | 0.138 ns |  0.23 |         - |          NA |
|               |        |             |          |          |       |           |             |
| **BaselineFill**  | **128**    | **1,091.16 ns** | **6.149 ns** | **4.446 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |   231.77 ns | 0.803 ns | 0.580 ns |  0.21 |         - |          NA |
