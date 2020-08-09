using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using System;
using System.Linq;

namespace GraphQL.WebApi.Graph.Type
{
    public class CountryGType : ObjectGraphType<Country>
    {
        public IServiceProvider Provider { get; set; }
        public CountryGType(IServiceProvider provider)
        {
            Field(x => x.id, type: typeof(IntGraphType));
            Field(x => x.name, type: typeof(StringGraphType));
            Field<ListGraphType<CityGType>>("cities", resolve: context => {
                IGenericRepository<City> cityRepository = (IGenericRepository<City>)provider.GetService(typeof(IGenericRepository<City>));
                return cityRepository.GetAll().Where(w=> w.Country.id ==  context.Source.id);
            });
        }
    }
}
