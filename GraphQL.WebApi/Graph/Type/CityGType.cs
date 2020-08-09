using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using System;

namespace GraphQL.WebApi.Graph.Type
{
    public class CityGType : ObjectGraphType<City>
    {
        public IServiceProvider Provider { get; set; }
        public CityGType(IServiceProvider provider)
        {
            Field(x => x.id, type: typeof(IntGraphType));
            Field(x => x.name, type: typeof(StringGraphType));
            Field(x => x.population, type: typeof(IntGraphType));
            Field<CountryGType>("country", resolve: context => {
                IGenericRepository<Country> countryRepository = (IGenericRepository<Country>)provider.GetService(typeof(IGenericRepository<Country>));
                return countryRepository.GetById(context.Source.country_id);
            });
        }
    }
}
