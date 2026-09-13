```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method         | Length | Mean         | Error     | StdDev    | Code Size |
|--------------- |------- |-------------:|----------:|----------:|----------:|
| **RawFill**        | **32**     |    **17.055 ns** | **0.0375 ns** | **0.0293 ns** |     **617 B** |
| UnitFill       | 32     |    18.955 ns | 0.0366 ns | 0.0286 ns |     737 B |
| BoundedIntFill | 32     |    32.594 ns | 0.9314 ns | 0.6160 ns |     579 B |
| NormalFill     | 32     |   424.462 ns | 0.4267 ns | 0.3086 ns |   2,259 B |
| VectorApi      | 32     |     1.859 ns | 0.0028 ns | 0.0020 ns |      97 B |
| **RawFill**        | **128**    |    **64.539 ns** | **0.0420 ns** | **0.0304 ns** |     **617 B** |
| UnitFill       | 128    |    73.316 ns | 0.0577 ns | 0.0451 ns |     737 B |
| BoundedIntFill | 128    |   114.518 ns | 0.9864 ns | 0.6524 ns |     579 B |
| NormalFill     | 128    | 1,675.074 ns | 1.3262 ns | 1.0354 ns |   2,259 B |
| VectorApi      | 128    |     1.857 ns | 0.0017 ns | 0.0013 ns |      97 B |
