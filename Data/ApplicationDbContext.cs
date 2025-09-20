using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScoreTracker.Api.Models.Entities;

namespace ScoreTracker.Api.Data;

/// <summary>
/// The application's database context.
/// Inheriting from IdentityDbContext<ApplicationUser> automatically configures
/// the DbSets for Users, Roles, Claims, etc.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add our custom entities as DbSets
    public DbSet<Score> Scores { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // You can add any custom model configurations here if needed.
        // For example, setting up relationships, constraints, or seed data.
    }
}
