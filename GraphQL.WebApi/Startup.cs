using GraphiQl;
using GraphQL.Server;
using GraphQL.WebApi.Graph.Mutation;
using GraphQL.WebApi.Graph.Query;
using GraphQL.WebApi.Graph.Schema;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Repository;
using GraphQL.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GraphQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.WebApi.Graph.Subscription;

namespace GraphQL.WebApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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

            services.AddGraphQL(o => { o.ExposeExceptions = _env.IsDevelopment(); })
              .AddGraphTypes(ServiceLifetime.Scoped)             
              .AddWebSockets();

            services.AddCors();
            services.AddGraphiQl(x =>
            {
                x.GraphiQlPath = "/graphiql-ui";
                x.GraphQlApiPath = "/graphql";
            });
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                       options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation($"ConnectionString: {Configuration["ConnectionString"]}");
            if (env.IsDevelopment())
            {
                using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();
                }
            }

            app.UseCors(builder =>
               builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseWebSockets();
            app.UseGraphQLWebSockets<GraphQLSchema>();
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());
            app.UseGraphiQl();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
