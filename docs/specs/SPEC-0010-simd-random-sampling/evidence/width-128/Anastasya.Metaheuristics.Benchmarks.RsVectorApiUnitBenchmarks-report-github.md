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
| **ScalarReferenceNext** | **32**     | **8.718 ns** | **0.1215 ns** | **0.0949 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     | 1.783 ns | 0.0012 ns | 0.0008 ns |  0.20 |         - |          NA |
|                     |        |          |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **8.696 ns** | **0.0773 ns** | **0.0604 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    | 1.779 ns | 0.0051 ns | 0.0040 ns |  0.20 |         - |          NA |
