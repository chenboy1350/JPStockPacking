using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class Stock
{
    [Key]
    [Column("StockID")]
    public int StockId { get; set; }

    public int ReceiveId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string ReceiveNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string CustCode { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? TempArticle { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Article { get; set; }

    [Unicode(false)]
    public string? ImgPath { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ListGem { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Unit { get; set; } = null!;

    [Column("EDesFn")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EdesFn { get; set; }

    [Column("EDesArt")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EdesArt { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    public int BillNumber { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal TtQty { get; set; }

    public double TtWg { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    public bool IsRepairing { get; set; }
}
