```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method        | Length | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------- |------- |-----------:|----------:|----------:|------:|----------:|------------:|
| **BaselineFill**  | **32**     |  **31.266 ns** | **0.0567 ns** | **0.0337 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   6.734 ns | 0.0144 ns | 0.0095 ns |  0.22 |         - |          NA |
|               |        |            |           |           |       |           |             |
| **BaselineFill**  | **128**    | **124.291 ns** | **0.3761 ns** | **0.2720 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |  24.316 ns | 0.0255 ns | 0.0184 ns |  0.20 |         - |          NA |
