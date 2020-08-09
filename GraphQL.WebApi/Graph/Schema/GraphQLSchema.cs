using GraphQL.WebApi.Graph.Mutation;
using GraphQL.WebApi.Graph.Query;
using GraphQL.WebApi.Graph.Subscription;
using GraphQL.WebApi.Interfaces;

namespace GraphQL.WebApi.Graph.Schema
{
    public class GraphQLSchema : GraphQL.Types.Schema
    {
        public GraphQLSchema(IDependencyResolver resolver) : base(resolver)
        {
            var fieldService = resolver.Resolve<IFieldService>();
            fieldService.RegisterFields();
            Mutation = resolver.Resolve<MainMutation>();
            Query = resolver.Resolve<MainQuery>();
            Subscription = resolver.Resolve<MainSubscription>();           
        }
    }
}
