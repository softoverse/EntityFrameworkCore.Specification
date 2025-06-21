// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using Benchmark;

using BenchmarkDotNet.Running;

// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

var summary = BenchmarkRunner.Run<MyBenchmark>();