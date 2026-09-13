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
| **BaselineFill**  | **32**     |  **31.176 ns** | **0.1181 ns** | **0.0922 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 32     |   6.812 ns | 0.1360 ns | 0.1062 ns |  0.22 |         - |          NA |
|               |        |            |           |           |       |           |             |
| **BaselineFill**  | **128**    | **120.507 ns** | **0.1728 ns** | **0.1249 ns** |  **1.00** |         **-** |          **NA** |
| CandidateFill | 128    |  30.541 ns | 0.0631 ns | 0.0493 ns |  0.25 |         - |          NA |
