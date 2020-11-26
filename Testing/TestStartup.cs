using GraphQL;
using GraphQL.Server;
using GraphQL.WebApi.Graph.Mutation;
using GraphQL.WebApi.Graph.Query;
using GraphQL.WebApi.Graph.Schema;
using GraphQL.WebApi.Graph.Subscription;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Repository;
using GraphQL.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Testing
{
    public class TestStartup
    {
        private readonly IWebHostEnvironment _env;
        public TestStartup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(Configuration["ConnectionString"]);
            });
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IFieldService, FieldService>();
            services.AddScoped<IDocumentExecuter, DocumentExecuter>();

            services.AddScoped<MainMutation>();
            services.AddScoped<MainQuery>();
            services.AddScoped<CityGType>();
            services.AddScoped<CountryGType>();

            services.AddSingleton<ISubscriptionServices, SubscriptionServices>();
            services.AddScoped<MainSubscription>();

            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddScoped<GraphQLSchema>();

            services.AddGraphQL()
              .AddGraphTypes(ServiceLifetime.Scoped);
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<TestStartup>();
            logger.LogInformation($"ConnectionString: {Configuration["ConnectionString"]}");

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();
            }

            //app.UseCors(builder =>
            //   builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());            
                   
            app.UseRouting();
            
        }
    }
}
