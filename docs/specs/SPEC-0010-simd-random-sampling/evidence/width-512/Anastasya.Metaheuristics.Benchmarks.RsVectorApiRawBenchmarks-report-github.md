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
| **ScalarReferenceNext** | **32**     | **14.703 ns** | **0.0097 ns** | **0.0070 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 32     |  3.178 ns | 0.0020 ns | 0.0015 ns |  0.22 |         - |          NA |
|                     |        |           |           |           |       |           |             |
| **ScalarReferenceNext** | **128**    | **14.703 ns** | **0.0241 ns** | **0.0159 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext       | 128    |  3.310 ns | 0.0053 ns | 0.0041 ns |  0.23 |         - |          NA |
