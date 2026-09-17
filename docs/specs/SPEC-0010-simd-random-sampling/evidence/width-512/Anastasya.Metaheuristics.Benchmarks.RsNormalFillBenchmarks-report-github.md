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
| **BaselineFill**  | **32**     |   **259.08 ns** | **1.889 ns** | **1.250 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |    34.94 ns | 0.173 ns | 0.114 ns |  0.13 |         - |          NA |
|               |        |             |          |          |       |           |             |
| **BaselineFill**  | **128**    | **1,004.35 ns** | **5.358 ns** | **3.544 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |   136.26 ns | 1.395 ns | 1.089 ns |  0.14 |         - |          NA |
