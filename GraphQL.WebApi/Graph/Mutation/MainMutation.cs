using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class MainMutation : ObjectGraphType
    {
        public MainMutation(IServiceProvider provider, IWebHostEnvironment env, IFieldService fieldService)
        {
            Name = "MainMutation";
            fieldService.ActivateFields(this, FieldServiceType.Mutation, env, provider);
        }
    }
}
