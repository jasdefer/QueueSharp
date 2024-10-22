using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

Summary[] summary = BenchmarkRunner.Run(typeof(Program).Assembly);