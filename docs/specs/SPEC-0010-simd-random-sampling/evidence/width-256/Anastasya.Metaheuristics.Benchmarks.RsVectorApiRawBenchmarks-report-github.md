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
| **ScalarReferenceNext** | **32**     | **9.264 ns** | **0.0134 ns** | **0.0097 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     | 2.090 ns | 0.0013 ns | 0.0010 ns |  0.23 |         - |          NA |
|                     |        |          |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **9.084 ns** | **0.0062 ns** | **0.0045 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    | 2.091 ns | 0.0034 ns | 0.0026 ns |  0.23 |         - |          NA |
