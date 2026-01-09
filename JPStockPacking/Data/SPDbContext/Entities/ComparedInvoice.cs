using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class ComparedInvoice
{
    [Key]
    [Column("ComparedID")]
    public int ComparedId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string InvoiceNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string CustCode { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string MakeUnit { get; set; } = null!;

    [Column("JPTtQty")]
    public double JpttQty { get; set; }

    [Column("JPPrice")]
    public double Jpprice { get; set; }

    [Column("JPTotalPrice")]
    public double JptotalPrice { get; set; }

    [Column("JPTotalSetTtQty")]
    public double JptotalSetTtQty { get; set; }

    [Column("SPTtQty")]
    public double SpttQty { get; set; }

    [Column("SPPrice")]
    public double Spprice { get; set; }

    [Column("SPTotalPrice")]
    public double SptotalPrice { get; set; }

    [Column("SPTotalSetTtQty")]
    public double SptotalSetTtQty { get; set; }

    public bool IsMatched { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
