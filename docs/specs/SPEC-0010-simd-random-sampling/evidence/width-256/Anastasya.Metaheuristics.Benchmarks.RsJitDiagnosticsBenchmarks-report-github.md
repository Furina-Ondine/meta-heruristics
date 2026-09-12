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
| **RawFill**        | **32**     |  **13.831 ns** | **0.0211 ns** | **0.0165 ns** |     **870 B** |
| UnitFill       | 32     |  14.015 ns | 0.0044 ns | 0.0029 ns |     991 B |
| BoundedIntFill | 32     |  29.943 ns | 0.0406 ns | 0.0294 ns |     613 B |
| NormalFill     | 32     | 229.617 ns | 0.2356 ns | 0.1839 ns |   2,443 B |
| VectorApi      | 32     |   2.494 ns | 0.0019 ns | 0.0015 ns |     120 B |
| **RawFill**        | **128**    |  **48.713 ns** | **0.1220 ns** | **0.0952 ns** |     **870 B** |
| UnitFill       | 128    |  54.585 ns | 0.0099 ns | 0.0072 ns |     991 B |
| BoundedIntFill | 128    | 115.626 ns | 0.1505 ns | 0.0995 ns |     613 B |
| NormalFill     | 128    | 892.598 ns | 0.4267 ns | 0.3085 ns |   2,443 B |
| VectorApi      | 128    |   2.494 ns | 0.0013 ns | 0.0008 ns |     120 B |
