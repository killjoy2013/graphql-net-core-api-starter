using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace GraphQL.WebApi.Graph.Query.Business
{
    public class CityQuery : IFieldQueryServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<ListGraphType<CityGType>>("cities",
               arguments: new QueryArguments(
                 new QueryArgument<StringGraphType> { Name = "name" }
               ),
               resolve: context =>
               {            
                   var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                   var baseQuery = cityRepository.GetAll();
                   var name = context.GetArgument<string>("name");
                   if (name != default(string))
                   {
                       return baseQuery.Where(w => w.name.Contains(name));
                   }
                   return baseQuery.ToList();
               });
        }
    }
}
