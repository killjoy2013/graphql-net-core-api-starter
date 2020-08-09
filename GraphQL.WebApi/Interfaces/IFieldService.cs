using GraphQL.Types;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Interfaces
{
    public interface IFieldService
    {
        void ActivateFields(
            ObjectGraphType objectGraph,
            FieldServiceType fieldType,
            IWebHostEnvironment env,
            IServiceProvider provider);



        void RegisterFields();
    }

    public enum FieldServiceType
    {
        Query,
        Mutation,
        Subscription
    }
}
