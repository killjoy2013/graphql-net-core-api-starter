using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class DeleteCityMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<StringGraphType>("deleteCity",
            arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "cityId" }               
            ),
            resolve: context =>
            {
                var cityId = context.GetArgument<int>("cityId");
                var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                cityRepository.Delete(cityId);
                return $"cityId:{cityId} deleted";
            });
        }
    }
}
