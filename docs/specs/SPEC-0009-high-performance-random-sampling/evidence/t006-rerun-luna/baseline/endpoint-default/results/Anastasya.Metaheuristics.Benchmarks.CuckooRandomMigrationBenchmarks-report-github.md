```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method | Dimension | Mean      | Error    | StdDev   | Gen0    | Gen1   | Allocated |
|------- |---------- |----------:|---------:|---------:|--------:|-------:|----------:|
| **Run**    | **32**        |  **59.00 μs** | **1.166 μs** | **1.248 μs** |  **5.3711** | **0.6714** |  **44.25 KB** |
| **Run**    | **128**       | **164.66 μs** | **0.441 μs** | **0.344 μs** | **17.0898** | **5.6152** |    **141 KB** |
