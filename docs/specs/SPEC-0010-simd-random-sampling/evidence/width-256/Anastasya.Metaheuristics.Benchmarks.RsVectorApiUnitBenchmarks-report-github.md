```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------- |------- |----------:|----------:|----------:|------:|----------:|------------:|
| **ScalarReferenceNext** | **32**     | **10.387 ns** | **0.0174 ns** | **0.0126 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     |  2.076 ns | 0.0016 ns | 0.0012 ns |  0.20 |         - |          NA |
|                     |        |           |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **10.638 ns** | **0.0124 ns** | **0.0089 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    |  2.076 ns | 0.0014 ns | 0.0009 ns |  0.20 |         - |          NA |
