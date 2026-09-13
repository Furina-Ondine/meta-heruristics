```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method     | Length | Mean       | Error     | StdDev    | Code Size |
|----------- |------- |-----------:|----------:|----------:|----------:|
| **RawFill**    | **32**     |  **16.896 ns** | **0.0192 ns** | **0.0150 ns** |     **617 B** |
| UnitFill   | 32     |  18.629 ns | 0.0051 ns | 0.0037 ns |     737 B |
| NormalFill | 32     | 112.767 ns | 0.0546 ns | 0.0361 ns |   2,234 B |
| VectorApi  | 32     |   1.833 ns | 0.0011 ns | 0.0008 ns |      97 B |
| **RawFill**    | **128**    |  **64.434 ns** | **0.0274 ns** | **0.0198 ns** |     **617 B** |
| UnitFill   | 128    |  73.215 ns | 0.0130 ns | 0.0094 ns |     737 B |
| NormalFill | 128    | 446.771 ns | 0.1960 ns | 0.1417 ns |   2,234 B |
| VectorApi  | 128    |   1.986 ns | 0.0010 ns | 0.0007 ns |      97 B |
