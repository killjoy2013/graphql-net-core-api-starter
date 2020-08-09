using GraphQL.Types;
using GraphQL.WebApi.Dto;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class AddCityMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<CityGType>("addCity",
            arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "countryId" },
               new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "cityName" },
               new QueryArgument<IntGraphType> { Name = "population" }
            ),
            resolve: context =>
            {                
                var countryId = context.GetArgument<int>("countryId");
                var cityName = context.GetArgument<string>("cityName");
                var population = context.GetArgument<int?>("population");

                var subscriptionServices = (ISubscriptionServices)sp.GetService(typeof(ISubscriptionServices));
                var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                var countryRepository = (IGenericRepository<Country>)sp.GetService(typeof(IGenericRepository<Country>));

                var foundCountry = countryRepository.GetById(countryId);

                var newCity = new City
                {
                    name = cityName,
                    country_id = countryId,
                    population=population
                };

                var addedCity = cityRepository.Insert(newCity);
                subscriptionServices.CityAddedService.AddCityAddedMessage(new CityAddedMessage
                {
                    cityName = addedCity.name,
                    countryName = foundCountry.name,
                    id = addedCity.id,
                    message = "A new city added"
                });
                return addedCity;

            });
        }
    }
}
