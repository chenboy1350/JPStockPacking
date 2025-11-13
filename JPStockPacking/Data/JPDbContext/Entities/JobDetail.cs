using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("JobBarcode", "DocNo", "EmpCode")]
[Table("JobDetail", Schema = "dbo")]
[Index("LotNo", Name = "IX_JobDetail_4")]
[Index("OrderNo", "LotNo", Name = "IX_JobDetail_5")]
[Index("JobClose", "ArtCode", "Barcode", Name = "IX_JobDetail_6")]
public partial class JobDetail
{
    [Key]
    [StringLength(12)]
    [Unicode(false)]
    public string JobBarcode { get; set; } = null!;

    public int JobNum { get; set; }

    [Key]
    [StringLength(12)]
    [Unicode(false)]
    public string DocNo { get; set; } = null!;

    [Key]
    public int EmpCode { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(2)]
    [Unicode(false)]
    public string Grade { get; set; } = null!;

    [StringLength(7)]
    [Unicode(false)]
    public string CustCode { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string ListNo { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [StringLength(14)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [Column("fnCode")]
    [StringLength(3)]
    [Unicode(false)]
    public string FnCode { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string ArtCode { get; set; } = null!;

    [StringLength(3)]
    [Unicode(false)]
    public string Unit { get; set; } = null!;

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal Qty12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal TtQty { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Ttlwg { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PriceJob { get; set; }

    [Column(TypeName = "text")]
    public string MarkJob { get; set; } = null!;

    [Column(TypeName = "text")]
    public string Description { get; set; } = null!;

    public bool ChkModel { get; set; }

    public int Model { get; set; }

    public bool ChkChunk { get; set; }

    public int Chunk { get; set; }

    [StringLength(230)]
    [Unicode(false)]
    public string MatItem { get; set; } = null!;

    [Column(TypeName = "decimal(18, 4)")]
    public decimal MatWg { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MatPrice { get; set; }

    public bool QtyType { get; set; }

    [Column("mDate", TypeName = "datetime")]
    public DateTime MDate { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// ปิดช่าง 1=ปิดช่าง
    /// </summary>
    public bool JobClose { get; set; }

    /// <summary>
    /// ชื่อผู้ทำรายการปิดช่าง
    /// </summary>
    [StringLength(20)]
    [Unicode(false)]
    public string UserClose { get; set; } = null!;

    /// <summary>
    /// วันที่ปิดรายการ
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime? DateClose { get; set; }

    /// <summary>
    /// รายการที่แก้ไขค่าแรง =1 
    /// </summary>
    public bool JobPriceEdit { get; set; }

    /// <summary>
    /// ราคาค่าแรงก่อนแก้ไข
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal JobPriceOld { get; set; }

    /// <summary>
    /// เช็คว่ามีพลอยติดตัวเรือนไปหรือไม่
    /// </summary>
    public bool ChkGem { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string GroupSetNo { get; set; } = null!;

    /// <summary>
    /// ค่าซิเนื้อเงิน คิดเป็น %
    /// </summary>
    [Column("DMPercent", TypeName = "decimal(18, 2)")]
    public decimal Dmpercent { get; set; }

    /// <summary>
    /// ราคาค่าแรงช่างคิดบัญชี
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AccPrice { get; set; }

    /// <summary>
    /// เช็คค่าวัตถุดิบ(ปักก้าน)ให้ช่าง 
    /// </summary>
    public bool ChkMaterial { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal EmpMaterial { get; set; }

    [Column("remark1")]
    [StringLength(150)]
    [Unicode(false)]
    public string Remark1 { get; set; } = null!;

    [Column("remark2")]
    [StringLength(150)]
    [Unicode(false)]
    public string Remark2 { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string GroupNo { get; set; } = null!;

    public bool Bill { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal RateChi { get; set; }

    public bool JobQtyEdit { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal MatWg2 { get; set; }

    [Column("e_ttqty", TypeName = "decimal(18, 1)")]
    public decimal ETtqty { get; set; }

    [Column("e_ttlwg", TypeName = "decimal(18, 2)")]
    public decimal ETtlwg { get; set; }

    [Column("e_pricejob", TypeName = "decimal(18, 2)")]
    public decimal EPricejob { get; set; }

    [Column("pass_ok")]
    public bool PassOk { get; set; }

    [Column("pass_date", TypeName = "datetime")]
    public DateTime? PassDate { get; set; }

    [Column("admin_over")]
    public bool AdminOver { get; set; }

    [Column("Qty_job", TypeName = "decimal(10, 1)")]
    public decimal QtyJob { get; set; }

    [Column("complete_component")]
    public bool CompleteComponent { get; set; }

    [Column("bodyWg", TypeName = "decimal(18, 2)")]
    public decimal? BodyWg { get; set; }

    [Column("adjustWg", TypeName = "decimal(18, 2)")]
    public decimal? AdjustWg { get; set; }

    [Column("bodyWg2", TypeName = "decimal(18, 2)")]
    public decimal? BodyWg2 { get; set; }

    [Column("Ttlwg_Old", TypeName = "decimal(18, 2)")]
    public decimal? TtlwgOld { get; set; }

    public bool EditCenterSend { get; set; }

    public bool EditCenterUpdate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EditCenterDate { get; set; }

    [ForeignKey("CustCode")]
    [InverseProperty("JobDetail")]
    public virtual CusProfile CustCodeNavigation { get; set; } = null!;

    [InverseProperty("JobDetail")]
    public virtual ICollection<JobBill> JobBill { get; set; } = new List<JobBill>();

    [ForeignKey("OrderNo, LotNo, Barcode, GroupSetNo, ListNo, GroupNo")]
    [InverseProperty("JobDetail")]
    public virtual OrdLotno OrdLotno { get; set; } = null!;
}
