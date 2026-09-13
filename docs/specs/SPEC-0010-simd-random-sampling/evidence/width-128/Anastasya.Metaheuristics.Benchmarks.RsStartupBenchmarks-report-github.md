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
| BaselineConstruct  |   2.206 ns | 0.0439 ns | 0.0290 ns |   1.00 |    0.02 |      - |         - |          NA |
| CandidateConstruct | 470.660 ns | 6.0201 ns | 4.3529 ns | 213.36 |    3.26 | 0.0019 |     112 B |          NA |
