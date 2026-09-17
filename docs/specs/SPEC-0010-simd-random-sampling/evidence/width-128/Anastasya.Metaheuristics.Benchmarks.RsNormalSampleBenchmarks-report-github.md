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
| **BaselineSample**  | **32**     | **13.46 ns** | **0.032 ns** | **0.021 ns** |  **1.00** |         **-** |          **NA** |
| CandidateSample | 32     | 13.05 ns | 0.011 ns | 0.007 ns |  0.97 |         - |          NA |
|                 |        |          |          |          |       |           |             |
| **BaselineSample**  | **128**    | **13.58 ns** | **0.051 ns** | **0.040 ns** |  **1.00** |         **-** |          **NA** |
| CandidateSample | 128    | 13.05 ns | 0.007 ns | 0.004 ns |  0.96 |         - |          NA |
