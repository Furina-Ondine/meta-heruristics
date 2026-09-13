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
| **RawFill**    | **32**     |   **5.882 ns** | **0.0068 ns** | **0.0053 ns** |     **852 B** |
| UnitFill   | 32     |   7.069 ns | 0.0030 ns | 0.0020 ns |     988 B |
| NormalFill | 32     |  43.571 ns | 0.0902 ns | 0.0652 ns |   2,654 B |
| VectorApi  | 32     |   3.319 ns | 0.0045 ns | 0.0035 ns |     156 B |
| **RawFill**    | **128**    |  **17.578 ns** | **0.0170 ns** | **0.0133 ns** |     **852 B** |
| UnitFill   | 128    |  19.544 ns | 0.0237 ns | 0.0157 ns |     988 B |
| NormalFill | 128    | 177.855 ns | 1.1454 ns | 0.8942 ns |   2,654 B |
| VectorApi  | 128    |   3.200 ns | 0.0091 ns | 0.0071 ns |     156 B |
