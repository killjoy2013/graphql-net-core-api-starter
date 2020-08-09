using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace GraphQL.WebApi.Dto
{
    public class CityAddedMessage
    {
        public int id { get; set; }
        public string cityName { get; set; }
        public string countryName { get; set; }
        public string message { get; set; }
    }
}
