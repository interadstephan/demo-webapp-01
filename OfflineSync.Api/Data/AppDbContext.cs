using Microsoft.EntityFrameworkCore;
using OfflineSync.Api.Models;

namespace OfflineSync.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<DataRecord> DataRecords { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }
    public DbSet<SyncMetadata> SyncMetadata { get; set; }
    public DbSet<MasterData> MasterData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Agent
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });

        // Configure DataRecord
        modelBuilder.Entity<DataRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AgentId, e.UpdatedAt });
            entity.HasIndex(e => e.Version);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.Agent)
                .WithMany(a => a.DataRecords)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FileAttachment
        modelBuilder.Entity<FileAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AgentId, e.UpdatedAt });
            entity.HasIndex(e => e.Version);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.BlobPath).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.Agent)
                .WithMany(a => a.FileAttachments)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.DataRecord)
                .WithMany(d => d.FileAttachments)
                .HasForeignKey(e => e.DataRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure SyncMetadata
        modelBuilder.Entity<SyncMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AgentId, e.DeviceId }).IsUnique();
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(200);
        });

        // Configure MasterData
        modelBuilder.Entity<MasterData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UpdatedAt);
            entity.HasIndex(e => e.Version);
            entity.HasIndex(e => new { e.Category, e.Key });
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
        });
    }
}
