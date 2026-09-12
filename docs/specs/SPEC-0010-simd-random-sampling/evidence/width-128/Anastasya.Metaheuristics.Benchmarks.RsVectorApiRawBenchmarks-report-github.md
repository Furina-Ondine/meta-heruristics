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
| **ScalarReferenceNext** | **32**     | **7.392 ns** | **0.1021 ns** | **0.0797 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     | 2.298 ns | 0.0006 ns | 0.0004 ns |  0.31 |         - |          NA |
|                     |        |          |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **7.259 ns** | **0.0702 ns** | **0.0548 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    | 2.299 ns | 0.0031 ns | 0.0024 ns |  0.32 |         - |          NA |
