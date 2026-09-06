```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a


```
| Method                         | Length | Mean         | Error     | StdDev    | Allocated |
|------------------------------- |------- |-------------:|----------:|----------:|----------:|
| **StandardNormalSample**           | **32**     |    **11.859 ns** | **0.0222 ns** | **0.0197 ns** |         **-** |
| ReferenceSourceBoxMullerSample | 32     |    11.858 ns | 0.0608 ns | 0.0569 ns |         - |
| CuckooReferenceSample          | 32     |     8.285 ns | 0.0310 ns | 0.0275 ns |         - |
| StandardNormalFill             | 32     |   246.751 ns | 1.5497 ns | 1.3738 ns |         - |
| ReferenceSourceBoxMullerFill   | 32     |   232.740 ns | 0.8798 ns | 0.7799 ns |         - |
| CuckooReferenceFill            | 32     |   361.615 ns | 1.2168 ns | 1.0787 ns |         - |
| **StandardNormalSample**           | **128**    |    **11.867 ns** | **0.0298 ns** | **0.0249 ns** |         **-** |
| ReferenceSourceBoxMullerSample | 128    |    11.881 ns | 0.0675 ns | 0.0599 ns |         - |
| CuckooReferenceSample          | 128    |     8.270 ns | 0.0199 ns | 0.0167 ns |         - |
| StandardNormalFill             | 128    |   977.268 ns | 5.1565 ns | 4.5711 ns |         - |
| ReferenceSourceBoxMullerFill   | 128    |   916.990 ns | 0.9568 ns | 0.8482 ns |         - |
| CuckooReferenceFill            | 128    | 1,406.722 ns | 3.7640 ns | 3.3367 ns |         - |
