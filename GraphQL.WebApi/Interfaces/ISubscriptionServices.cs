using GraphQL.WebApi.Services;

namespace GraphQL.WebApi.Interfaces
{
    public interface ISubscriptionServices
    {
        CityAddedService CityAddedService { get; }
    }
}
