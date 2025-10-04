using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Peel.Data.Entities;

namespace Peel.Data.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SearchHistory> SearchHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(e => e.SearchId).HasName("search_history_pkey");

            entity.ToTable("search_history");

            entity.Property(e => e.SearchId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("search_id");
            entity.Property(e => e.AmountRange).HasColumnName("amount_range");
            entity.Property(e => e.MaxSpread).HasColumnName("max_spread");
            entity.Property(e => e.MinReputation).HasColumnName("min_reputation");
            entity.Property(e => e.OccurredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("occurred_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
