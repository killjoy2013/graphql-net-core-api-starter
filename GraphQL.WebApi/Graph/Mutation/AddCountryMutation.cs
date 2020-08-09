using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;


namespace GraphQL.WebApi.Graph.Mutation
{    public class AddCountryMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<CountryGType>("addCountry",
            arguments: new QueryArguments(               
               new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "countryName" }
            ),
            resolve: context =>
            {                
                var countryName = context.GetArgument<string>("countryName");
                var countryRepository = (IGenericRepository<Country>)sp.GetService(typeof(IGenericRepository<Country>));

                var newCountry = new Country
                {
                    name = countryName
                };

                return countryRepository.Insert(newCountry);
            });
        }
    }
}
