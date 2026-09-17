```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |----------:|----------:|----------:|------:|----------:|------------:|
| **BaselineNext**  | **32**     | **0.9887 ns** | **0.0007 ns** | **0.0005 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext | 32     | 0.9937 ns | 0.0011 ns | 0.0008 ns |  1.01 |         - |          NA |
|               |        |           |           |           |       |           |             |
| **BaselineNext**  | **128**    | **0.9891 ns** | **0.0019 ns** | **0.0015 ns** |  **1.00** |         **-** |          **NA** |
| CandidateNext | 128    | 0.9941 ns | 0.0009 ns | 0.0006 ns |  1.01 |         - |          NA |
