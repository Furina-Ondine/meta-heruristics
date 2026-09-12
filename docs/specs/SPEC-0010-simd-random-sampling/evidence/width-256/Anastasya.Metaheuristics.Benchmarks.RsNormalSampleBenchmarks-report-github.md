```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method          | Length | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|---------------- |------- |---------:|---------:|---------:|------:|----------:|------------:|
| **BaselineSample**  | **32**     | **13.50 ns** | **0.012 ns** | **0.009 ns** |  **1.00** |         **-** |          **NA** |
| CandidateSample | 32     | 13.09 ns | 0.010 ns | 0.006 ns |  0.97 |         - |          NA |
|                 |        |          |          |          |       |           |             |
| **BaselineSample**  | **128**    | **13.41 ns** | **0.029 ns** | **0.019 ns** |  **1.00** |         **-** |          **NA** |
| CandidateSample | 128    | 13.08 ns | 0.008 ns | 0.006 ns |  0.98 |         - |          NA |
