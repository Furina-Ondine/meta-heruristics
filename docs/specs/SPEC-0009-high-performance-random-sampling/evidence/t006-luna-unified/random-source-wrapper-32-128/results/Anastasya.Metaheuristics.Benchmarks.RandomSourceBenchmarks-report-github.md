```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method                          | Length | Mean        | Error     | StdDev    | Allocated |
|-------------------------------- |------- |------------:|----------:|----------:|----------:|
| **RandomSourceFillULong**           | **32**     |    **23.82 ns** |  **0.058 ns** |  **0.052 ns** |         **-** |
| RandomSourceScalarULong         | 32     |    78.41 ns |  1.508 ns |  1.481 ns |         - |
| SystemRandomScalarULong         | 32     |   921.24 ns |  3.235 ns |  2.868 ns |         - |
| RandomSourceFillUnitDouble      | 32     |    24.38 ns |  0.115 ns |  0.108 ns |         - |
| RandomSourceScalarUnitDouble    | 32     |    74.89 ns |  0.235 ns |  0.209 ns |         - |
| SystemRandomScalarUnitDouble    | 32     |   180.99 ns |  1.519 ns |  1.421 ns |         - |
| RandomSourceFillBoundedDouble   | 32     |    27.59 ns |  0.206 ns |  0.183 ns |         - |
| RandomSourceScalarBoundedDouble | 32     |    79.86 ns |  0.305 ns |  0.270 ns |         - |
| SystemRandomScalarBoundedDouble | 32     |   183.12 ns |  1.965 ns |  1.641 ns |         - |
| RandomSourceFillBoundedInt      | 32     |    25.57 ns |  0.094 ns |  0.084 ns |         - |
| RandomSourceScalarBoundedInt    | 32     |    74.92 ns |  0.326 ns |  0.289 ns |         - |
| SystemRandomScalarBoundedInt    | 32     |   170.71 ns |  0.508 ns |  0.396 ns |         - |
| **RandomSourceFillULong**           | **128**    |    **92.20 ns** |  **0.063 ns** |  **0.053 ns** |         **-** |
| RandomSourceScalarULong         | 128    |   314.09 ns |  1.003 ns |  0.938 ns |         - |
| SystemRandomScalarULong         | 128    | 3,679.44 ns | 22.285 ns | 20.846 ns |         - |
| RandomSourceFillUnitDouble      | 128    |    95.58 ns |  0.139 ns |  0.123 ns |         - |
| RandomSourceScalarUnitDouble    | 128    |   299.99 ns |  1.101 ns |  0.976 ns |         - |
| SystemRandomScalarUnitDouble    | 128    |   661.82 ns |  8.668 ns |  7.684 ns |         - |
| RandomSourceFillBoundedDouble   | 128    |   111.60 ns |  0.610 ns |  0.541 ns |         - |
| RandomSourceScalarBoundedDouble | 128    |   320.34 ns |  1.165 ns |  1.090 ns |         - |
| SystemRandomScalarBoundedDouble | 128    |   671.23 ns |  4.466 ns |  3.959 ns |         - |
| RandomSourceFillBoundedInt      | 128    |   100.60 ns |  0.113 ns |  0.088 ns |         - |
| RandomSourceScalarBoundedInt    | 128    |   303.66 ns |  0.979 ns |  0.818 ns |         - |
| SystemRandomScalarBoundedInt    | 128    |   683.00 ns |  3.196 ns |  2.989 ns |         - |
