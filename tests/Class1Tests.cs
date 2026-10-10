using Xunit;

namespace SnapshotStore.Tests
{
    public class SnapshotStoreFactoryTests
    {
        [Fact]
        public void CreateActionWithTypeReturnsActionWithMatchingType()
        {
            var action = SnapshotStoreFactory.CreateAction("counter/increment");

            Assert.Equal("counter/increment", action.Type);
        }

        [Fact]
        public void CreateActionWithPayloadReturnsTypedActionWithMatchingValues()
        {
            var action = SnapshotStoreFactory.CreateAction("counter/set", 42);

            Assert.Equal("counter/set", action.Type);
            Assert.Equal(42, action.Payload);
        }
    }
}