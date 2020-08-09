using GraphQL.WebApi.Interfaces;

namespace GraphQL.WebApi.Services
{
    public class SubscriptionServices : ISubscriptionServices
    {
        public SubscriptionServices()
        {
            this.CityAddedService = new CityAddedService();
        }
        public CityAddedService CityAddedService { get; }
    }
}
