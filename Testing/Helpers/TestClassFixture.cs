using GraphQL.WebApi;
using GraphQL.WebApi.Repository;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Testing.Helpers
{
    public class TestClassFixture : IDisposable
    {
        #region Properties
        public DatabaseContext Context { get; set; }

        public TestServer Server { get; set; }

        public HttpClient Client { get; set; }
        #endregion

        public TestClassFixture()
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder();
            webHostBuilder.UseDefaultServiceProvider(options => options.ValidateScopes = false);
            webHostBuilder.UseEnvironment("Development");

            webHostBuilder.ConfigureAppConfiguration((builderContext, config) =>
            {              
                config.SetBasePath(Directory.GetCurrentDirectory());                
                    config.AddJsonFile($"appsettings.Development.json", optional: false, reloadOnChange: true)
                                 .AddEnvironmentVariables();
               
                //else
                //{
                //    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //                   .AddEnvironmentVariables();
                //}
               
            });


            Server = new TestServer(webHostBuilder.UseStartup<Startup>());
            Client = Server.CreateClient();          

            Context = Server.Host.Services.GetService(typeof(DatabaseContext)) as DatabaseContext;
           
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void Dispose()
        {
            Context.Dispose();
            Client.Dispose();
            Server.Dispose();

        }

    }

    public class GraphQLError
    {
        public string Key { get; set; }
        public string Value { get; set; }

    }

    public class GqlResult<T>
    {
        public GqlResult(string serviceResultJson, string queryName)
        {
            var rawResultJObject = JObject.Parse(serviceResultJson);
            var dataJObject = rawResultJObject["data"];
            Data = dataJObject == null ? default(T) : dataJObject[queryName].ToObject<T>();
            var graphQLErrorsJArray = (JArray)rawResultJObject["graphQLErrors"];
            GraphQLError = graphQLErrorsJArray == null ? null : graphQLErrorsJArray.ToObject<IList<GraphQLError>>()[0];

        }

        public T Data { get; set; }
        public GraphQLError GraphQLError { get; set; }
    }

    public class GqlResultList<T>
    {
        public GqlResultList(string serviceResultJson, string queryName)
        {
            var rawResultJObject = JObject.Parse(serviceResultJson);

            var dataJObject = rawResultJObject["data"];
            var dataArray = dataJObject == null ? new JArray() : (JArray)rawResultJObject["data"][queryName];
            Data = dataArray.ToObject<IList<T>>();
            var graphQLErrorsJArray = (JArray)rawResultJObject["graphQLErrors"];
            GraphQLError = graphQLErrorsJArray == null ? null : graphQLErrorsJArray.ToObject<IList<GraphQLError>>()[0];

        }

        public IList<T> Data { get; set; }
        public GraphQLError GraphQLError { get; set; }
    }


}
