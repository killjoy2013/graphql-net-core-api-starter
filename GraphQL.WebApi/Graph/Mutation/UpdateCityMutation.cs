using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class UpdateCityMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<CityGType>("updateCity",
            arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "cityId" },
               new QueryArgument<IntGraphType> { Name = "countryId" },
               new QueryArgument<StringGraphType> { Name = "cityName" },
               new QueryArgument<IntGraphType> { Name = "population" }
            ),
            resolve: context =>
            {
                var cityId = context.GetArgument<int>("cityId");
                var countryId = context.GetArgument<int?>("countryId");
                var cityName = context.GetArgument<string>("cityName");
                var population = context.GetArgument<int?>("population");

                var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                var foundCity = cityRepository.GetById(cityId);

                if (countryId != null)
                {
                    foundCity.country_id = countryId.Value;
                }
                if (!String.IsNullOrEmpty(cityName))
                {
                    foundCity.name = cityName;
                }
                if (population != null)
                {
                    foundCity.population = population.Value;
                }

                return cityRepository.Update(foundCity);
            });
        }
    }
}
