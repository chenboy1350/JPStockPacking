using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("Orderno", "Lotno")]
[Table("JobCost", Schema = "dbo")]
[Index("Lotno", Name = "IX_JobCost", IsUnique = true)]
public partial class JobCost
{
    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string Orderno { get; set; } = null!;

    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string Lotno { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center1 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost1 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center2 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost2 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center3 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost3 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center41 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost41 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center42 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost42 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center5 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost5 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center6 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost6 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Center7 { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost7 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string List1 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List2 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List3 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List41 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List42 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List5 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List6 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string List7 { get; set; } = null!;

    [Column("QtySI")]
    public int QtySi { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("mdate", TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    public bool? StartJob { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateStart { get; set; }

    [Column("QtySI2")]
    public int QtySi2 { get; set; }
}
