using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.WebApi.Dto;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Subscription
{
    public class CityAddedSubscription : IFieldSubscriptionServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.AddField(new EventStreamFieldType
            {
                Name = "cityAdded",
                Type = typeof(CityAddedMessageGType),
                Resolver = new FuncFieldResolver<CityAddedMessage>(context => context.Source as CityAddedMessage),
                Arguments = new QueryArguments(                   
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "countryName" }
                ),
                Subscriber = new EventStreamResolver<CityAddedMessage>(context =>
                {
                    var subscriptionServices = (ISubscriptionServices)sp.GetService(typeof(ISubscriptionServices));
                    var countryName = context.GetArgument<string>("countryName");                  
                    return subscriptionServices.CityAddedService.GetMessages(countryName);
                })
            });
        }
    }
}
