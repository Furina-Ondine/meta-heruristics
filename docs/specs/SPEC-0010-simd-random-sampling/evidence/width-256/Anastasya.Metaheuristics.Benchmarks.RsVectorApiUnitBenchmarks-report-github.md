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
| **ScalarReferenceNext** | **32**     | **10.401 ns** | **0.0122 ns** | **0.0088 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     |  2.081 ns | 0.0078 ns | 0.0056 ns |  0.20 |         - |          NA |
|                     |        |           |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **10.655 ns** | **0.0171 ns** | **0.0124 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    |  2.077 ns | 0.0027 ns | 0.0021 ns |  0.19 |         - |          NA |
