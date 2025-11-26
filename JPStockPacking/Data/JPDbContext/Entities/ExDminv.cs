using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("Code", "MinvNo", "LotNo", "Article", "SetNo", "OrderNo", "Efn", "ListGem", "ListGem1", "Barcode")]
[Table("ExDMInv", Schema = "dbo")]
public partial class ExDminv
{
    [Key]
    public int Code { get; set; }

    [Key]
    [Column("MInvNo")]
    [StringLength(9)]
    [Unicode(false)]
    public string MinvNo { get; set; } = null!;

    [StringLength(9)]
    [Unicode(false)]
    public string InvNo { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [Key]
    [StringLength(14)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string SetNo { get; set; } = null!;

    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [Column("EDes")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Edes { get; set; }

    [Key]
    [Column("EFn")]
    [StringLength(100)]
    [Unicode(false)]
    public string Efn { get; set; } = null!;

    [Column("EDesFn1")]
    [StringLength(50)]
    [Unicode(false)]
    public string EdesFn1 { get; set; } = null!;

    [Key]
    [StringLength(100)]
    [Unicode(false)]
    public string ListGem { get; set; } = null!;

    /// <summary>
    /// รายการพลอยที่แก้ไขเอง
    /// </summary>
    [Key]
    [StringLength(100)]
    [Unicode(false)]
    public string ListGem1 { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string? MakeUnit { get; set; }

    [Column("PPerUnit", TypeName = "decimal(18, 4)")]
    public decimal PperUnit { get; set; }

    [Column("PPerG", TypeName = "decimal(18, 4)")]
    public decimal PperG { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? WgPerPc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Ttwg { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? TtQty { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? TtPrice { get; set; }

    [Column("PO")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Po { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? CusNo { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? Box { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Grop { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S1 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S2 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S3 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S4 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S5 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S6 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S7 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S8 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S9 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S10 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S11 { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string? S12 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q1 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q2 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q3 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q4 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q5 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q6 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q7 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q8 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q9 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q10 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q11 { get; set; }

    [Column(TypeName = "decimal(10, 0)")]
    public decimal? Q12 { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode1 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode2 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode3 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode4 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode5 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode6 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode7 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode8 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode9 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode10 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode11 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Barcode12 { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string? UserName { get; set; }

    [Column("MDate", TypeName = "datetime")]
    public DateTime? Mdate { get; set; }

    /// <summary>
    /// Barcode
    /// </summary>
    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    /// <summary>
    /// ลงยาสี
    /// </summary>
    [StringLength(100)]
    [Unicode(false)]
    public string Epoxycolor { get; set; } = null!;

    /// <summary>
    /// ไมีมีประวัติ
    /// </summary>
    [Column("Not_History")]
    public bool NotHistory { get; set; }

    [Column("ID")]
    public int Id { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string SizeZone { get; set; } = null!;

    [Column("chksize1")]
    public bool Chksize1 { get; set; }

    [Column("E_unit")]
    [StringLength(6)]
    [Unicode(false)]
    public string EUnit { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GemWg { get; set; }

    [Column("Order_Cust")]
    [StringLength(20)]
    [Unicode(false)]
    public string OrderCust { get; set; } = null!;

    [Column("Group_Cust")]
    [StringLength(30)]
    [Unicode(false)]
    public string GroupCust { get; set; } = null!;

    [Column("Price_hallMark", TypeName = "decimal(18, 2)")]
    public decimal PriceHallMark { get; set; }

    [Column("hscode")]
    [StringLength(20)]
    [Unicode(false)]
    public string Hscode { get; set; } = null!;

    [Column("nostock")]
    public bool Nostock { get; set; }
}
