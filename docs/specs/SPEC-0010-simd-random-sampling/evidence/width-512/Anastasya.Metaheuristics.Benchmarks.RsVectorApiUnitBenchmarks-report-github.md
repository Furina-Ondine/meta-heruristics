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
| **ScalarReferenceNext** | **32**     | **15.535 ns** | **0.0246 ns** | **0.0192 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     |  3.337 ns | 0.0009 ns | 0.0006 ns |  0.21 |         - |          NA |
|                     |        |           |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **15.926 ns** | **0.0736 ns** | **0.0575 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    |  3.176 ns | 0.0046 ns | 0.0036 ns |  0.20 |         - |          NA |
