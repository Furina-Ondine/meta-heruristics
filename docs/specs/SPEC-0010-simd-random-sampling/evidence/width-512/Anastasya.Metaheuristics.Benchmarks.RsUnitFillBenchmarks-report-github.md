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
| **BaselineFill**        | **32**     |  **20.556 ns** | **0.0271 ns** | **0.0212 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 32     |   7.874 ns | 0.0065 ns | 0.0047 ns |  0.38 |         - |          NA |
| ScalarReferenceFill | 32     |  67.606 ns | 0.1766 ns | 0.1379 ns |  3.29 |         - |          NA |
|                     |        |            |           |           |       |           |             |
| **BaselineFill**        | **128**    |  **78.794 ns** | **0.0965 ns** | **0.0753 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill       | 128    |  28.209 ns | 0.0097 ns | 0.0070 ns |  0.36 |         - |          NA |
| ScalarReferenceFill | 128    | 268.183 ns | 0.5223 ns | 0.4078 ns |  3.40 |         - |          NA |
