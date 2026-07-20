namespace SC.AtomicState.Tests
{
    public class TypedActionTests
    {
        [Fact]
        public void CreateAction_WithTypedActionId_SetsTypeAndPayload()
        {
            var id = new SC.AtomicState.Helpers.TypedActionId<string>("@PERSON/SET_NAME");
            var action = Factory.CreateAction(id, "Lee-Lee");

            Assert.Equal(id.Type, action.Type);
            Assert.Equal("Lee-Lee", ((IAction<string>)action).Payload);
        }
    }
}
