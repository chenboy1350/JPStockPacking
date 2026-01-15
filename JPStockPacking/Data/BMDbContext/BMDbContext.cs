using System;
using System.Collections.Generic;
using JPStockPacking.Data.BMDbContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.BMDbContext;

public partial class BMDbContext : DbContext
{
    public BMDbContext(DbContextOptions<BMDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<OrderDetail> OrderDetail { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC2741B8C09B");

            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.Box).HasDefaultValue("");
            entity.Property(e => e.Cancle).HasDefaultValue("False");
            entity.Property(e => e.ChkComp).HasDefaultValue("");
            entity.Property(e => e.ChkIn).HasDefaultValue("");
            entity.Property(e => e.ChkOut).HasDefaultValue("");
            entity.Property(e => e.ChkPay).HasDefaultValue("");
            entity.Property(e => e.Core).HasDefaultValue("");
            entity.Property(e => e.CustCode).HasDefaultValue("");
            entity.Property(e => e.EmpCode).HasDefaultValue("");
            entity.Property(e => e.EmpName).HasDefaultValue("");
            entity.Property(e => e.Finish).HasDefaultValue("False");
            entity.Property(e => e.ListNo).HasDefaultValue("");
            entity.Property(e => e.MaskAndBox).HasDefaultValue("");
            entity.Property(e => e.OrdBarcode).HasDefaultValue("");
            entity.Property(e => e.OrderNo).HasDefaultValue("");
            entity.Property(e => e.Picture).HasDefaultValue("");
            entity.Property(e => e.PricePerUnit).HasDefaultValueSql("('')");
            entity.Property(e => e.RefBarcode).HasDefaultValue("");
            entity.Property(e => e.RefNo).HasDefaultValue("");
            entity.Property(e => e.Ring).HasDefaultValue("");
            entity.Property(e => e.ShipDate).HasDefaultValueSql("('')");
            entity.Property(e => e.Sr).HasDefaultValue("");
            entity.Property(e => e.TypeUnit).HasDefaultValue("");
            entity.Property(e => e.UserNameCreate).HasDefaultValue("");
            entity.Property(e => e.UserNameEdit).HasDefaultValue("");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
