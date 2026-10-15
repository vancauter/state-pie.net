namespace StatePie.Benchmarks
{
    [MemoryDiagnoser]
    public class StateMergerBenchmarks
    {
        private readonly ProfileState _current = new()
        {
            DisplayName = "Guest",
            TimeZone = "UTC",
            IsActive = false
        };

        private readonly object _updates = new { DisplayName = "FooBar", IsActive = true };

        [Benchmark(Baseline = true)]
        public ProfileState ManualAssignment() =>
            new()
            {
                DisplayName = "FooBar",
                TimeZone = _current.TimeZone,
                IsActive = true
            };

        [Benchmark]
        public ProfileState StateMergerMerge() =>
            StateMerger.Merge(_current, _updates);

        public sealed class ProfileState : IState
        {
            public string DisplayName { get; set; } = string.Empty;
            public string TimeZone { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}
