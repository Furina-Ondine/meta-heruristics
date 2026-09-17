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
| **RawFill**    | **32**     |   **9.035 ns** | **0.0222 ns** | **0.0161 ns** |     **756 B** |
| UnitFill   | 32     |  11.061 ns | 0.0220 ns | 0.0172 ns |     875 B |
| NormalFill | 32     |  65.135 ns | 0.4905 ns | 0.3547 ns |   2,408 B |
| VectorApi  | 32     |   2.076 ns | 0.0022 ns | 0.0015 ns |     106 B |
| **RawFill**    | **128**    |  **32.847 ns** | **0.0136 ns** | **0.0090 ns** |     **756 B** |
| UnitFill   | 128    |  36.882 ns | 0.0258 ns | 0.0202 ns |     875 B |
| NormalFill | 128    | 246.588 ns | 0.5068 ns | 0.3957 ns |   2,408 B |
| VectorApi  | 128    |   2.090 ns | 0.0017 ns | 0.0012 ns |     106 B |
