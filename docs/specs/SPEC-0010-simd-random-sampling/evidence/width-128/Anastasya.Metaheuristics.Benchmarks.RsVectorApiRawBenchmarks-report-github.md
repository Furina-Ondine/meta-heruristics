```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------- |------- |---------:|----------:|----------:|------:|----------:|------------:|
| **ScalarReferenceNext** | **32**     | **7.319 ns** | **0.0530 ns** | **0.0414 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     | 1.854 ns | 0.0034 ns | 0.0025 ns |  0.25 |         - |          NA |
|                     |        |          |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **7.248 ns** | **0.0154 ns** | **0.0111 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    | 1.871 ns | 0.0025 ns | 0.0019 ns |  0.26 |         - |          NA |
