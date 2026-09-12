```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method         | Length | Mean       | Error     | StdDev    | Code Size |
|--------------- |------- |-----------:|----------:|----------:|----------:|
| **RawFill**        | **32**     |   **7.675 ns** | **0.0126 ns** | **0.0099 ns** |   **1,008 B** |
| UnitFill       | 32     |   7.971 ns | 0.0026 ns | 0.0019 ns |   1,144 B |
| BoundedIntFill | 32     |  30.429 ns | 0.0332 ns | 0.0240 ns |     687 B |
| NormalFill     | 32     | 232.749 ns | 0.2030 ns | 0.1585 ns |   2,668 B |
| VectorApi      | 32     |   3.295 ns | 0.0018 ns | 0.0012 ns |     182 B |
| **RawFill**        | **128**    |  **25.857 ns** | **0.0560 ns** | **0.0437 ns** |   **1,008 B** |
| UnitFill       | 128    |  28.183 ns | 0.0110 ns | 0.0080 ns |   1,144 B |
| BoundedIntFill | 128    | 110.765 ns | 0.4100 ns | 0.2965 ns |     687 B |
| NormalFill     | 128    | 909.367 ns | 0.5388 ns | 0.3564 ns |   2,668 B |
| VectorApi      | 128    |   3.298 ns | 0.0051 ns | 0.0039 ns |     182 B |
