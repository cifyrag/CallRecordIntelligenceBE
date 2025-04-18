using CallRecordIntelligence.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace CallRecordIntelligence.EF;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<CallRecord> CallRecords { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CallRecord>()
            .Property(b => b.Inserted)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<CallRecord>()
            .Property(b => b.LastUpdated)
            .HasDefaultValueSql("now()");
    }
}
