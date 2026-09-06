```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method | Dimension | Mean      | Error    | StdDev   | Gen0    | Gen1   | Allocated |
|------- |---------- |----------:|---------:|---------:|--------:|-------:|----------:|
| **Run**    | **32**        |  **56.85 μs** | **0.094 μs** | **0.079 μs** |  **5.3711** | **0.7324** |     **44 KB** |
| **Run**    | **128**       | **169.14 μs** | **0.286 μs** | **0.267 μs** | **17.0898** | **5.6152** | **140.75 KB** |
