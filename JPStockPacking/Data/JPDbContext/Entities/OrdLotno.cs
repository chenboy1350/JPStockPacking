using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("OrderNo", "LotNo", "Barcode", "ListNo", "GroupNo", "GroupSetNo")]
[Table("OrdLotno", Schema = "dbo")]
public partial class OrdLotno
{
    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? TrayNo { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo1 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo2 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo3 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo4 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo5 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo6 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo7 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo8 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo9 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? SetNo10 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string Barcodemat { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [Column("EDesFn")]
    [StringLength(20)]
    [Unicode(false)]
    public string? EdesFn { get; set; }

    [Column("CustPCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CustPcode { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Wg { get; set; }

    public double? Price { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? TtQty { get; set; }

    public double? PriceWg { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? TtWg { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? TtPrice { get; set; }

    public bool? ChkSize { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Unit { get; set; } = null!;

    public int? SaleType { get; set; }

    [Column("US")]
    public bool? Us { get; set; }

    [Column(TypeName = "text")]
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

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SetPrice { get; set; }

    [Column("CSetPrice", TypeName = "decimal(18, 3)")]
    public decimal CsetPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SetWg { get; set; }

    public int SetQty { get; set; }

    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string ListNo { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string GroupNo { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string GroupSetNo { get; set; } = null!;

    [Column("CNOrder")]
    public bool Cnorder { get; set; }

    public bool Cancel { get; set; }

    [Column("Lotno_Link")]
    [StringLength(10)]
    [Unicode(false)]
    public string LotnoLink { get; set; } = null!;

    [Column("type_lot")]
    [StringLength(1)]
    [Unicode(false)]
    public string TypeLot { get; set; } = null!;
}
