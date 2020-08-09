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

            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.creation_date).HasDefaultValueSql("(now())");
            });
        }
    }
}
