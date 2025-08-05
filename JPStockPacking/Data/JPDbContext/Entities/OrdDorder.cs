using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("OrderNo", "SetNo", "Barcode", "Num")]
[Table("OrdDOrder", Schema = "dbo")]
public partial class OrdDorder
{
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? StamP { get; set; }

    [Column(TypeName = "decimal(18, 5)")]
    public decimal? BarP { get; set; }

    [Column(TypeName = "decimal(18, 5)")]
    public decimal? CardP { get; set; }

    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string? LotNo { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? TrayNo { get; set; }

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string SetNo { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Barcodemat { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [Column("EDesFn")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EdesFn { get; set; }

    [Column("CustPCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CustPcode { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Wg { get; set; }

    public double? Price { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? TtQty { get; set; }

    public double? PriceWg { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TtWg { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? TtPrice { get; set; }

    public bool? ChkSize { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Unit { get; set; } = null!;

    /// <summary>
    /// 1=หน่วย,2=น้ำหนัก
    /// </summary>
    public int? SaleType { get; set; }

    /// <summary>
    /// 1=US,0=Thai
    /// </summary>
    [Column("US")]
    public bool? Us { get; set; }

    [StringLength(600)]
    [Unicode(false)]
    public string? SaleRem { get; set; }

    [Column(TypeName = "text")]
    public string? Remark { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? SizeZone { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S1 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S2 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S3 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S4 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S5 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S6 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S7 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S8 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S9 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S10 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S11 { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string? S12 { get; set; }

    [Column(TypeName = "decimal(18, 1)")]
    public decimal? Q1 { get; set; }

    public int? Q2 { get; set; }

    public int? Q3 { get; set; }

    public int? Q4 { get; set; }

    public int? Q5 { get; set; }

    public int? Q6 { get; set; }

    public int? Q7 { get; set; }

    public int? Q8 { get; set; }

    public int? Q9 { get; set; }

    public int? Q10 { get; set; }

    public int? Q11 { get; set; }

    public int? Q12 { get; set; }

    [Column("CS1")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs1 { get; set; }

    [Column("CS2")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs2 { get; set; }

    [Column("CS3")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs3 { get; set; }

    [Column("CS4")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs4 { get; set; }

    [Column("CS5")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs5 { get; set; }

    [Column("CS6")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs6 { get; set; }

    [Column("CS7")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs7 { get; set; }

    [Column("CS8")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs8 { get; set; }

    [Column("CS9")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs9 { get; set; }

    [Column("CS10")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs10 { get; set; }

    [Column("CS11")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs11 { get; set; }

    [Column("CS12")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Cs12 { get; set; }

    [StringLength(14)]
    [Unicode(false)]
    public string Zarticle { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Zbarcode { get; set; } = null!;

    public int Recno { get; set; }

    public bool EditOrder { get; set; }

    [Column("EditEDesFn")]
    [StringLength(50)]
    [Unicode(false)]
    public string EditEdesFn { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string EditListGem { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string EditEpoxyColor { get; set; } = null!;

    [Column("RemarkOC", TypeName = "text")]
    public string RemarkOc { get; set; } = null!;

    [Column("CNOrder")]
    public bool Cnorder { get; set; }

    [Column("CNEmpcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cnempcode { get; set; } = null!;

    [Column("CNArticle")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cnarticle { get; set; } = null!;

    public bool Cancel { get; set; }

    [Key]
    [Column("num")]
    public int Num { get; set; }
}
