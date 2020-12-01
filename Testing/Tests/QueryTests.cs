using GraphQL.WebApi.Models;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        public async Task Adds_Country()
        {
            var param = new JObject();
            param["query"] = @"mutation addCountry($countryName:String!){
                                  addCountry(countryName:$countryName){
                                    id
                                    name
                                  }
                                }";

            dynamic variables = new JObject();      
            variables.countryName = TestClassFixture.RandomString(8);
            param["variables"] = variables;

            var content = new StringContent(JsonConvert.SerializeObject(param), UTF8Encoding.UTF8, "application/json");
            var response = await _fixture.Client.PostAsync("graphql", content);
            var serviceResultJson = await response.Content.ReadAsStringAsync();

            var gqlResult = new GqlResult<Country>(serviceResultJson, "addCountry");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK, "response status code 200");
            Assert.True(gqlResult.GraphQLError == null, "addCountry mutation should not throw exception");
            Assert.True(gqlResult.Data.id > 0, $"Added country's id should be mode than zero");
        }
    }
}
