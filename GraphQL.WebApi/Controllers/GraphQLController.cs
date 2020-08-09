using GraphQL.WebApi.Graph.Schema;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQL.WebApi.Controllers
{
    [Route("graphql")]
    public class GraphQLController : Controller
    {
        private IDocumentExecuter _documentExecuter;
        private GraphQLSchema _schema;
        private readonly IServiceProvider _provider;

        public GraphQLController(IDocumentExecuter documentExecuter, GraphQLSchema schema, IServiceProvider provider)
        {
            _provider = provider;
            _documentExecuter = documentExecuter;
            _schema = schema;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query)
        {
            if (query == null) { throw new ArgumentNullException(nameof(query)); }

            var inputs = query.Variables == null ? default(Inputs) : query.Variables.ToString().ToInputs();

            var executionOptions = new ExecutionOptions
            {
                Schema = _schema,
                Query = query.Query,
                OperationName = query.OperationName,
                Inputs = inputs               
            };
           

            var result = await _documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);

            if (result.Errors?.Count > 0)
            {
               

                var graphQLErrors = new List<string>();
                var errors = result.Errors.GetEnumerator();
                while (errors.MoveNext())
                {
                    graphQLErrors.Add(errors.Current.InnerException != null ? errors.Current.InnerException.Message : errors.Current.Message);
                }

                return BadRequest(new { result, graphQLErrors });
            }
            return Ok(result);
        }

    }
}
