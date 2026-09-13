```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-HHSXOG : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=12  WarmupCount=5  

```
| Method                    | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| BaselineConstructAndFill  |    23.56 ns | 0.011 ns | 0.007 ns |  1.00 |    0.00 |      - |         - |          NA |
| CandidateConstructAndFill | 1,782.25 ns | 1.338 ns | 0.967 ns | 75.64 |    0.05 | 0.0057 |     304 B |          NA |
