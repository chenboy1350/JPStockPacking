using System;
using System.Collections.Generic;
using JPStockPacking.Data.SPDbContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext;

public partial class SPDbContext : DbContext
{
    public SPDbContext(DbContextOptions<SPDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignment { get; set; }

    public virtual DbSet<AssignmentMember> AssignmentMember { get; set; }

    public virtual DbSet<AssignmentTable> AssignmentTable { get; set; }

    public virtual DbSet<BreakDescription> BreakDescription { get; set; }

    public virtual DbSet<Lot> Lot { get; set; }

    public virtual DbSet<LotNotify> LotNotify { get; set; }

    public virtual DbSet<Order> Order { get; set; }

    public virtual DbSet<OrderNotify> OrderNotify { get; set; }

    public virtual DbSet<Received> Received { get; set; }

    public virtual DbSet<Returned> Returned { get; set; }

    public virtual DbSet<ReturnedDetail> ReturnedDetail { get; set; }

    public virtual DbSet<SendQtyToPack> SendQtyToPack { get; set; }

    public virtual DbSet<SendQtyToPackDetail> SendQtyToPackDetail { get; set; }

    public virtual DbSet<SendQtyToPackDetailSize> SendQtyToPackDetailSize { get; set; }

    public virtual DbSet<WorkTable> WorkTable { get; set; }

    public virtual DbSet<WorkTableMember> WorkTableMember { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_100_CI_AI");

        modelBuilder.Entity<Received>(entity =>
        {
            entity.HasKey(e => e.ReceivedId).HasName("PK_Receive");
        });

        modelBuilder.Entity<Returned>(entity =>
        {
            entity.HasKey(e => e.ReturnId).HasName("PK_Returned_1");
        });

        modelBuilder.Entity<ReturnedDetail>(entity =>
        {
            entity.HasKey(e => e.ReturnDetailId).HasName("PK_Returned");
        });

        modelBuilder.Entity<WorkTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
