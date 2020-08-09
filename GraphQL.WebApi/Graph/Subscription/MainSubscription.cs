using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.WebApi.Graph.Subscription
{
    public class MainSubscription : ObjectGraphType
    {
        public MainSubscription(IServiceProvider provider, IWebHostEnvironment env, IFieldService fieldService)
        {
            Name = "MainSubscription";
            fieldService.ActivateFields(this, FieldServiceType.Subscription, env, provider);
        }
    }
}
