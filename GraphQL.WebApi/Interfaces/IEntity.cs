using System;

namespace GraphQL.WebApi.Interfaces
{
    public interface IEntity
    {
        int id { get; set; }
        DateTime? creation_date { get; set; }
    }
}
