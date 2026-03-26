using SC.AtomicState.MiddlewareEx;

namespace SC.AtomicState.Tests
{
    public class TimetravelTests
    {
        private sealed record TestState(int Value) : IState;

        [Fact]
        public void Timetravel_Controller_Exists()
        {
            var options = new TimeTravelOptions();
            var middleware = TimetravelMiddleware.Create(options, out var controller);
            Assert.NotNull(controller);
            Assert.IsAssignableFrom<ITimetravelController>(controller);
        }
    }
}
