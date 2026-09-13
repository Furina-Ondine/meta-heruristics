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
| **RawFill**        | **32**     |   **6.201 ns** | **0.0071 ns** | **0.0055 ns** |     **852 B** |
| UnitFill       | 32     |   6.888 ns | 0.0173 ns | 0.0125 ns |     988 B |
| BoundedIntFill | 32     |  29.101 ns | 0.0361 ns | 0.0261 ns |     650 B |
| NormalFill     | 32     | 188.329 ns | 0.1130 ns | 0.0817 ns |   2,652 B |
| VectorApi      | 32     |   3.193 ns | 0.0036 ns | 0.0028 ns |     156 B |
| **RawFill**        | **128**    |  **17.870 ns** | **0.0233 ns** | **0.0139 ns** |     **852 B** |
| UnitFill       | 128    |  20.871 ns | 0.0591 ns | 0.0391 ns |     988 B |
| BoundedIntFill | 128    | 116.408 ns | 5.3310 ns | 4.1621 ns |     650 B |
| NormalFill     | 128    | 741.836 ns | 4.8003 ns | 3.7478 ns |   2,652 B |
| VectorApi      | 128    |   3.197 ns | 0.0033 ns | 0.0024 ns |     156 B |
