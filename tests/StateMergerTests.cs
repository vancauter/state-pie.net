using Xunit;

namespace StatePie.Tests
{
    public class StateMergerTests
    {
        [Fact]
        public void GivenCurrentStateAndPartialUpdates_WhenMerging_ThenMatchingPropertiesAreOverwrittenAndOthersPreserved()
        {
            var current = new ProfileState
            {
                DisplayName = "Guest",
                TimeZone = "UTC",
                IsActive = false
            };

            var merged = StateMerger.Merge(current, new { DisplayName = "FooBar", IsActive = true });

            Assert.Equal("FooBar", merged.DisplayName);
            Assert.Equal("UTC", merged.TimeZone);
            Assert.True(merged.IsActive);
            Assert.NotSame(current, merged);
        }

        [Fact]
        public void GivenNullState_WhenMerging_ThenArgumentNullExceptionIsThrown()
        {
            var updates = new { DisplayName = "FooBar" };
            _ = Assert.Throws<ArgumentNullException>(() => StateMerger.Merge<ProfileState>(null!, updates));
        }

        [Fact]
        public void GivenNullUpdates_WhenMerging_ThenArgumentNullExceptionIsThrown()
        {
            var current = new ProfileState();
            _ = Assert.Throws<ArgumentNullException>(() => StateMerger.Merge(current, null!));
        }

        private sealed class ProfileState : IState
        {
            public string DisplayName { get; set; } = string.Empty;

            public string TimeZone { get; set; } = string.Empty;

            public bool IsActive { get; set; }
        }
    }
}
