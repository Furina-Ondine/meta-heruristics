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
| **ScalarReferenceNext** | **32**     | **8.570 ns** | **0.0361 ns** | **0.0239 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     | 2.269 ns | 0.0037 ns | 0.0029 ns |  0.26 |         - |          NA |
|                     |        |          |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **8.522 ns** | **0.0044 ns** | **0.0029 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    | 2.275 ns | 0.0032 ns | 0.0025 ns |  0.27 |         - |          NA |
