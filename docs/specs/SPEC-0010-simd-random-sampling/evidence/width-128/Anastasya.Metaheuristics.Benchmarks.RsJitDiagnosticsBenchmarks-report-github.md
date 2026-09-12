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
| **RawFill**        | **32**     |    **25.187 ns** | **0.0724 ns** | **0.0523 ns** |     **731 B** |
| UnitFill       | 32     |    27.699 ns | 0.0137 ns | 0.0107 ns |     853 B |
| BoundedIntFill | 32     |    29.163 ns | 0.3627 ns | 0.2831 ns |     604 B |
| NormalFill     | 32     |   425.081 ns | 0.1170 ns | 0.0913 ns |   2,269 B |
| VectorApi      | 32     |     2.323 ns | 0.0040 ns | 0.0032 ns |     111 B |
| **RawFill**        | **128**    |    **97.620 ns** | **0.4989 ns** | **0.3895 ns** |     **731 B** |
| UnitFill       | 128    |   108.740 ns | 0.1042 ns | 0.0814 ns |     853 B |
| BoundedIntFill | 128    |   113.035 ns | 0.5683 ns | 0.4109 ns |     604 B |
| NormalFill     | 128    | 1,678.072 ns | 1.1027 ns | 0.8609 ns |   2,269 B |
| VectorApi      | 128    |     2.274 ns | 0.0025 ns | 0.0018 ns |     111 B |
