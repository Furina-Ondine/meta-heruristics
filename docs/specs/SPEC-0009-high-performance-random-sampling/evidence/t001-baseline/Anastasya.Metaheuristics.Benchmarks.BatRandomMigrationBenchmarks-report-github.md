```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method | Dimension | Mean       | Error    | StdDev   | Median     | Gen0    | Gen1    | Allocated |
|------- |---------- |-----------:|---------:|---------:|-----------:|--------:|--------:|----------:|
| **Run**    | **32**        |   **319.6 μs** |  **7.70 μs** | **22.10 μs** |   **313.4 μs** | **27.3438** | **12.6953** | **223.99 KB** |
| **Run**    | **128**       | **2,526.4 μs** | **28.51 μs** | **25.27 μs** | **2,529.5 μs** | **97.6563** | **27.3438** | **800.78 KB** |
