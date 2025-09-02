using System;
using System.Collections.Generic;
using JPStockPacking.Data.JPDbContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext;

public partial class JPDbContext : DbContext
{
    public JPDbContext(DbContextOptions<JPDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CpriceSale> CpriceSale { get; set; }

    public virtual DbSet<Cprofile> Cprofile { get; set; }

    public virtual DbSet<JobBillSendStock> JobBillSendStock { get; set; }

    public virtual DbSet<JobCost> JobCost { get; set; }

    public virtual DbSet<JobOrder> JobOrder { get; set; }

    public virtual DbSet<OrdDorder> OrdDorder { get; set; }

    public virtual DbSet<OrdHorder> OrdHorder { get; set; }

    public virtual DbSet<OrdLotno> OrdLotno { get; set; }

    public virtual DbSet<OrdOrder> OrdOrder { get; set; }

    public virtual DbSet<Spdreceive> Spdreceive { get; set; }

    public virtual DbSet<Sphreceive> Sphreceive { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CpriceSale>(entity =>
        {
            entity.HasKey(e => e.Barcode)
                .IsClustered(false)
                .HasFillFactor(90);

            entity.ToTable("CPriceSale", "dbo", tb => tb.HasTrigger("CpriceSale_Trigger"));

            entity.HasIndex(e => e.Article, "CPriceSale4")
                .IsClustered()
                .HasFillFactor(90);

            entity.HasIndex(e => e.Barcode, "IX_CPriceSale")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.EpoxyColor, e.FnCode }, "IX_CPriceSale_1").HasFillFactor(90);

            entity.HasIndex(e => e.LinkBar, "IX_CPriceSale_2").HasFillFactor(90);

            entity.Property(e => e.ArtCode).HasDefaultValue("");
            entity.Property(e => e.ChkFinish).HasDefaultValue(1);
            entity.Property(e => e.ComCode).HasDefaultValue("0");
            entity.Property(e => e.ComputerName).HasDefaultValue("");
            entity.Property(e => e.DisCode).HasDefaultValue("0");
            entity.Property(e => e.EpoxyColor).HasDefaultValue("");
            entity.Property(e => e.FactoryCode).HasDefaultValue("0");
            entity.Property(e => e.FactorycodeOld).HasDefaultValue("");
            entity.Property(e => e.FnCode).HasDefaultValue("");
            entity.Property(e => e.FngemCode).HasDefaultValue("");
            entity.Property(e => e.LinkBar).HasDefaultValue("");
            entity.Property(e => e.ListGem).HasDefaultValue("");
            entity.Property(e => e.ListMat).HasDefaultValue("");
            entity.Property(e => e.Picture).HasDefaultValue("");
            entity.Property(e => e.PictureC).HasDefaultValue("");
            entity.Property(e => e.PictureL).HasDefaultValue("");
            entity.Property(e => e.PictureM).HasDefaultValue("");
            entity.Property(e => e.PictureR).HasDefaultValue("");
            entity.Property(e => e.PictureS).HasDefaultValue("");
            entity.Property(e => e.ProductType).HasDefaultValue(1);
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.RingSize).HasDefaultValue("");
            entity.Property(e => e.TdesFn).HasDefaultValue("");
            entity.Property(e => e.UserName).HasDefaultValue("");

            entity.HasOne(d => d.ArticleNavigation).WithMany(p => p.CpriceSale)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CPriceSale_CProfile");
        });

        modelBuilder.Entity<Cprofile>(entity =>
        {
            entity.HasKey(e => e.Article).HasFillFactor(90);

            entity.ToTable("CProfile", "dbo", tb =>
                {
                    tb.HasTrigger("Cprofile_Del");
                    tb.HasTrigger("trg_CProfile_Insert");
                });

            entity.HasIndex(e => e.Article, "IX_CProfile")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.Article, e.SupArticle }, "IX_CProfile_1")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.ArtCode, e.List }, "IX_CProfile_2")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.CreaDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GemType).HasDefaultValue(4);
            entity.Property(e => e.Idpic).HasDefaultValue("");
            entity.Property(e => e.LinkArticle)
                .HasDefaultValue("")
                .HasComment("รหัสงานใหม่ (Z)");
            entity.Property(e => e.LinkArticle1)
                .HasDefaultValue("")
                .HasComment("รหัสงาน(เปลี่ยนรหัสใหม่)");
            entity.Property(e => e.MarkCenter).HasDefaultValue("");
            entity.Property(e => e.PendantType).HasDefaultValue("");
            entity.Property(e => e.PictureScale).HasDefaultValue("");
            entity.Property(e => e.SupArticle).HasDefaultValue("");
        });

        modelBuilder.Entity<JobBillSendStock>(entity =>
        {
            entity.Property(e => e.ItemSend).HasDefaultValue("");
            entity.Property(e => e.Doc).HasDefaultValue("");
            entity.Property(e => e.MdateSend).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Numsend).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<JobCost>(entity =>
        {
            entity.HasKey(e => new { e.Orderno, e.Lotno }).HasFillFactor(90);

            entity.Property(e => e.Orderno).HasDefaultValue("");
            entity.Property(e => e.Lotno)
                .HasDefaultValue("")
                .HasComment("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.List1).HasDefaultValue("");
            entity.Property(e => e.List2).HasDefaultValue("");
            entity.Property(e => e.List3).HasDefaultValue("");
            entity.Property(e => e.List41).HasDefaultValue("");
            entity.Property(e => e.List42).HasDefaultValue("");
            entity.Property(e => e.List5).HasDefaultValue("");
            entity.Property(e => e.List6).HasDefaultValue("");
            entity.Property(e => e.List7).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.StartJob).HasDefaultValue(false);
            entity.Property(e => e.Username).HasDefaultValue("");
        });

        modelBuilder.Entity<OrdDorder>(entity =>
        {
            entity.HasKey(e => new { e.OrderNo, e.SetNo, e.Barcode, e.Num })
                .IsClustered(false)
                .HasFillFactor(90);

            entity.ToTable("OrdDOrder", "dbo", tb => tb.HasTrigger("OrdDorder_Trigger"));

            entity.HasIndex(e => new { e.OrderNo, e.Barcode, e.SetNo, e.Num }, "IX_OrdDOrder")
                .IsUnique()
                .IsClustered()
                .HasFillFactor(90);

            entity.Property(e => e.OrderNo).HasDefaultValue("");
            entity.Property(e => e.SetNo).HasDefaultValue("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.Num).ValueGeneratedOnAdd();
            entity.Property(e => e.BarP).HasDefaultValue(0m);
            entity.Property(e => e.Barcodemat).HasDefaultValue("");
            entity.Property(e => e.CardP).HasDefaultValue(0m);
            entity.Property(e => e.ChkSize).HasDefaultValue(false);
            entity.Property(e => e.Cnarticle).HasDefaultValue("");
            entity.Property(e => e.Cnempcode).HasDefaultValue("");
            entity.Property(e => e.Cs1).HasDefaultValue("");
            entity.Property(e => e.Cs10).HasDefaultValue("");
            entity.Property(e => e.Cs11).HasDefaultValue("");
            entity.Property(e => e.Cs12).HasDefaultValue("");
            entity.Property(e => e.Cs2).HasDefaultValue("");
            entity.Property(e => e.Cs3).HasDefaultValue("");
            entity.Property(e => e.Cs4).HasDefaultValue("");
            entity.Property(e => e.Cs5).HasDefaultValue("");
            entity.Property(e => e.Cs6)
                .HasDefaultValue("")
                .HasComment("");
            entity.Property(e => e.Cs7).HasDefaultValue("");
            entity.Property(e => e.Cs8).HasDefaultValue("");
            entity.Property(e => e.Cs9).HasDefaultValue("");
            entity.Property(e => e.CustPcode).HasDefaultValue("");
            entity.Property(e => e.EdesFn).HasDefaultValue("");
            entity.Property(e => e.EditEdesFn).HasDefaultValue("");
            entity.Property(e => e.EditEpoxyColor).HasDefaultValue("");
            entity.Property(e => e.EditListGem).HasDefaultValue("");
            entity.Property(e => e.LotNo).HasDefaultValue("");
            entity.Property(e => e.Price).HasDefaultValue(0.0);
            entity.Property(e => e.PriceWg).HasDefaultValue(0.0);
            entity.Property(e => e.Q1).HasDefaultValue(0m);
            entity.Property(e => e.Q10).HasDefaultValue(0);
            entity.Property(e => e.Q11).HasDefaultValue(0);
            entity.Property(e => e.Q12).HasDefaultValue(0);
            entity.Property(e => e.Q2).HasDefaultValue(0);
            entity.Property(e => e.Q3).HasDefaultValue(0);
            entity.Property(e => e.Q4).HasDefaultValue(0);
            entity.Property(e => e.Q5).HasDefaultValue(0);
            entity.Property(e => e.Q6).HasDefaultValue(0);
            entity.Property(e => e.Q7).HasDefaultValue(0);
            entity.Property(e => e.Q8).HasDefaultValue(0);
            entity.Property(e => e.Q9).HasDefaultValue(0);
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.RemarkOc).HasDefaultValue("");
            entity.Property(e => e.S1).HasDefaultValue("");
            entity.Property(e => e.S10).HasDefaultValue("");
            entity.Property(e => e.S11).HasDefaultValue("");
            entity.Property(e => e.S12).HasDefaultValue("");
            entity.Property(e => e.S2).HasDefaultValue("");
            entity.Property(e => e.S3).HasDefaultValue("");
            entity.Property(e => e.S4).HasDefaultValue("");
            entity.Property(e => e.S5).HasDefaultValue("");
            entity.Property(e => e.S6).HasDefaultValue("");
            entity.Property(e => e.S7).HasDefaultValue("");
            entity.Property(e => e.S8).HasDefaultValue("");
            entity.Property(e => e.S9).HasDefaultValue("");
            entity.Property(e => e.SaleRem).HasDefaultValue("");
            entity.Property(e => e.SaleType).HasComment("1=หน่วย,2=น้ำหนัก");
            entity.Property(e => e.SizeZone).HasDefaultValue("");
            entity.Property(e => e.StamP).HasDefaultValue(0m);
            entity.Property(e => e.TrayNo).HasDefaultValue("");
            entity.Property(e => e.TtPrice).HasDefaultValue(0m);
            entity.Property(e => e.TtQty).HasDefaultValue(0m);
            entity.Property(e => e.TtWg).HasDefaultValue(0m);
            entity.Property(e => e.Unit)
                .HasDefaultValue("")
                .IsFixedLength();
            entity.Property(e => e.Us).HasComment("1=US,0=Thai");
            entity.Property(e => e.Wg).HasDefaultValue(0m);
            entity.Property(e => e.Zarticle).HasDefaultValue("");
            entity.Property(e => e.Zbarcode).HasDefaultValue("");
        });

        modelBuilder.Entity<OrdHorder>(entity =>
        {
            entity.HasKey(e => e.OrderNo).IsClustered(false);

            entity.HasIndex(e => e.OrderNo, "OrdHOrder11")
                .IsClustered()
                .HasFillFactor(90);

            entity.Property(e => e.Bysale).HasDefaultValue(9);
            entity.Property(e => e.Company).IsFixedLength();
            entity.Property(e => e.Currency).HasDefaultValue("");
            entity.Property(e => e.DecimalPrice)
                .HasDefaultValue(2)
                .HasComment("ทศนิยม");
            entity.Property(e => e.Factory).HasComment("ยืนยันผลิต");
            entity.Property(e => e.Opened).HasDefaultValue(false);
            entity.Property(e => e.OrdType).HasDefaultValue("");
            entity.Property(e => e.RangeSilver).HasDefaultValue("");
            entity.Property(e => e.RevNo).HasDefaultValue("");
            entity.Property(e => e.SaleRef).HasDefaultValue("");
            entity.Property(e => e.Sled1).HasComment("วันที่เลื่อนนัดจ่ายงานครั้งที่1");
            entity.Property(e => e.Sled2).HasComment("วันที่เลื่อนนัดจ่ายงานครั้งที่2");
            entity.Property(e => e.ValidDate1).HasDefaultValue("");
            entity.Property(e => e.WebIdorder).HasDefaultValue("");
        });

        modelBuilder.Entity<OrdLotno>(entity =>
        {
            entity.HasKey(e => new { e.OrderNo, e.LotNo, e.Barcode, e.ListNo, e.GroupNo, e.GroupSetNo })
                .IsClustered(false)
                .HasFillFactor(90);

            entity.ToTable("OrdLotno", "dbo", tb => tb.HasTrigger("OrdLotno_Trigger"));

            entity.HasIndex(e => new { e.OrderNo, e.LotNo, e.Barcode, e.GroupSetNo, e.ListNo, e.GroupNo }, "IX_OrdLotno")
                .IsUnique()
                .IsClustered()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.OrderNo, e.SetNo1, e.SetNo2, e.SetNo3, e.SetNo4, e.SetNo5, e.SetNo6, e.SetNo7, e.SetNo8, e.SetNo9, e.SetNo10, e.Barcode, e.GroupSetNo, e.LotNo }, "IX_OrdLotno_1")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.OrderNo, e.LotNo, e.GroupSetNo }, "IX_OrdLotno_2").HasFillFactor(90);

            entity.HasIndex(e => e.LotNo, "OrdLotno31").HasFillFactor(90);

            entity.Property(e => e.ListNo).HasDefaultValue("");
            entity.Property(e => e.GroupNo).HasDefaultValue("");
            entity.Property(e => e.GroupSetNo).HasDefaultValue("");
            entity.Property(e => e.Barcodemat).HasDefaultValue("");
            entity.Property(e => e.ChkSize).HasDefaultValue(false);
            entity.Property(e => e.Cs1).HasDefaultValue("");
            entity.Property(e => e.Cs10).HasDefaultValue("");
            entity.Property(e => e.Cs11).HasDefaultValue("");
            entity.Property(e => e.Cs12).HasDefaultValue("");
            entity.Property(e => e.Cs2).HasDefaultValue("");
            entity.Property(e => e.Cs3).HasDefaultValue("");
            entity.Property(e => e.Cs4).HasDefaultValue("");
            entity.Property(e => e.Cs5).HasDefaultValue("");
            entity.Property(e => e.Cs6).HasDefaultValue("");
            entity.Property(e => e.Cs7).HasDefaultValue("");
            entity.Property(e => e.Cs8).HasDefaultValue("");
            entity.Property(e => e.Cs9).HasDefaultValue("");
            entity.Property(e => e.CustPcode).HasDefaultValue("");
            entity.Property(e => e.EdesFn).HasDefaultValue("");
            entity.Property(e => e.LotnoLink).HasDefaultValue("");
            entity.Property(e => e.Price).HasDefaultValue(0.0);
            entity.Property(e => e.PriceWg).HasDefaultValue(0.0);
            entity.Property(e => e.Q1).HasDefaultValue(0m);
            entity.Property(e => e.Q10).HasDefaultValue(0);
            entity.Property(e => e.Q11).HasDefaultValue(0);
            entity.Property(e => e.Q12).HasDefaultValue(0);
            entity.Property(e => e.Q2).HasDefaultValue(0);
            entity.Property(e => e.Q3).HasDefaultValue(0);
            entity.Property(e => e.Q4).HasDefaultValue(0);
            entity.Property(e => e.Q5).HasDefaultValue(0);
            entity.Property(e => e.Q6).HasDefaultValue(0);
            entity.Property(e => e.Q7).HasDefaultValue(0);
            entity.Property(e => e.Q8).HasDefaultValue(0);
            entity.Property(e => e.Q9).HasDefaultValue(0);
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.S1).HasDefaultValue("");
            entity.Property(e => e.S10).HasDefaultValue("");
            entity.Property(e => e.S11).HasDefaultValue("");
            entity.Property(e => e.S12).HasDefaultValue("");
            entity.Property(e => e.S2).HasDefaultValue("");
            entity.Property(e => e.S3).HasDefaultValue("");
            entity.Property(e => e.S4).HasDefaultValue("");
            entity.Property(e => e.S5).HasDefaultValue("");
            entity.Property(e => e.S6).HasDefaultValue("");
            entity.Property(e => e.S7).HasDefaultValue("");
            entity.Property(e => e.S8).HasDefaultValue("");
            entity.Property(e => e.S9).HasDefaultValue("");
            entity.Property(e => e.SaleRem).HasDefaultValue("");
            entity.Property(e => e.SaleType).HasDefaultValue(0);
            entity.Property(e => e.SetNo1).HasDefaultValue("");
            entity.Property(e => e.SetNo10).HasDefaultValue("");
            entity.Property(e => e.SetNo2)
                .HasDefaultValue("")
                .IsFixedLength();
            entity.Property(e => e.SetNo3).HasDefaultValue("");
            entity.Property(e => e.SetNo4).HasDefaultValue("");
            entity.Property(e => e.SetNo5).HasDefaultValue("");
            entity.Property(e => e.SetNo6).HasDefaultValue("");
            entity.Property(e => e.SetNo7).HasDefaultValue("");
            entity.Property(e => e.SetNo8).HasDefaultValue("");
            entity.Property(e => e.SetNo9).HasDefaultValue("");
            entity.Property(e => e.SizeZone).HasDefaultValue("");
            entity.Property(e => e.TrayNo).HasDefaultValue("");
            entity.Property(e => e.TtPrice).HasDefaultValue(0m);
            entity.Property(e => e.TtQty).HasDefaultValue(0m);
            entity.Property(e => e.TtWg).HasDefaultValue(0m);
            entity.Property(e => e.TypeLot).HasDefaultValue("0");
            entity.Property(e => e.Unit)
                .HasDefaultValue("")
                .IsFixedLength();
            entity.Property(e => e.Us).HasDefaultValue(false);
            entity.Property(e => e.Wg).HasDefaultValue(0m);
        });

        modelBuilder.Entity<OrdOrder>(entity =>
        {
            entity.Property(e => e.Ordno)
                .HasDefaultValue("")
                .HasComment("เลขที่ Order");
            entity.Property(e => e.Type)
                .HasDefaultValue("2")
                .HasComment("ประเภทงาน ='0' เงิน,='1' พลอย ,='2' ทั้งหมด,เงินใหม่='3',พลอยใหม่='4',Set='5'");
            entity.Property(e => e.Custcode)
                .HasDefaultValue("")
                .HasComment("รหัสลูกค้า");
            entity.Property(e => e.Jobuser).HasDefaultValue("");
            entity.Property(e => e.MakeOrder).HasComment("=0 งานOrder =1 ตั๋วมือ");
            entity.Property(e => e.Num).ValueGeneratedOnAdd();
            entity.Property(e => e.ProductDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Qorder).HasComment("รายการตั๋ว Order ที่ลงจำนวนเอง =1 ,0 ดึงรายการจาก Packing");
        });

        modelBuilder.Entity<Spdreceive>(entity =>
        {
            entity.HasKey(e => new { e.ReceiveNo, e.Id }).HasFillFactor(90);

            entity.HasIndex(e => new { e.ReceiveNo, e.Setno, e.Trayno, e.Article, e.Barcode, e.Lotno, e.Boxno, e.Id }, "IX_SPDReceive")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.ReceiveNo).HasDefaultValue("");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.BarcodeSam).HasDefaultValue("");
            entity.Property(e => e.Boxno).HasDefaultValue("");
            entity.Property(e => e.CusCode).HasDefaultValue("");
            entity.Property(e => e.Lotno).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.RequestNo).HasDefaultValue("");
            entity.Property(e => e.S1).HasDefaultValue("");
            entity.Property(e => e.S10).HasDefaultValue("");
            entity.Property(e => e.S11).HasDefaultValue("");
            entity.Property(e => e.S12).HasDefaultValue("");
            entity.Property(e => e.S2).HasDefaultValue("");
            entity.Property(e => e.S3).HasDefaultValue("");
            entity.Property(e => e.S4).HasDefaultValue("");
            entity.Property(e => e.S5).HasDefaultValue("");
            entity.Property(e => e.S6).HasDefaultValue("");
            entity.Property(e => e.S7).HasDefaultValue("");
            entity.Property(e => e.S8).HasDefaultValue("");
            entity.Property(e => e.S9).HasDefaultValue("");
            entity.Property(e => e.Setno).HasDefaultValue("");
            entity.Property(e => e.Setno1).HasDefaultValue("");
            entity.Property(e => e.Trayno).HasDefaultValue("");
            entity.Property(e => e.Upday).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Username).HasDefaultValue("");

            entity.HasOne(d => d.ReceiveNoNavigation).WithMany(p => p.Spdreceive)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SPDReceive_SPHReceive");
        });

        modelBuilder.Entity<Sphreceive>(entity =>
        {
            entity.Property(e => e.ReceiveNo).HasDefaultValue("");
            entity.Property(e => e.Cancel).HasComment("ยกเลิก=1");
            entity.Property(e => e.Department)
                .HasDefaultValue("")
                .HasComment("แผนกที่โอนข้อมูลเข้า");
            entity.Property(e => e.Docno)
                .HasDefaultValue("")
                .HasComment("เลขที่เอกสารที่โอนเข้า");
            entity.Property(e => e.Insure).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.Upday).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Username).HasDefaultValue("");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
