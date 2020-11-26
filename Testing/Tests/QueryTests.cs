using Helpers;
using Testing.Helpers;
using Xunit;

namespace Testing.Tests
{
    public class QueryTests : TestClassBase, IClassFixture<TestClassFixture>
    {
        private readonly TestClassFixture _fixture;
        public QueryTests(TestClassFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(1)]
        public void Adds()
        {
            Assert.True(1 == 1);
        }
    }
}
