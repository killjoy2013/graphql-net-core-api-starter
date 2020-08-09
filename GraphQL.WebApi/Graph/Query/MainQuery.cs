using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Query
{
    public class MainQuery : ObjectGraphType
    {
        public MainQuery(IServiceProvider provider, IWebHostEnvironment env, IFieldService fieldService)
        {
            Name = "MainQuery";
            fieldService.ActivateFields(this, FieldServiceType.Query, env, provider);
        }
    }
}
