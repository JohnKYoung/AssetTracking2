using Microsoft.EntityFrameworkCore;
using AssetTracking2.Models;

namespace AssetTracking2.Data
{
    // AppDbContext is EF Core's connection between the C# asset classes and SQL Server.
    public class AppDbContext : DbContext
    {
        // DbSet properties let the app query and save each kind of asset.
        // The derived asset types share the Asset table hierarchy in EF Core.
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<Laptop> Laptops { get; set; } = null!;
        public DbSet<DesktopComputer> DesktopComputers { get; set; } = null!;
        public DbSet<IPhone> IPhones { get; set; } = null!;
        public DbSet<Samsung> Samsungs { get; set; } = null!;
        public DbSet<Nokia> Nokias { get; set; } = null!;
        public DbSet<Tablet> Tablets { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Read the full SQL Server connection string from the environment so
            // database credentials are not stored in source code.
            var connectionString = Environment.GetEnvironmentVariable("ASSETTRACKING_SQL_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Set the ASSETTRACKING_SQL_CONNECTION_STRING environment variable before running the app.");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
