using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bsze3Blog.Models;

public partial class Bsze3BlogContext : DbContext
{
    private readonly IConfiguration _config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
    public bool DisableValueGeneratedOnAddOrUpdate { get; init; }

    public Bsze3BlogContext() : base() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseMySql(_config["ConnectionStrings:bsze3_blog"], ServerVersion.Parse("10.5.23-mariadb"));
            //.LogTo(Console.WriteLine, LogLevel.Information)
            //.EnableSensitiveDataLogging()
            //.EnableDetailedErrors();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        if (DisableValueGeneratedOnAddOrUpdate)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UpdatedAt)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<UserPersonalInformation>(entity =>
            {
                entity.Property(e => e.UpdatedAt)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<OldPassword>(entity =>
            {
                entity.Property(e => e.ExpiredAt)
                    .ValueGeneratedNever();
            });
        }
    }
}
