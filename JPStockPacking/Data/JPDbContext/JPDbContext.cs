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

    public virtual DbSet<CfnCode> CfnCode { get; set; }

    public virtual DbSet<CpriceSale> CpriceSale { get; set; }

    public virtual DbSet<Cprofile> Cprofile { get; set; }

    public virtual DbSet<CusProfile> CusProfile { get; set; }

    public virtual DbSet<CusZoneType> CusZoneType { get; set; }

    public virtual DbSet<ExDminv> ExDminv { get; set; }

    public virtual DbSet<ExHminv> ExHminv { get; set; }

    public virtual DbSet<JobBill> JobBill { get; set; }

    public virtual DbSet<JobBillSendStock> JobBillSendStock { get; set; }

    public virtual DbSet<JobBillsize> JobBillsize { get; set; }

    public virtual DbSet<JobCost> JobCost { get; set; }

    public virtual DbSet<JobDetail> JobDetail { get; set; }

    public virtual DbSet<JobHead> JobHead { get; set; }

    public virtual DbSet<JobOrder> JobOrder { get; set; }

    public virtual DbSet<OrdDorder> OrdDorder { get; set; }

    public virtual DbSet<OrdHorder> OrdHorder { get; set; }

    public virtual DbSet<OrdLotno> OrdLotno { get; set; }

    public virtual DbSet<OrdOrder> OrdOrder { get; set; }

    public virtual DbSet<Sj1dreceive> Sj1dreceive { get; set; }

    public virtual DbSet<Sj1hreceive> Sj1hreceive { get; set; }

    public virtual DbSet<Sj2dreceive> Sj2dreceive { get; set; }

    public virtual DbSet<Sj2hreceive> Sj2hreceive { get; set; }

    public virtual DbSet<Spdreceive> Spdreceive { get; set; }

    public virtual DbSet<Sphreceive> Sphreceive { get; set; }

    public virtual DbSet<TempProfile> TempProfile { get; set; }

    public virtual DbSet<Userid> Userid { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("admin");

        modelBuilder.Entity<CfnCode>(entity =>
        {
            entity.HasKey(e => e.FnCode).HasFillFactor(90);

            entity.ToTable("CFnCode", "dbo", tb =>
                {
                    tb.HasTrigger("trg_CFnCode_Insert");
                    tb.HasTrigger("trg_CFnCode_Update");
                });

            entity.HasIndex(e => e.FnCode, "IX_CFnCode")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Case1).HasComment("<10");
            entity.Property(e => e.Case2).HasComment(">=10 and <=20");
            entity.Property(e => e.Case3).HasComment(">20");
        });

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

        modelBuilder.Entity<CusProfile>(entity =>
        {
            entity.HasKey(e => e.Code).HasFillFactor(90);

            entity.HasIndex(e => e.CusCode, "IX_CusProfile")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Agemail1).HasDefaultValue("");
            entity.Property(e => e.Agemail2).HasDefaultValue("");
            entity.Property(e => e.Agemail3).HasDefaultValue("");
            entity.Property(e => e.Agemail4).HasDefaultValue("");
            entity.Property(e => e.Agemail5).HasDefaultValue("");
            entity.Property(e => e.Agmobile1).HasDefaultValue("");
            entity.Property(e => e.Agmobile2).HasDefaultValue("");
            entity.Property(e => e.Agmobile3).HasDefaultValue("");
            entity.Property(e => e.Avail).HasDefaultValue("Y");
            entity.Property(e => e.Bankfee).HasDefaultValue("");
            entity.Property(e => e.CargoDescription).HasDefaultValue("");
            entity.Property(e => e.CargoStatus).HasDefaultValue(false);
            entity.Property(e => e.Cdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Cemail1).HasDefaultValue("");
            entity.Property(e => e.Cemail2).HasDefaultValue("");
            entity.Property(e => e.Cemail3).HasDefaultValue("");
            entity.Property(e => e.Cemail4).HasDefaultValue("");
            entity.Property(e => e.Cemail5).HasDefaultValue("");
            entity.Property(e => e.CommissionFee).HasDefaultValue(0.00m);
            entity.Property(e => e.ContactName1).HasDefaultValue("");
            entity.Property(e => e.ContactName2).HasDefaultValue("");
            entity.Property(e => e.ContactName3).HasDefaultValue("");
            entity.Property(e => e.ContactName4).HasDefaultValue("");
            entity.Property(e => e.ContactName5).HasDefaultValue("");
            entity.Property(e => e.CourierName).HasDefaultValue("");
            entity.Property(e => e.CreditTerm).HasDefaultValue(false);
            entity.Property(e => e.CreditValue).HasDefaultValue("");
            entity.Property(e => e.CurierStatus).HasDefaultValue(false);
            entity.Property(e => e.CusAccount).HasDefaultValue("");
            entity.Property(e => e.CusZoneId).HasDefaultValue(0);
            entity.Property(e => e.DepositPay).HasDefaultValue(false);
            entity.Property(e => e.DepositValue).HasDefaultValueSql("(5)");
            entity.Property(e => e.LastUpdate).HasDefaultValueSql("('')");
            entity.Property(e => e.MDateF).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MDateL).HasDefaultValueSql("('')");
            entity.Property(e => e.PathPicStamping).HasDefaultValue("");
            entity.Property(e => e.Position1).HasDefaultValue("");
            entity.Property(e => e.Position2).HasDefaultValue("");
            entity.Property(e => e.Position3).HasDefaultValue("");
            entity.Property(e => e.Position4).HasDefaultValue("");
            entity.Property(e => e.Position5).HasDefaultValue("");
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.Remark2).HasDefaultValue("");
            entity.Property(e => e.Remark3).HasDefaultValue("");
            entity.Property(e => e.Remark4).HasDefaultValue("");
            entity.Property(e => e.ShipAddress).HasDefaultValue("");
            entity.Property(e => e.ShipCity).HasDefaultValue("");
            entity.Property(e => e.ShipCompany).HasDefaultValue("");
            entity.Property(e => e.ShipContact).HasDefaultValue("");
            entity.Property(e => e.ShipCountry).HasDefaultValue("");
            entity.Property(e => e.ShipEmail).HasDefaultValue("");
            entity.Property(e => e.ShipFax).HasDefaultValue("");
            entity.Property(e => e.ShipTel).HasDefaultValue("");
            entity.Property(e => e.ShipZipCode).HasDefaultValue("");
            entity.Property(e => e.ShipmentType).HasDefaultValue(0);
            entity.Property(e => e.SourceFrom).HasDefaultValue("");
            entity.Property(e => e.TradeAddr).HasDefaultValue("");
            entity.Property(e => e.TradeCity).HasDefaultValue("");
            entity.Property(e => e.TradeCom).HasDefaultValue("");
            entity.Property(e => e.TradeCountry).HasDefaultValue("");
            entity.Property(e => e.TradeEmail).HasDefaultValue("");
            entity.Property(e => e.TradeFax).HasDefaultValue("");
            entity.Property(e => e.TradeName).HasDefaultValue("");
            entity.Property(e => e.TradeTel).HasDefaultValue("");
            entity.Property(e => e.TradeType).HasDefaultValue(0);
            entity.Property(e => e.TradeZipCode).HasDefaultValue("");
            entity.Property(e => e.UserDel).HasDefaultValue("");
            entity.Property(e => e.UserNameF).HasDefaultValue("");
            entity.Property(e => e.UserNameL).HasDefaultValue("");
            entity.Property(e => e.WebIdcust).HasDefaultValue("");
        });

        modelBuilder.Entity<CusZoneType>(entity =>
        {
            entity.HasKey(e => e.CusZoneId).HasFillFactor(90);

            entity.Property(e => e.CusZoneId).ValueGeneratedNever();
            entity.Property(e => e.CusZoneName).HasDefaultValue("");
            entity.Property(e => e.RefSale).HasDefaultValue("");
        });

        modelBuilder.Entity<ExDminv>(entity =>
        {
            entity.HasKey(e => new { e.Code, e.MinvNo, e.LotNo, e.Article, e.SetNo, e.OrderNo, e.Efn, e.ListGem, e.ListGem1, e.Barcode }).HasFillFactor(90);

            entity.HasIndex(e => new { e.MinvNo, e.LotNo, e.Barcode, e.SetNo, e.Article, e.Code }, "ExDMInv1")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.MinvNo, e.Code }, "IX_ExDMInv")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.MinvNo, e.LotNo, e.SetNo, e.OrderNo }, "IX_ExDMInv_1").HasFillFactor(90);

            entity.HasIndex(e => new { e.MinvNo, e.SetNo, e.Barcode, e.OrderNo }, "IX_ExDMInv_2").HasFillFactor(90);

            entity.HasIndex(e => e.Code, "IX_ExDMInv_3")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Code).ValueGeneratedOnAdd();
            entity.Property(e => e.LotNo).HasDefaultValue("");
            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.SetNo).HasDefaultValue("");
            entity.Property(e => e.OrderNo).HasDefaultValue("");
            entity.Property(e => e.Efn).HasDefaultValue("");
            entity.Property(e => e.ListGem).HasDefaultValue("");
            entity.Property(e => e.ListGem1)
                .HasDefaultValue("")
                .HasComment("รายการพลอยที่แก้ไขเอง");
            entity.Property(e => e.Barcode)
                .HasDefaultValue("")
                .HasComment("Barcode");
            entity.Property(e => e.Barcode1).HasDefaultValue("");
            entity.Property(e => e.Barcode10).HasDefaultValue("");
            entity.Property(e => e.Barcode11).HasDefaultValue("");
            entity.Property(e => e.Barcode12).HasDefaultValue("");
            entity.Property(e => e.Barcode2).HasDefaultValue("");
            entity.Property(e => e.Barcode3).HasDefaultValue("");
            entity.Property(e => e.Barcode4).HasDefaultValue("");
            entity.Property(e => e.Barcode5).HasDefaultValue("");
            entity.Property(e => e.Barcode6).HasDefaultValue("");
            entity.Property(e => e.Barcode7).HasDefaultValue("");
            entity.Property(e => e.Barcode8).HasDefaultValue("");
            entity.Property(e => e.Barcode9).HasDefaultValue("");
            entity.Property(e => e.EUnit).HasDefaultValue("");
            entity.Property(e => e.Edes).HasDefaultValue("");
            entity.Property(e => e.EdesFn1).HasDefaultValue("");
            entity.Property(e => e.Epoxycolor)
                .HasDefaultValue("")
                .HasComment("ลงยาสี");
            entity.Property(e => e.GroupCust).HasDefaultValue("");
            entity.Property(e => e.Hscode).HasDefaultValue("");
            entity.Property(e => e.InvNo).HasDefaultValue("");
            entity.Property(e => e.MakeUnit).HasDefaultValue(" ");
            entity.Property(e => e.NotHistory).HasComment("ไมีมีประวัติ");
            entity.Property(e => e.OrderCust).HasDefaultValue("");
            entity.Property(e => e.Q1).HasDefaultValue(0m);
            entity.Property(e => e.Q10).HasDefaultValue(0m);
            entity.Property(e => e.Q11).HasDefaultValue(0m);
            entity.Property(e => e.Q12).HasDefaultValue(0m);
            entity.Property(e => e.Q2).HasDefaultValue(0m);
            entity.Property(e => e.Q3).HasDefaultValue(0m);
            entity.Property(e => e.Q4).HasDefaultValue(0m);
            entity.Property(e => e.Q5).HasDefaultValue(0m);
            entity.Property(e => e.Q6).HasDefaultValue(0m);
            entity.Property(e => e.Q7).HasDefaultValue(0m);
            entity.Property(e => e.Q8).HasDefaultValue(0m);
            entity.Property(e => e.Q9).HasDefaultValue(0m);
            entity.Property(e => e.SizeZone).HasDefaultValue("");
            entity.Property(e => e.TtPrice).HasDefaultValue(0.0000m);
            entity.Property(e => e.TtQty).HasDefaultValue(0m);
            entity.Property(e => e.Ttwg).HasDefaultValue(0m);
            entity.Property(e => e.WgPerPc).HasDefaultValue(0m);
        });

        modelBuilder.Entity<ExHminv>(entity =>
        {
            entity.HasKey(e => new { e.MinvNo, e.InvNo }).HasFillFactor(90);

            entity.ToTable("ExHMInv", "dbo", tb => tb.HasTrigger("tgInvDate"));

            entity.Property(e => e.AddressSend).HasDefaultValue("");
            entity.Property(e => e.Adjust).HasDefaultValue(1m);
            entity.Property(e => e.Agreement).HasDefaultValue("");
            entity.Property(e => e.ApplicantAdd1).HasDefaultValue("");
            entity.Property(e => e.ApplicantName).HasDefaultValue("");
            entity.Property(e => e.AwbNo).HasDefaultValue("");
            entity.Property(e => e.BankItem).HasDefaultValue(1);
            entity.Property(e => e.Condition).HasDefaultValue("COUNTRY  OF  ORIGIN  THAILAND.");
            entity.Property(e => e.ConsigneeAdd1).HasDefaultValue("");
            entity.Property(e => e.ConsigneeAdd2).HasDefaultValue("");
            entity.Property(e => e.ConsigneeAdd3).HasDefaultValue("");
            entity.Property(e => e.ConsigneeAdd4).HasDefaultValue("");
            entity.Property(e => e.ConsigneeName).HasDefaultValue("");
            entity.Property(e => e.Currency).HasDefaultValue("");
            entity.Property(e => e.CustomerName).HasDefaultValue("");
            entity.Property(e => e.DestinationCity).HasDefaultValue("");
            entity.Property(e => e.Docno)
                .HasDefaultValue("")
                .HasComment("เลขที่ใบเบิกตัด Stock");
            entity.Property(e => e.Freight).HasDefaultValue("");
            entity.Property(e => e.GrossWg).HasDefaultValue(0m);
            entity.Property(e => e.Header).HasDefaultValue("");
            entity.Property(e => e.Insurance).HasDefaultValue("");
            entity.Property(e => e.InvRemark1).HasDefaultValue("");
            entity.Property(e => e.InvRemark2).HasDefaultValue("");
            entity.Property(e => e.InvRepno).HasDefaultValue("");
            entity.Property(e => e.Mdecimal).HasDefaultValue(4);
            entity.Property(e => e.NetWg).HasDefaultValue(0m);
            entity.Property(e => e.NotifyAdd1).HasDefaultValue("");
            entity.Property(e => e.NotifyAdd2).HasDefaultValue("");
            entity.Property(e => e.NotifyAdd3).HasDefaultValue("");
            entity.Property(e => e.NotifyAdd4).HasDefaultValue("");
            entity.Property(e => e.NotifyName).HasDefaultValue("");
            entity.Property(e => e.Payment).HasDefaultValue("");
            entity.Property(e => e.Pfob).HasDefaultValue(0.0000m);
            entity.Property(e => e.Pfob1).HasComment("เก็บ Fob ก่อน Adjust ราคารวม");
            entity.Property(e => e.Pfreight).HasDefaultValue(0m);
            entity.Property(e => e.PgrandTt).HasDefaultValue(0.0000m);
            entity.Property(e => e.Pinsurance).HasDefaultValue(0m);
            entity.Property(e => e.Pless1).HasDefaultValue(0m);
            entity.Property(e => e.Pless2).HasDefaultValue(0m);
            entity.Property(e => e.Pless3).HasDefaultValue(0m);
            entity.Property(e => e.Pless4).HasDefaultValue(0m);
            entity.Property(e => e.Pless5).HasDefaultValue(0m);
            entity.Property(e => e.Pplus1).HasDefaultValue(0m);
            entity.Property(e => e.Pplus2).HasDefaultValue(0m);
            entity.Property(e => e.Pplus3).HasDefaultValue(0m);
            entity.Property(e => e.Pplus4).HasDefaultValue(0m);
            entity.Property(e => e.Pplus5).HasDefaultValue(0m);
            entity.Property(e => e.Ppostage).HasDefaultValue(0m);
            entity.Property(e => e.ProNo).HasDefaultValue("");
            entity.Property(e => e.Puntil).HasDefaultValue(0m);
            entity.Property(e => e.RemarkDimension).HasDefaultValue("");
            entity.Property(e => e.ToOrderOf).HasDefaultValue("");
            entity.Property(e => e.Tpless).HasDefaultValue(0m);
            entity.Property(e => e.Tpplus).HasDefaultValue(0m);
            entity.Property(e => e.TtBox).HasDefaultValue(0m);
            entity.Property(e => e.TtPfob).HasDefaultValue(0.0000m);
            entity.Property(e => e.TtPless).HasDefaultValue(0m);
            entity.Property(e => e.TtPplus).HasDefaultValue(0m);
            entity.Property(e => e.Ttaccount1).HasComment("บริหาร");
            entity.Property(e => e.Ttaccount2).HasComment("บัญชี");
            entity.Property(e => e.UpStock).HasComment("ปรับปรุง Stock=1");
            entity.Property(e => e.Vw).HasComment("Value Weight");
            entity.Property(e => e.WareHouseAdd1).HasDefaultValue("");
            entity.Property(e => e.WareHouseAdd2).HasDefaultValue("");
            entity.Property(e => e.WareHouseAdd3).HasDefaultValue("");
            entity.Property(e => e.WareHouseAdd4).HasDefaultValue("");
            entity.Property(e => e.WareHouseName).HasDefaultValue("");

            entity.HasOne(d => d.CusCodeNavigation).WithMany(p => p.ExHminv)
                .HasPrincipalKey(p => p.CusCode)
                .HasForeignKey(d => d.CusCode)
                .HasConstraintName("FK_ExHMInv_CusProfile");
        });

        modelBuilder.Entity<JobBill>(entity =>
        {
            entity.HasKey(e => e.Billnumber).HasFillFactor(90);

            entity.HasIndex(e => e.Billnumber, "IX_JobBill")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.JobBarcode, e.Num }, "IX_JobBill_1")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.Billnumber, e.JobBarcode, e.Num }, "IX_JobBill_2")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => e.JobBarcode, "IX_JobBill_3").HasFillFactor(90);

            entity.HasIndex(e => new { e.EmpCode, e.JobBarcode, e.OkTtl, e.OkWg, e.EpTtl, e.EpWg, e.RtTtl, e.RtWg, e.DmTtl, e.DmWg }, "JobBill9").HasFillFactor(90);

            entity.Property(e => e.ArtCode).HasDefaultValue("");
            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.CloseLast).HasComment("จบงานช่าง ส่งงานครั้งสุดท้าย");
            entity.Property(e => e.DocNo).HasDefaultValue("");
            entity.Property(e => e.EpQ3).HasComment("");
            entity.Property(e => e.FnCode).HasDefaultValue("");
            entity.Property(e => e.JobBarcode).HasDefaultValue("");
            entity.Property(e => e.ListNo).HasDefaultValue("");
            entity.Property(e => e.Lotno).HasDefaultValue("");
            entity.Property(e => e.MDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Num).HasComment("เลขลำดับที่ ของ JobBarcode เช่น '01','02'");
            entity.Property(e => e.PackDoc).HasDefaultValue("");
            entity.Property(e => e.SendMeltDoc).HasDefaultValue("");
            entity.Property(e => e.SendStockDoc).HasDefaultValue("");
            entity.Property(e => e.Silver).HasComment("เศษเนื้อเงิน");
            entity.Property(e => e.UserName).HasDefaultValue("");

            entity.HasOne(d => d.JobDetail).WithMany(p => p.JobBill)
                .HasPrincipalKey(p => new { p.JobBarcode, p.Barcode })
                .HasForeignKey(d => new { d.JobBarcode, d.Barcode })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobBill_JobDetail");
        });

        modelBuilder.Entity<JobBillSendStock>(entity =>
        {
            entity.Property(e => e.ItemSend).HasDefaultValue("");
            entity.Property(e => e.Doc).HasDefaultValue("");
            entity.Property(e => e.MdateSend).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Numsend).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<JobBillsize>(entity =>
        {
            entity.Property(e => e.MDate).HasDefaultValueSql("(getdate())");
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
            entity.Property(e => e.UserName).HasDefaultValue("");
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

        modelBuilder.Entity<JobDetail>(entity =>
        {
            entity.HasKey(e => new { e.JobBarcode, e.DocNo, e.EmpCode })
                .IsClustered(false)
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.JobBarcode, e.Barcode }, "IX_JobDetail")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.DocNo, e.EmpCode }, "IX_JobDetail_1").HasFillFactor(90);

            entity.HasIndex(e => e.JobBarcode, "IX_JobDetail_2")
                .IsUnique()
                .IsClustered()
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.JobBarcode, e.Barcode, e.CustCode }, "IX_JobDetail_3")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.JobBarcode).HasDefaultValue("");
            entity.Property(e => e.AccPrice).HasComment("ราคาค่าแรงช่างคิดบัญชี");
            entity.Property(e => e.AdjustWg).HasDefaultValue(0m);
            entity.Property(e => e.ArtCode).HasDefaultValue("");
            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.BodyWg).HasDefaultValue(0m);
            entity.Property(e => e.BodyWg2).HasDefaultValue(0m);
            entity.Property(e => e.ChkGem).HasComment("เช็คว่ามีพลอยติดตัวเรือนไปหรือไม่");
            entity.Property(e => e.ChkMaterial).HasComment("เช็คค่าวัตถุดิบ(ปักก้าน)ให้ช่าง ");
            entity.Property(e => e.CustCode).HasDefaultValue("");
            entity.Property(e => e.DateClose).HasComment("วันที่ปิดรายการ");
            entity.Property(e => e.Description).HasDefaultValue("");
            entity.Property(e => e.Dmpercent).HasComment("ค่าซิเนื้อเงิน คิดเป็น %");
            entity.Property(e => e.FnCode).HasDefaultValue("");
            entity.Property(e => e.Grade).HasDefaultValue("");
            entity.Property(e => e.GroupNo).HasDefaultValue("");
            entity.Property(e => e.GroupSetNo).HasDefaultValue("");
            entity.Property(e => e.JobClose).HasComment("ปิดช่าง 1=ปิดช่าง");
            entity.Property(e => e.JobPriceEdit).HasComment("รายการที่แก้ไขค่าแรง =1 ");
            entity.Property(e => e.JobPriceOld).HasComment("ราคาค่าแรงก่อนแก้ไข");
            entity.Property(e => e.ListNo).HasDefaultValue("");
            entity.Property(e => e.LotNo).HasDefaultValue("");
            entity.Property(e => e.MarkJob).HasDefaultValue("");
            entity.Property(e => e.MatItem).HasDefaultValue("");
            entity.Property(e => e.OrderNo).HasDefaultValue("");
            entity.Property(e => e.Remark1).HasDefaultValue("");
            entity.Property(e => e.Remark2).HasDefaultValue("");
            entity.Property(e => e.TtlwgOld).HasDefaultValue(0.00m);
            entity.Property(e => e.Unit).HasDefaultValue("");
            entity.Property(e => e.UserClose)
                .HasDefaultValue("")
                .HasComment("ชื่อผู้ทำรายการปิดช่าง");
            entity.Property(e => e.UserName).HasDefaultValue("");

            entity.HasOne(d => d.CustCodeNavigation).WithMany(p => p.JobDetail)
                .HasPrincipalKey(p => p.CusCode)
                .HasForeignKey(d => d.CustCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobDetail_CusProfile");

            entity.HasOne(d => d.OrdLotno).WithMany(p => p.JobDetail)
                .HasPrincipalKey(p => new { p.OrderNo, p.LotNo, p.Barcode, p.GroupSetNo, p.ListNo, p.GroupNo })
                .HasForeignKey(d => new { d.OrderNo, d.LotNo, d.Barcode, d.GroupSetNo, d.ListNo, d.GroupNo })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobDetail_OrdLotno");
        });

        modelBuilder.Entity<JobHead>(entity =>
        {
            entity.HasKey(e => new { e.DocNo, e.EmpCode }).HasFillFactor(90);

            entity.HasIndex(e => new { e.DocNo, e.EmpCode }, "IX_JobHead")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.ChkGem).HasComment("เช็คราคาค่าแรงรวมค่าพลอย");
            entity.Property(e => e.ChkSilver).HasComment("เช็คราคาค่าแรงรวมค่าเนื้อเงิน");
            entity.Property(e => e.DueDate).HasDefaultValueSql("(getdate() + 3)");
            entity.Property(e => e.JobDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PrintBy).HasDefaultValueSql("(0)");
            entity.Property(e => e.Runid).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeOther).HasDefaultValue("");
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

            entity.HasOne(d => d.CustcodeNavigation).WithMany(p => p.OrdOrder)
                .HasPrincipalKey(p => p.CusCode)
                .HasForeignKey(d => d.Custcode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdOrder_CusProfile");
        });

        modelBuilder.Entity<Sj1dreceive>(entity =>
        {
            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.BarcodeSam).HasDefaultValue("");
            entity.Property(e => e.Boxno).HasDefaultValue("");
            entity.Property(e => e.CusCode).HasDefaultValue("");
            entity.Property(e => e.Lotno).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReceiveNo).HasDefaultValue("");
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
        });

        modelBuilder.Entity<Sj1hreceive>(entity =>
        {
            entity.Property(e => e.ReceiveNo).HasDefaultValue("");
            entity.Property(e => e.Department).HasDefaultValue("");
            entity.Property(e => e.Docno).HasDefaultValue("");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Insure).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.Upday).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Username).HasDefaultValue("");
        });

        modelBuilder.Entity<Sj2dreceive>(entity =>
        {
            entity.Property(e => e.Article).HasDefaultValue("");
            entity.Property(e => e.Barcode).HasDefaultValue("");
            entity.Property(e => e.BarcodeSam).HasDefaultValue("");
            entity.Property(e => e.Boxno).HasDefaultValue("");
            entity.Property(e => e.CusCode).HasDefaultValue("");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Lotno).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReceiveNo).HasDefaultValue("");
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
        });

        modelBuilder.Entity<Sj2hreceive>(entity =>
        {
            entity.Property(e => e.Department).HasDefaultValue("");
            entity.Property(e => e.Docno).HasDefaultValue("");
            entity.Property(e => e.Insure).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReceiveNo).HasDefaultValue("");
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.Upday).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Username).HasDefaultValue("");
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

        modelBuilder.Entity<TempProfile>(entity =>
        {
            entity.HasKey(e => e.EmpCode).HasFillFactor(90);

            entity.ToTable("TEmpProfile", "dbo", tb => tb.HasTrigger("TEmpProfile_Trigger"));

            entity.Property(e => e.EmpCode).ValueGeneratedNever();
            entity.Property(e => e.Btype).HasDefaultValue("000000000");
            entity.Property(e => e.DempType).HasDefaultValue("");
            entity.Property(e => e.Detail).HasDefaultValue("");
            entity.Property(e => e.EmpLink).ValueGeneratedOnAdd();
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Remark).HasDefaultValue("");
            entity.Property(e => e.RunDoc).HasDefaultValue(0);
            entity.Property(e => e.TitleName).HasDefaultValue("");
            entity.Property(e => e.Username).HasDefaultValue("");
        });

        modelBuilder.Entity<Userid>(entity =>
        {
            entity.HasKey(e => e.Num).IsClustered(false);

            entity.HasIndex(e => new { e.Userid1, e.Password, e.Useridgroup }, "IX_Userid")
                .IsUnique()
                .IsClustered()
                .HasFillFactor(90);

            entity.Property(e => e.Description).HasDefaultValue("");
            entity.Property(e => e.Mdate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Password).HasDefaultValue("");
            entity.Property(e => e.Useridgroup).HasDefaultValue("");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
