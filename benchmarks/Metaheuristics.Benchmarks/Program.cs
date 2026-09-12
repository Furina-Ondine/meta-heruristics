using System.Numerics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Running;

Console.WriteLine(
    $"Vector<ulong>.Count={Vector<ulong>.Count} Vector<double>.Count={Vector<double>.Count} " +
    $"HardwareAccelerated={Vector.IsHardwareAccelerated} Avx512F={Avx512F.IsSupported} " +
    $"Runtime={Environment.Version} MaxVectorTBitWidth={Environment.GetEnvironmentVariable("DOTNET_MaxVectorTBitWidth")}");
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
