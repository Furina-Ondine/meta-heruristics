```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method | Dimension | Mean       | Error    | StdDev   | Gen0    | Gen1    | Allocated |
|------- |---------- |-----------:|---------:|---------:|--------:|--------:|----------:|
| **Run**    | **32**        |   **326.4 μs** |  **1.46 μs** |  **1.36 μs** | **27.3438** | **13.1836** | **223.74 KB** |
| **Run**    | **128**       | **1,541.4 μs** | **14.02 μs** | **13.11 μs** | **97.6563** | **23.4375** | **800.52 KB** |
