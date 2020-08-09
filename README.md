# Introduction

We will create a GraphQL api using dotnet core 3.1 webapi. For someone from dotnet framework and C# background, dotnet core is awesome due to it's platform independence. You can develop not only on Windows, but also on MacOS or Linux. This leads the way to create a Docker image of your app and host it on Kubernetes as a container orchestrator. 

Complete github repo is [here](https://github.com/killjoy2013/graphql-net-core-api-starter)

In this article, we'll setup our project structure and create a development environment. Here are the steps;

#### 1. Project creation
#### 2. Adding Postgresql using docker compose
#### 3. Adding Entity Framework Core. We'll be using code first
#### 4. Adding GraphQL. We'll create query,  mutation and also subscription.

In a second article, we'll be creating our own Docker image and deploy and run it on OpenShift 4.4 platform as our Kubernetes orchestrator.

Lets start...  

#### 1. Project creation
We'll be using Visual Studio 2019 as our IDE. Also we need Docker Desktop installed on our development machine. Now we need to check the installed dotnet core SDKs;


```
D:\>dotnet --list-sdks
2.2.103 [C:\Program Files\dotnet\sdk]
2.2.105 [C:\Program Files\dotnet\sdk]
3.0.101 [C:\Program Files\dotnet\sdk]
3.1.201 [C:\Program Files\dotnet\sdk]
3.1.302 [C:\Program Files\dotnet\sdk]
```

You can have different SDK versions installed. What we need is SDK 3.1. Now create a folder (mine is `graphql-net-core-api-starter`) and navigate there. Issue below cli commands to create necessary projects;
```
dotnet new webapi --name GraphQL.WebApi
dotnet new sln --name graphql-net-core-api-starter
```

Eventually, our solution structure should be like this;

![project-structure-init](https://dev-to-uploads.s3.amazonaws.com/i/uka74stf029cv9utjiab.PNG)

Make sure that the project file has `<TargetFramework>netcoreapp3.1</TargetFramework>`  

our launch settings is like below;

*graphql.webapi\Properties\launchSettings.json*

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "GraphQL.WebApi": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "ui/playground",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Our project's nuget package references are as follows,

```xml
<ItemGroup>
    <PackageReference Include="graphiql" Version="2.0.0" />
    <PackageReference Include="GraphQL.Server.Ui.Playground" Version="3.4.0" />
    <PackageReference Include="GraphQL" Version="2.4.0" />
    <PackageReference Include="GraphQL.Server.Transports.WebSockets" Version="3.4.0" />
    <PackageReference Include="GraphQL.Client.Abstractions.Websocket" Version="3.1.3" />
    <PackageReference Include="GraphQL.Server.Transports.AspNetCore" Version="3.4.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="3.1.6" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="3.1.6">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="3.1.4" />   
    <PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="3.1.6" />   
    <PackageReference Include="System.Reactive" Version="4.4.1" />   
    <PackageReference Include="System.Reactive.Compatibility" Version="4.4.1" />   
  </ItemGroup>
```


navigate to `graphql.webapi` and issue `dotnet run` you must have a running api on port 5000 now!

![running-5000](https://dev-to-uploads.s3.amazonaws.com/i/co941x4jt369mmosthxa.PNG)

#### 2. Adding Postgresql using docker compose

Let's add below `docker-compose.yaml` file;

```yaml
version: "3.3"
networks:
  graph-starter:
services:
  postgresql:
    restart: always
    image: postgres:12.2-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_USER=testuser
      - POSTGRES_PASSWORD=testpassword
      - POSTGRES_DB=graphdb
    volumes:
      - /var/lib/postgresql/data
    networks:
      - graph-starter    
```

In vscode terminal run the command `docker-compose up -d`  After pulling necessary packages from docker hub, our db will be ready. Check it out with the command `docker ps -a`  Its result is supposed to be like;

```
CONTAINER ID        IMAGE                  COMMAND                  CREATED             STATUS              PORTS                    NAMES
35ba1091da13        postgres:12.2-alpine   "docker-entrypoint.s…"   8 minutes ago       Up 8 minutes        0.0.0.0:5432->5432/tcp   graphql-net-core-api-starter_postgresql_1
```

We have a postgresql db without even installing it, wonderful!

#### 3. Adding Entity Framework Core

As models increases, repository methods have to be implemented in each repository. Since most of the implementations will be similar and repetitive, a rather generic approach is necessary. To achieve this goal, we will use generic repository pattern. Thereby all models will inherit existing methods and new ones can be added quickly.

As of dotnet core 3.0, you need to install dotnet-ef as separate tool;

```dotnet tool install --global dotnet-ef --version 3.1.6```

Now we'll create a base entity from which all the other domain models will be extended. We're assuming that all the models will have `id` as primary key and `creation_date` fields in database tables.

*GraphQL.WebApi\Interfaces\IEntity.cs*
```csharp
using System;
namespace GraphQL.WebApi.Interfaces
{
    public interface IEntity
    {
        int id { get; set; }
        DateTime? creation_date { get; set; }
    }
}
```

*GraphQL.WebApi\Models\BaseEntity.cs*
```csharp
using GraphQL.WebApi.Interfaces;
using System;
namespace GraphQL.WebApi.Models
{
    public abstract class BaseEntity : IEntity
    {
        public int id { get; set; }
        public DateTime? creation_date { get; set; }

    }
}
```

We can create our generic repository;

*GraphQL.WebApi\Interfaces\IGenericRepository.cs*
```csharp
using GraphQL.WebApi.Models;
using System.Collections.Generic;

namespace GraphQL.WebApi.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        T Insert(T entity);
        T Update(T entity);
        void Delete(int id);
    }
}

```

*GraphQL.WebApi\Repositories\GenericRepository.cs*
```csharp
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.WebApi.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly DatabaseContext context;
        private DbSet<T> entities;
        string errorMessage = string.Empty;
        public GenericRepository(DatabaseContext context)
        {
            this.context = context;
            entities = context.Set<T>();
        }
        public IEnumerable<T> GetAll()
        {
            return entities.AsEnumerable();
        }
        public T GetById(int id)
        {
            return entities.SingleOrDefault(s => s.id == id);
        }
        public T Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            entities.Add(entity);
            context.SaveChanges();
            return entity;
        }
        public T Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            context.SaveChanges();
            return entity;
        }
        public void Delete(int id)
        {
            T entity = entities.SingleOrDefault(s => s.id == id);
            entities.Remove(entity);
            context.SaveChanges();
        }
    }
}


```

We can now define the database context;

*GraphQL.WebApi\Repositories\DatabaseContext.cs*
```csharp
using GraphQL.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.WebApi.Repository
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>(entity =>
            {
                entity.Property(e => e.creation_date).HasDefaultValueSql("(now())");
               
            });


        }
    }
}
```
As the initial domain model `City` is added to database context. We'll be adding new domain models here. `OnModelCreating` is the correct place to further shape the models, set default values, create indexes etc.


Let's add `City` as our first model. Database schema and table name is supplied in table attribute,

*GraphQL.WebApi\Models\City.cs*
```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQL.WebApi.Models
{
    [Table("city", Schema = "business")]
    public partial class City: BaseEntity
    {
        public string name { get; set; }
        public int? population { get; set; }
    }
}
```

Basically, we're ready to create our first migration. However, we need a way to supply database connection info to database context in designtime. First add the connection string to application configuraion;

*GraphQL.WebApi\appsettings.Development.json*
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionString": "Server=localhost;Port=5432;Database=graphdb;Username=postgres;Password=postgres"
}
```

Then, create database context factory. We first read the db connection string from `appsettings.Development.json`. Then supply it to `DbContextOptionsBuilder`. Finally, create the `DatabaseContext` with it.

*GraphQL.WebApi\Repositories\DatabaseContextFactory.cs*
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace GraphQL.WebApi.Repository
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            var path = Directory.GetCurrentDirectory();
           
            var configuration = new ConfigurationBuilder()
               .SetBasePath(path)              
               .AddJsonFile($"appsettings.Development.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();           

            var connectionString = configuration["ConnectionString"];

            Console.WriteLine($"connectionString:{connectionString}");

            optionsBuilder.UseNpgsql(connectionString);
            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}

```

Let's build the project and create our initial migration,

```
D:\Dev\GITHUB\graphql-net-core-api-starter\GraphQL.WebApi>dotnet ef migrations add InitialCreate
```

If everything went alright, you'll have an initial migration like this,

![initial migration](https://dev-to-uploads.s3.amazonaws.com/i/fn5cjlxm9sooejemd0sz.PNG)

Let's add another model `Country`,

*GraphQL.WebApi\Models\Country.cs*
```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQL.WebApi.Models
{
    [Table("country", Schema = "business")]
    public partial class Country : BaseEntity
    {
        public string name { get; set; }
        public string continent { get; set; }
    }
}

```

Add this new model to `DatabaseContext.cs`

```csharp
 modelBuilder.Entity<Country>(entity =>
{
    entity.Property(e => e.creation_date).HasDefaultValueSql("(now())");
});
```


Run the command `dotnet ef migrations add AddCountry` to add a new migration which includes necessary db schema update on top of `InitialCreate` migration.

![country-migration](https://dev-to-uploads.s3.amazonaws.com/i/kof3ipomxwkbg15rw9ih.PNG)

Very well. Suddenly realized that we forgot to add foreign key relation between **City** and **Country** tables. We need to remove lastly added migration running command `dotnet ef migrations remove` This will pops out the last migration and it also disapears from **Migrations** folder.

We can now make the necessary ammendments to the models and create a brand new migration. Updated `City.cs` is like this,

```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQL.WebApi.Models
{
    [Table("city", Schema = "business")]
    public partial class City: BaseEntity
    {
        public string name { get; set; }
        public int? population { get; set; }
        public int country_id { get; set; }

        [ForeignKey("country_id")]
        public Country Country { get; set; }
    }
}
```

Note the `ForeignKey` attribute. It'll add `county_id` column to `city` table and create a foreign key relation with primary key `id` column of `country` table.
Let's reflect all what we've done so far to database. Issue the command `dotnet ef database update` If you're lucky enough, your db is supposed to be updated with these new domain models.

![city_country](https://dev-to-uploads.s3.amazonaws.com/i/fgjqnvnnyoylppd2x5ya.PNG)

As seen here, graphql db updated with `city` and `country` tables. I'm using [dbeaver](https://dbeaver.io/) for db operations. A fantastic free tool. Highly recommend it. 


Finally, we need to update `Startup.cs` like below to add the `DatabaseContext` to services collection and register `GenericRepository` to dependency injection.

```csharp
 public void ConfigureServices(IServiceCollection services)
  {
      services.AddDbContext<DatabaseContext>(options =>
      {
          options.UseNpgsql(Configuration["ConnectionString"]);
      });
      services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
      services.AddControllers();
  }
```

If you'd like to have all the migrations executed on your db when the service starts, add below part to `Startup.cs` `Configure`

```csharp
  if (env.IsDevelopment())
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();
            }
        }
```

Try `dotnet run` Your webapi is still supposed to run on port 5000. 

#### 4. Adding GraphQL

We'll create our GraphQL schema that consist Query, Mutation & Subscription. We'll add pretty many stuff here. After adding necessary folders, project structure is as follows,

![graph_solution-folders](https://dev-to-uploads.s3.amazonaws.com/i/ntzjt3b1gxnxv88sc2qz.PNG)

Let's start adding our `MainMutation`

*GraphQL.WebApi\Graph\Mutation\MainMutation.cs*
```csharp
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
```

`MainMutation` is a top level entity (like `MainQuery` and `MainSubscription`) to be added to GraphQL schema. Normally it includes all mutation definitions in it. i.e., you may have tens of mutations defined here. For a trivial project, that would be quiet alright. However, as the project gets larger and the number of mutations & queries increase, this becomes a problem. Let's say you have a large developers team. Everybody will be updating the same file when they update a mutation. The same is valid for query development. This would lead to merge conflicts when developments are to push to GIT. It's not actually very good in terms of transparency. You'll not be able to see all the mutations and queries at a glance. It's much leaner to create separate files for each schema items. This brings a question, how to load these schema items? Answer is `IFieldService`

*GraphQL.WebApi\Interfaces\IFieldService.cs*
```csharp
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

```

*GraphQL.WebApi\Services\FieldService.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.WebApi.Services
{
    public class FieldService : IFieldService
    {
        private IDictionary<FieldServiceType, IList<IFieldServiceItem>> _fieldTable;
        private readonly ISubscriptionServices _subscriptionServices;

        public FieldService(ISubscriptionServices subscriptionServices)
        {
            _subscriptionServices = subscriptionServices;
            _fieldTable = new Dictionary<FieldServiceType, IList<IFieldServiceItem>>();
            _fieldTable.Add(FieldServiceType.Mutation, new List<IFieldServiceItem>());
            _fieldTable.Add(FieldServiceType.Query, new List<IFieldServiceItem>());
            _fieldTable.Add(FieldServiceType.Subscription, new List<IFieldServiceItem>());
        }

        public void ActivateFields(
            ObjectGraphType objectGraph,
            FieldServiceType fieldType,
            IWebHostEnvironment env,
            IServiceProvider provider)
        {

            var serviceItemList = _fieldTable[fieldType];

            foreach (var serviceItem in serviceItemList)
            {
                serviceItem.Activate(objectGraph, env, provider);
            }
        }

        public void RegisterFields()
        {
            var type = typeof(IFieldServiceItem);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            foreach (var fieldType in types)
            {
                if (fieldType.IsClass)
                {
                    if (typeof(IFieldMutationServiceItem).IsAssignableFrom(fieldType))
                    {
                        _fieldTable[FieldServiceType.Mutation].Add((IFieldServiceItem)Activator.CreateInstance(fieldType));
                    }
                    else if (typeof(IFieldQueryServiceItem).IsAssignableFrom(fieldType))
                    {
                        _fieldTable[FieldServiceType.Query].Add((IFieldServiceItem)Activator.CreateInstance(fieldType));
                    }
                    else if (typeof(IFieldSubscriptionServiceItem).IsAssignableFrom(fieldType))
                    {
                        _fieldTable[FieldServiceType.Subscription].Add((IFieldServiceItem)Activator.CreateInstance(fieldType));
                    }

                }
            }
        }
    }
}

```
Now we register this `FieldService` in Startup.cs
```csharp 
services.AddScoped<IFieldService, FieldService>();
```

Let's see our `GraphQLSchema`

*GraphQL.WebApi\Graph\Schema\GraphQLSchema.cs*
```csharp
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

```
In the constructor, first thing to do is to resolve `IFieldService` and call its `RegisterFields()` method. What this does is, to collect all the classes which implements `IFieldQueryServiceItem` and keep them in `_fieldTable` of the `FieldService` Then `MainMutation`, `MainQuery` and `MainSubscription` are resolved. 

Let's see MainQuery & MainSubscription as well,

*GraphQL.WebApi\Graph\Query\MainQuery.cs*
```csharp
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

```

*GraphQL.WebApi\Graph\Subscription*
```csharp
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

```

Now we need to register them in Startup.cs
```csharp
 services.AddScoped<MainMutation>();
 services.AddScoped<MainQuery>();
```

Let's go over the mutations. They all implement `IFieldMutationServiceItem` and has to implement its `Activate` method. This leads to standardization and very clean architecture. Our first mutation is `addCountry` and it expects a mandadory parameter `countryName`. Each query, mutation & subscrition has its own resolver where they carry out the business logic. `addCountry` mutation firts receives the parameters and simply inserts new  country in the resolver. 

*GraphQL.WebApi\Graph\Mutation\AddCountryMutation.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;


namespace GraphQL.WebApi.Graph.Mutation
{    public class AddCountryMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<CountryGType>("addCountry",
            arguments: new QueryArguments(               
               new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "countryName" }
            ),
            resolve: context =>
            {                
                var countryName = context.GetArgument<string>("countryName");
                var countryRepository = (IGenericRepository<Country>)sp.GetService(typeof(IGenericRepository<Country>));

                var newCountry = new Country
                {
                    name = countryName
                };

                return countryRepository.Insert(newCountry);
            });
        }
    }
}

```
`addCity` mutation expects `countryId`, `cityName` mandadory parameters and an optional parameter `population`. After obtaining the parameters it creates a new city. It also notifies `CityAddedService` which we'll cover shortly.

*GraphQL.WebApi\Graph\Mutation\AddCityMutation.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Dto;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class AddCityMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<CityGType>("addCity",
            arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "countryId" },
               new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "cityName" },
               new QueryArgument<IntGraphType> { Name = "population" }
            ),
            resolve: context =>
            {                
                var countryId = context.GetArgument<int>("countryId");
                var cityName = context.GetArgument<string>("cityName");
                var population = context.GetArgument<int?>("population");

                var subscriptionServices = (ISubscriptionServices)sp.GetService(typeof(ISubscriptionServices));
                var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                var countryRepository = (IGenericRepository<Country>)sp.GetService(typeof(IGenericRepository<Country>));

                var foundCountry = countryRepository.GetById(countryId);

                var newCity = new City
                {
                    name = cityName,
                    country_id = countryId,
                    population=population
                };

                var addedCity = cityRepository.Insert(newCity);
                subscriptionServices.CityAddedService.AddCityAddedMessage(new CityAddedMessage
                {
                    cityName = addedCity.name,
                    countryName = foundCountry.name,
                    id = addedCity.id,
                    message = "A new city added"
                });
                return addedCity;

            });
        }
    }
}

```

`addCountry` and `addCity` mutations return `CountryGType` and `CityGType` respectively. Let's go over them,

*GraphQL.WebApi\Graph\Type\CityGType.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using System;

namespace GraphQL.WebApi.Graph.Type
{
    public class CityGType : ObjectGraphType<City>
    {
        public IServiceProvider Provider { get; set; }
        public CityGType(IServiceProvider provider)
        {
            Field(x => x.id, type: typeof(IntGraphType));
            Field(x => x.name, type: typeof(StringGraphType));
            Field(x => x.population, type: typeof(IntGraphType));
            Field<CountryGType>("country", resolve: context => {
                IGenericRepository<Country> countryRepository = (IGenericRepository<Country>)provider.GetService(typeof(IGenericRepository<Country>));
                return countryRepository.GetById(context.Source.country_id);
            });
        }
    }
}

```

*GraphQL.WebApi\Graph\Type\CountryGType.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using System;
using System.Linq;

namespace GraphQL.WebApi.Graph.Type
{
    public class CountryGType : ObjectGraphType<Country>
    {
        public IServiceProvider Provider { get; set; }
        public CountryGType(IServiceProvider provider)
        {
            Field(x => x.id, type: typeof(IntGraphType));
            Field(x => x.name, type: typeof(StringGraphType));
            Field<ListGraphType<CityGType>>("cities", resolve: context => {
                IGenericRepository<City> cityRepository = (IGenericRepository<City>)provider.GetService(typeof(IGenericRepository<City>));
                return cityRepository.GetAll().Where(w=> w.Country.id ==  context.Source.id);
            });
        }
    }
}

```


All the types which will be returned from our GraphQL api are supposed to extend `ObjectGraphType` If they will have some additional properties that are not originally in the base model, like `cities` property in `CountryGType`, we need to create a resolver for them. We add them to DI as follows,

```csharp
    services.AddScoped<CityGType>();
    services.AddScoped<CountryGType>();
```

Our other two mutations are `deleteCity` and `updateCity`

*GraphQL.WebApi\Graph\Mutation\DeleteCityMutation.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class DeleteCityMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<StringGraphType>("deleteCity",
            arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "cityId" }               
            ),
            resolve: context =>
            {
                var cityId = context.GetArgument<int>("cityId");
                var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                cityRepository.Delete(cityId);
                return $"cityId:{cityId} deleted";
            });
        }
    }
}
```

`deleteCity` is very simple. It's receives `cityId` mandadory parameter and deletes the city entity through `cityRepository`.

*GraphQL.WebApi\Graph\Mutation\UpdateCityMutation.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;

namespace GraphQL.WebApi.Graph.Mutation
{
    public class UpdateCityMutation : IFieldMutationServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<CityGType>("updateCity",
            arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "cityId" },
               new QueryArgument<IntGraphType> { Name = "countryId" },
               new QueryArgument<StringGraphType> { Name = "cityName" },
               new QueryArgument<IntGraphType> { Name = "population" }
            ),
            resolve: context =>
            {
                var cityId = context.GetArgument<int>("cityId");
                var countryId = context.GetArgument<int?>("countryId");
                var cityName = context.GetArgument<string>("cityName");
                var population = context.GetArgument<int?>("population");

                var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                var foundCity = cityRepository.GetById(cityId);

                if (countryId != null)
                {
                    foundCity.country_id = countryId.Value;
                }
                if (!String.IsNullOrEmpty(cityName))
                {
                    foundCity.name = cityName;
                }
                if (population != null)
                {
                    foundCity.population = population.Value;
                }

                return cityRepository.Update(foundCity);
            });
        }
    }
}

```

`updateCity` expects a mandatory `cityId` of the entity to be updated and three optional parameters. We can update any of the `countryId`,`cityName` & `population` properties.

Let's see our two queries,

*GraphQL.WebApi\Graph\Query\Business\CityQuery.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace GraphQL.WebApi.Graph.Query.Business
{
    public class CityQuery : IFieldQueryServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<ListGraphType<CityGType>>("cities",
               arguments: new QueryArguments(
                 new QueryArgument<StringGraphType> { Name = "name" }
               ),
               resolve: context =>
               {            
                   var cityRepository = (IGenericRepository<City>)sp.GetService(typeof(IGenericRepository<City>));
                   var baseQuery = cityRepository.GetAll();
                   var name = context.GetArgument<string>("name");
                   if (name != default(string))
                   {
                       return baseQuery.Where(w => w.name.Contains(name));
                   }
                   return baseQuery.ToList();
               });
        }
    }
}

```

*GraphQL.WebApi\Graph\Query\Business\CountryQuery.cs*
```csharp
using GraphQL.Types;
using GraphQL.WebApi.Graph.Type;
using GraphQL.WebApi.Interfaces;
using GraphQL.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace GraphQL.WebApi.Graph.Query.Business
{
    public class CountryQuery : IFieldQueryServiceItem
    {
        public void Activate(ObjectGraphType objectGraph, IWebHostEnvironment env, IServiceProvider sp)
        {
            objectGraph.Field<ListGraphType<CountryGType>>("countries",
               arguments: new QueryArguments(
                 new QueryArgument<StringGraphType> { Name = "name" }
               ),
               resolve: context =>
               {
                   var countryRepository = (IGenericRepository<Country>)sp.GetService(typeof(IGenericRepository<Country>));
                   var baseQuery = countryRepository.GetAll();
                   var name = context.GetArgument<string>("name");
                   if (name != default(string))
                   {
                       return baseQuery.Where(w => w.name.Contains(name));
                   }
                   return baseQuery.ToList();
               });
        }
    }
}

```

Both receives `name` parameter and execute related Linq queries.

### GraphQL Subscriptions

Sometimes we need to notify the clientside when a certain operation (query / mutation) is carried out on the api. GraphQL Subscription makes use of **websocket** technology. We already installed 

```<PackageReference Include="GraphQL.Server.Transports.WebSockets" Version="3.4.0" />``` 

nuget package for this purpose. 
We configure GraphQL service and add websocket in `Configure` like this,

```csharp
  services.AddGraphQL(o => { o.ExposeExceptions = _env.IsDevelopment(); })
    .AddGraphTypes(ServiceLifetime.Scoped)             
    .AddWebSockets();
```


We've seen that we add `Subscription = resolver.Resolve<MainSubscription>();` in our `GraphQLSchema.cs`

Let's say we'd like to get notified when a new city added to country `Germany` We don't need any notification in adding city to other countries.
First we create a subscription `SubscriptionServices` and register as singleton in `Startup.cs` as 

```services.AddSingleton<ISubscriptionServices, SubscriptionServices>();```

*GraphQL.WebApi\Services\SubscriptionServices.cs*
```csharp
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

```

*GraphQL.WebApi\Services\CityAddedService.cs*
```csharp
using GraphQL.WebApi.Dto;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GraphQL.WebApi.Services
{
    public class CityAddedService
    {
        private readonly ISubject<CityAddedMessage> _messageStream = new ReplaySubject<CityAddedMessage>(1);
        public CityAddedMessage AddCityAddedMessage(CityAddedMessage message)
        {
            _messageStream.OnNext(message);
            return message;
        }

        public IObservable<CityAddedMessage> GetMessages(string countryName)
        {
            var mess = _messageStream
                .Where(message =>
                    message.countryName == countryName
                ).Select(s => s)
                .AsObservable();

            return mess;
        }
    }
}

```

`CityAddedService` uses `System.Reactive.Linq` and `System.Reactive.Subjects`. Here we've created a `ReplaySubject` which will hold only the last one `CityAddedMessage`, without storing the old messages. It's also important to note that `GetMessages` method will be called with `countryName` parameter as we need to decide if the city has been added to the county we just subscribed. We'll be making use of this `GetMessages` method in `CityAddedSubscription`

*GraphQL.WebApi\Graph\Subscription\CityAddedSubscription.cs*
```csharp
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

```

As you see, `CityAddedSubscription` is another schema object like queries & mutaions. So, it also implements `IFieldSubscriptionServiceItem` Interesting part is its `Subscriber` resolver. Here we obtain the relevant massage from `CityAddedService` using its aforementioned `GetMessages` method.

That much talk is enough I guess. Let's try out our api to see it in action. Before that, let me tell you about two wonderful tool which we use in querying our GraphQL api. [graphiql](https://github.com/graphql/graphiql) and [graphql-playground](https://github.com/prisma-labs/graphql-playground) You can use any one of them. We'are making use both to see them in action. Already installed the two necessary nuget packages;

```xml
 <PackageReference Include="graphiql" Version="2.0.0" />
 <PackageReference Include="GraphQL.Server.Ui.Playground" Version="3.4.0" />
```

We add graphiql by adding below part in `ConfigureServices` of `Startup.cs`

```csharp
    services.AddGraphiQl(x =>
        {
            x.GraphiQlPath = "/graphiql-ui";
            x.GraphQlApiPath = "/graphql";
        });
```

and making use of it like `app.UseGraphiQl();` in `Configure`

For **graphql-playground**, we add `app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());` in `Configure`
One final note, we need to prevent circular referencing error by adding


```csharp
    services.AddControllers()
        .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        );
```

## Demo time...

Let's build the project in VS2019 and hit F5 to run. Your browser will be opened in `http://localhost:5000/ui/playground` displaying **playground**  as we set the `launchUrl` in `launchSettings.json`
Open another browser and navigate to `http://localhost:5000/graphiql-ui`  Now you have `graphiql` You're supposed to see the GraphQL schema with all the elements in both tools.

Try
```json
query countries{
  countries{
    id
    name
  }
}
```
You'll see no data as there's not yet any country. Let's add some,

```json
mutation addCountry{
  addCountry(countryName:"France"){
    id
    name
  }
}
mutation addCountry{
  addCountry(countryName:"Germany"){
    id
    name
  }
}
```

When we query countries again you'll see France & Germany,
```json
{
  "data": {
    "countries": [
      {
        "id": 1,
        "name": "France"
      },
      {
        "id": 2,
        "name": "Germany"
      }
    ]
  }
}
```

We can add some cities,

```json

mutation addCityToFrance{
  addCity(countryId:1,cityName:"Paris",population:123000){
    id
    name
    population
    country{
      id
      name
    }
  }
}

mutation addCityToGermany{
  addCity(countryId:2,cityName:"Köln",population:500000){
    id
    name
    population
    country{
      id
      name
    }
  }
}
```
and query cities,
```json
query cities{
  cities{
    id
    name
    population
    country{
      id
      name
    }
  }
}
```
Our cities so far,
```json
{
  "data": {
    "cities": [
      {
        "id": 1,
        "name": "Paris",
        "population": 123000,
        "country": {
          "id": 1,
          "name": "France"
        }
      },
      {
        "id": 2,
        "name": "Köln",
        "population": 500000,
        "country": {
          "id": 2,
          "name": "Germany"
        }
      }
    ]
  }
}
```

You can simply test `DeleteCityMutation` and `UpdateCityMutation` in a similar way. 
Let's test the most funny part, `CityAddedSubscription` Restart webapi. You'll navigate to `http://localhost:5000/ui/playground` Open another browser and navigate to `http://localhost:5000/graphiql-ui/`
Use playground to issue 

```json
subscription cityAddedToGermany{
  cityAdded(countryName:"Germany"){
    id
    cityName
    countryName
    message
  }
}
```

Apparently, we've subscribed to get notified when a city added to Germany. Playground enters into listening mode,

![listening](https://dev-to-uploads.s3.amazonaws.com/i/gd8y94bnxyvc7lpui00e.PNG)

Now let's add a city to Germany using graphiql

![hamburg_added](https://dev-to-uploads.s3.amazonaws.com/i/5xicd85c9pj94jagw0c2.PNG)

Once we execute `addCityToGermany` we'll be notified in playground like follows,

![add-city-notified](https://dev-to-uploads.s3.amazonaws.com/i/loolsvgkby35pm7nx4zu.PNG)

Now you can try to add city to France. You're supposed not to see any notification in playground this time!

Using subscription in GraphQL saves us from using additional libraries to use websockets. It also simplifies development as we'll be writing our codes in GraphQL ecosystem. GraphQL makes the heavy lifting for us.

In second part of this article, we will
- add SSL support
- create our Docker image 
- and deploy it to OpenShift

Happy coding :-)





