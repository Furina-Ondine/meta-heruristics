```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method              | Length | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------- |------- |-----------:|----------:|----------:|------:|----------:|------------:|
| **BaselineFill**        | **32**     |  **20.438 ns** | **0.0166 ns** | **0.0120 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   6.940 ns | 0.0143 ns | 0.0112 ns |  0.34 |         - |          NA |
| ScalarReferenceFill | 32     |  68.013 ns | 0.1902 ns | 0.1375 ns |  3.33 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **78.183 ns** | **0.0637 ns** | **0.0422 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  19.886 ns | 0.0224 ns | 0.0175 ns |  0.25 |         - |          NA |
| ScalarReferenceFill | 128    | 270.952 ns | 0.5484 ns | 0.3965 ns |  3.47 |         - |          NA |
