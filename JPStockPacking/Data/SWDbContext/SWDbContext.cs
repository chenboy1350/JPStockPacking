using System;
using System.Collections.Generic;
using JPStockPacking.Data.SWDbContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext;

public partial class SWDbContext : DbContext
{
    public SWDbContext(DbContextOptions<SWDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Borrow> Borrow { get; set; }

    public virtual DbSet<BorrowDetail> BorrowDetail { get; set; }

    public virtual DbSet<BorrowTrayDeduction> BorrowTrayDeduction { get; set; }

    public virtual DbSet<Break> Break { get; set; }

    public virtual DbSet<BreakDetail> BreakDetail { get; set; }

    public virtual DbSet<MappingPermission> MappingPermission { get; set; }

    public virtual DbSet<Permission> Permission { get; set; }

    public virtual DbSet<Stock> Stock { get; set; }

    public virtual DbSet<Tray> Tray { get; set; }

    public virtual DbSet<TrayItem> TrayItem { get; set; }

    public virtual DbSet<Withdrawal> Withdrawal { get; set; }

    public virtual DbSet<WithdrawalDetail> WithdrawalDetail { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_100_CI_AI");

        modelBuilder.Entity<Break>(entity =>
        {
            entity.HasKey(e => e.BreakNo).HasName("PK_Break_1");
        });

        modelBuilder.Entity<BreakDetail>(entity =>
        {
            entity.HasKey(e => e.BreakDetailId).HasName("PK_Break");
        });

        modelBuilder.Entity<MappingPermission>(entity =>
        {
            entity.HasKey(e => e.MappingPermissionId).HasName("PK_MenuPermission");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK_Menu");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.StockId).HasName("PK_Receive");
        });

        modelBuilder.Entity<Tray>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TrayItem>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Tray).WithMany(p => p.TrayItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrayItem_Tray");
        });

        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.HasKey(e => e.WithdrawalNo).HasName("PK_Withdrawal_1");
        });

        modelBuilder.Entity<WithdrawalDetail>(entity =>
        {
            entity.HasKey(e => e.WithdrawalDetailId).HasName("PK_Withdrawal");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
