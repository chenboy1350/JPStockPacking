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

    public virtual DbSet<AssignmentReceived> AssignmentReceived { get; set; }

    public virtual DbSet<AssignmentTable> AssignmentTable { get; set; }

    public virtual DbSet<Break> Break { get; set; }

    public virtual DbSet<BreakDescription> BreakDescription { get; set; }

    public virtual DbSet<ComparedInvoice> ComparedInvoice { get; set; }

    public virtual DbSet<CustomerGroup> CustomerGroup { get; set; }

    public virtual DbSet<Export> Export { get; set; }

    public virtual DbSet<Lost> Lost { get; set; }

    public virtual DbSet<Lot> Lot { get; set; }

    public virtual DbSet<MappingCustomerGroup> MappingCustomerGroup { get; set; }

    public virtual DbSet<MappingPermission> MappingPermission { get; set; }

    public virtual DbSet<Melt> Melt { get; set; }

    public virtual DbSet<Order> Order { get; set; }

    public virtual DbSet<Permission> Permission { get; set; }

    public virtual DbSet<ProductType> ProductType { get; set; }

    public virtual DbSet<Received> Received { get; set; }

    public virtual DbSet<Returned> Returned { get; set; }

    public virtual DbSet<ReturnedDetail> ReturnedDetail { get; set; }

    public virtual DbSet<SendLost> SendLost { get; set; }

    public virtual DbSet<SendQtyToPack> SendQtyToPack { get; set; }

    public virtual DbSet<SendQtyToPackDetail> SendQtyToPackDetail { get; set; }

    public virtual DbSet<SendQtyToPackDetailSize> SendQtyToPackDetailSize { get; set; }

    public virtual DbSet<Store> Store { get; set; }

    public virtual DbSet<WorkTable> WorkTable { get; set; }

    public virtual DbSet<WorkTableMember> WorkTableMember { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_100_CI_AI");

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK_Assignment_1");
        });

        modelBuilder.Entity<AssignmentReceived>(entity =>
        {
            entity.HasKey(e => e.AssignmentReceivedId).HasName("PK_Assignment");
        });

        modelBuilder.Entity<Export>(entity =>
        {
            entity.HasKey(e => e.ExportId).HasName("PK_ExportDetailID");
        });

        modelBuilder.Entity<MappingPermission>(entity =>
        {
            entity.HasKey(e => e.MappingPermissionId).HasName("PK_MenuPermission");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK_Menu");
        });

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
