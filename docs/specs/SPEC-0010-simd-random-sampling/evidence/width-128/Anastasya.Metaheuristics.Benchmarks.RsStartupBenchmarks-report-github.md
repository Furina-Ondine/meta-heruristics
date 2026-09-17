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
| BaselineConstruct  |   2.005 ns | 0.0030 ns | 0.0022 ns |   1.00 |    0.00 |      - |         - |          NA |
| CandidateConstruct | 451.839 ns | 0.3410 ns | 0.2466 ns | 225.41 |    0.26 | 0.0019 |     112 B |          NA |
