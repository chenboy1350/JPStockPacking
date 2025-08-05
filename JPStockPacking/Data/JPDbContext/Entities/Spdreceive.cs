using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("ReceiveNo", "Id")]
[Table("SPDReceive", Schema = "dbo")]
public partial class Spdreceive
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string ReceiveNo { get; set; } = null!;

    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string Setno { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Setno1 { get; set; } = null!;

    [StringLength(14)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Lotno { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string Trayno { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Column("TTwg", TypeName = "decimal(18, 2)")]
    public decimal Ttwg { get; set; }

    [Column("TTQty", TypeName = "decimal(18, 2)")]
    public decimal Ttqty { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OldStock { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal NewStock { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string S1 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S2 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S3 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S4 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S5 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S6 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S7 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S8 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S9 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S10 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S11 { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string S12 { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q1 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q2 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q3 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q4 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q5 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q6 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q7 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q8 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q9 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q10 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q11 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Q12 { get; set; }

    [Column(TypeName = "text")]
    public string Remark { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string CusCode { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    public bool Chksize { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UpdateQty { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Boxno { get; set; } = null!;

    [Column("sum_all")]
    public bool SumAll { get; set; }

    [Column("Barcode_Sam")]
    [StringLength(13)]
    [Unicode(false)]
    public string BarcodeSam { get; set; } = null!;

    [StringLength(8)]
    [Unicode(false)]
    public string RequestNo { get; set; } = null!;

    [Column("Request_ID")]
    public int RequestId { get; set; }

    [Column("upday", TypeName = "datetime")]
    public DateTime Upday { get; set; }

    public int Billnumber { get; set; }

    public bool SizeNoOrd { get; set; }

    [Column("set_qty", TypeName = "decimal(18, 2)")]
    public decimal SetQty { get; set; }

    [Column("set_wg", TypeName = "decimal(18, 2)")]
    public decimal SetWg { get; set; }

    [Column("set_price", TypeName = "decimal(18, 2)")]
    public decimal SetPrice { get; set; }

    public bool ChkLotNot { get; set; }

    public int Numsend { get; set; }

    [ForeignKey("ReceiveNo")]
    [InverseProperty("Spdreceive")]
    public virtual Sphreceive ReceiveNoNavigation { get; set; } = null!;
}
