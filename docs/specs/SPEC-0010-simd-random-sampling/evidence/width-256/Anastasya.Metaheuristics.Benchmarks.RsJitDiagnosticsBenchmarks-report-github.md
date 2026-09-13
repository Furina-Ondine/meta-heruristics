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
| **RawFill**        | **32**     |   **9.075 ns** | **0.0267 ns** | **0.0209 ns** |     **756 B** |
| UnitFill       | 32     |  10.834 ns | 0.0925 ns | 0.0612 ns |     875 B |
| BoundedIntFill | 32     |  31.966 ns | 0.2947 ns | 0.2131 ns |     588 B |
| NormalFill     | 32     | 230.930 ns | 0.5016 ns | 0.3916 ns |   2,422 B |
| VectorApi      | 32     |   2.079 ns | 0.0070 ns | 0.0055 ns |     106 B |
| **RawFill**        | **128**    |  **33.376 ns** | **0.0563 ns** | **0.0439 ns** |     **756 B** |
| UnitFill       | 128    |  36.925 ns | 0.0104 ns | 0.0075 ns |     875 B |
| BoundedIntFill | 128    | 115.368 ns | 1.4644 ns | 1.0589 ns |     588 B |
| NormalFill     | 128    | 886.628 ns | 0.4344 ns | 0.3392 ns |   2,422 B |
| VectorApi      | 128    |   2.078 ns | 0.0023 ns | 0.0015 ns |     106 B |
