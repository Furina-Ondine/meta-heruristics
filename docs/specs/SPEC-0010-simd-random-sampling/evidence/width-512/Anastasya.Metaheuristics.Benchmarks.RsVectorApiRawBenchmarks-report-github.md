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
| **ScalarReferenceNext** | **32**     | **14.646 ns** | **0.0080 ns** | **0.0048 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     |  3.294 ns | 0.0020 ns | 0.0014 ns |  0.22 |         - |          NA |
|                     |        |           |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **14.382 ns** | **0.0198 ns** | **0.0155 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    |  3.294 ns | 0.0015 ns | 0.0011 ns |  0.23 |         - |          NA |
