```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method                          | Length | Mean        | Error     | StdDev   | Allocated |
|-------------------------------- |------- |------------:|----------:|---------:|----------:|
| **RandomSourceFillULong**           | **32**     |    **70.82 ns** |  **0.347 ns** | **0.308 ns** |         **-** |
| RandomSourceScalarULong         | 32     |    74.19 ns |  0.599 ns | 0.468 ns |         - |
| SystemRandomScalarULong         | 32     |   915.44 ns |  7.504 ns | 6.266 ns |         - |
| RandomSourceFillUnitDouble      | 32     |    73.29 ns |  1.044 ns | 0.815 ns |         - |
| RandomSourceScalarUnitDouble    | 32     |    74.01 ns |  0.684 ns | 0.534 ns |         - |
| SystemRandomScalarUnitDouble    | 32     |   171.84 ns |  2.644 ns | 2.473 ns |         - |
| RandomSourceFillBoundedDouble   | 32     |    76.45 ns |  0.451 ns | 0.377 ns |         - |
| RandomSourceScalarBoundedDouble | 32     |    77.83 ns |  0.179 ns | 0.168 ns |         - |
| SystemRandomScalarBoundedDouble | 32     |   178.23 ns |  2.929 ns | 2.876 ns |         - |
| RandomSourceFillBoundedInt      | 32     |    71.89 ns |  0.901 ns | 0.752 ns |         - |
| RandomSourceScalarBoundedInt    | 32     |    73.60 ns |  0.345 ns | 0.322 ns |         - |
| SystemRandomScalarBoundedInt    | 32     |   169.96 ns |  2.926 ns | 2.593 ns |         - |
| **RandomSourceFillULong**           | **128**    |   **301.58 ns** |  **2.728 ns** | **2.278 ns** |         **-** |
| RandomSourceScalarULong         | 128    |   308.60 ns |  1.136 ns | 1.007 ns |         - |
| SystemRandomScalarULong         | 128    | 3,675.39 ns |  8.397 ns | 7.012 ns |         - |
| RandomSourceFillUnitDouble      | 128    |   300.69 ns |  4.798 ns | 4.007 ns |         - |
| RandomSourceScalarUnitDouble    | 128    |   294.43 ns |  2.167 ns | 1.692 ns |         - |
| SystemRandomScalarUnitDouble    | 128    |   658.60 ns |  6.682 ns | 5.217 ns |         - |
| RandomSourceFillBoundedDouble   | 128    |   319.52 ns |  6.342 ns | 7.550 ns |         - |
| RandomSourceScalarBoundedDouble | 128    |   320.14 ns |  0.304 ns | 0.254 ns |         - |
| SystemRandomScalarBoundedDouble | 128    |   698.98 ns | 10.554 ns | 9.872 ns |         - |
| RandomSourceFillBoundedInt      | 128    |   291.56 ns |  0.847 ns | 0.751 ns |         - |
| RandomSourceScalarBoundedInt    | 128    |   302.49 ns |  2.197 ns | 1.834 ns |         - |
| SystemRandomScalarBoundedInt    | 128    |   674.56 ns |  2.454 ns | 2.050 ns |         - |
