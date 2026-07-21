using FF_ModelsDB;
using Microsoft.EntityFrameworkCore;

namespace FF_DataDB;

public class FeedFlowDbContext : DbContext
{
    public FeedFlowDbContext()
    {
    }

    public FeedFlowDbContext(DbContextOptions<FeedFlowDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Source> Sources { get; set; } = null!;
    public virtual DbSet<SourceItem> SourceItems { get; set; } = null!;
    public virtual DbSet<SourceSecret> SourceSecrets { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS02;Database=FeedFlowDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Source>(entity =>
        {
            entity.ToTable("Sources");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ComponentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RequiresSecret).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LastFetchedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<SourceItem>(entity =>
        {
            entity.ToTable("SourceItems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Json).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(e => e.Source)
                .WithMany(s => s.SourceItems)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SourceSecret>(entity =>
        {
            entity.ToTable("SourceSecrets");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.KeyName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.KeyValue).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Location).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Source)
                .WithMany(s => s.Secrets)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
