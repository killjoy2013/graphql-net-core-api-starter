using GraphQL.Types;
using GraphQL.WebApi.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.WebApi.Graph.Type
{
    public class CityAddedMessageGType : ObjectGraphType<CityAddedMessage>
    {
        public CityAddedMessageGType()
        {
            Field(x => x.id, type: typeof(IntGraphType));
            Field(x => x.message, type: typeof(StringGraphType));
            Field(x => x.cityName, type: typeof(StringGraphType));
            Field(x => x.countryName, type: typeof(StringGraphType));
        }
    }
}
