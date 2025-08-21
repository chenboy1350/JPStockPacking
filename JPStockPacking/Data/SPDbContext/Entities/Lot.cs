using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class Lot
{
    [Key]
    [StringLength(50)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? ListNo { get; set; }

    [Column("CustPCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string? CustPcode { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? TtQty { get; set; }

    public double? TtWg { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? Barcode { get; set; }

    [Column("TDesArt")]
    [Unicode(false)]
    public string? TdesArt { get; set; }

    [Column(TypeName = "text")]
    public string? MarkCenter { get; set; }

    [Unicode(false)]
    public string? SaleRem { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? ReceivedQty { get; set; }

    public double? OperateDays { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? AssignedQty { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? ReturnedQty { get; set; }

    public bool IsSuccess { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
