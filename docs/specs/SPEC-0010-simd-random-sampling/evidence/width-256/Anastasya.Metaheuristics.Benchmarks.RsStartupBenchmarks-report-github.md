```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method             | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| BaselineConstruct  |   2.021 ns | 0.0075 ns | 0.0050 ns |   1.00 |    0.00 |      - |         - |          NA |
| CandidateConstruct | 902.978 ns | 3.0997 ns | 2.4200 ns | 446.79 |    1.56 | 0.0029 |     176 B |          NA |
