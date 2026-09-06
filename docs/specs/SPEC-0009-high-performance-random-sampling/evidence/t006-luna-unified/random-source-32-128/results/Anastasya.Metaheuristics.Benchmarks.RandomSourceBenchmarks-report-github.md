```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method                          | Length | Mean        | Error     | StdDev    | Allocated |
|-------------------------------- |------- |------------:|----------:|----------:|----------:|
| **RandomSourceFillULong**           | **32**     |    **23.70 ns** |  **0.059 ns** |  **0.055 ns** |         **-** |
| RandomSourceScalarULong         | 32     |    95.49 ns |  1.394 ns |  1.236 ns |         - |
| SystemRandomScalarULong         | 32     |   927.90 ns |  5.104 ns |  4.774 ns |         - |
| RandomSourceFillUnitDouble      | 32     |    24.28 ns |  0.022 ns |  0.019 ns |         - |
| RandomSourceScalarUnitDouble    | 32     |    94.45 ns |  1.351 ns |  1.263 ns |         - |
| SystemRandomScalarUnitDouble    | 32     |   179.36 ns |  0.335 ns |  0.313 ns |         - |
| RandomSourceFillBoundedDouble   | 32     |    27.54 ns |  0.146 ns |  0.130 ns |         - |
| RandomSourceScalarBoundedDouble | 32     |    88.97 ns |  1.587 ns |  1.485 ns |         - |
| SystemRandomScalarBoundedDouble | 32     |   181.09 ns |  1.221 ns |  1.142 ns |         - |
| RandomSourceFillBoundedInt      | 32     |    25.34 ns |  0.036 ns |  0.032 ns |         - |
| RandomSourceScalarBoundedInt    | 32     |    90.95 ns |  0.990 ns |  0.878 ns |         - |
| SystemRandomScalarBoundedInt    | 32     |   168.55 ns |  0.914 ns |  0.855 ns |         - |
| **RandomSourceFillULong**           | **128**    |    **92.23 ns** |  **0.072 ns** |  **0.068 ns** |         **-** |
| RandomSourceScalarULong         | 128    |   389.73 ns |  4.039 ns |  3.779 ns |         - |
| SystemRandomScalarULong         | 128    | 3,688.98 ns | 16.976 ns | 14.176 ns |         - |
| RandomSourceFillUnitDouble      | 128    |    95.69 ns |  0.158 ns |  0.140 ns |         - |
| RandomSourceScalarUnitDouble    | 128    |   369.67 ns |  7.215 ns |  7.086 ns |         - |
| SystemRandomScalarUnitDouble    | 128    |   664.73 ns |  4.473 ns |  4.184 ns |         - |
| RandomSourceFillBoundedDouble   | 128    |   111.22 ns |  0.283 ns |  0.264 ns |         - |
| RandomSourceScalarBoundedDouble | 128    |   379.35 ns |  7.150 ns |  7.022 ns |         - |
| SystemRandomScalarBoundedDouble | 128    |   695.75 ns |  5.465 ns |  5.112 ns |         - |
| RandomSourceFillBoundedInt      | 128    |   100.63 ns |  0.180 ns |  0.160 ns |         - |
| RandomSourceScalarBoundedInt    | 128    |   387.78 ns |  4.148 ns |  3.880 ns |         - |
| SystemRandomScalarBoundedInt    | 128    |   673.44 ns | 10.863 ns | 10.161 ns |         - |
