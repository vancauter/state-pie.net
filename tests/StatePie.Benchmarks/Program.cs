using BenchmarkDotNet.Running;

namespace StatePie.Benchmarks
{
    public static class Program
    {
        public static void Main() => BenchmarkRunner.Run<StateMergerBenchmarks>();
    }
}
