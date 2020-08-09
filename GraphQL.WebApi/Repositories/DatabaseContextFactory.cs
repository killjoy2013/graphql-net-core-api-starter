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
