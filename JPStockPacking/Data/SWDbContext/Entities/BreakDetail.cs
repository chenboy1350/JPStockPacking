using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class BreakDetail
{
    [Key]
    [Column("BreakDetailID")]
    public int BreakDetailId { get; set; }

    [Column("StockID")]
    public int StockId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BreakNo { get; set; }

    [Column("BreakDescriptionID")]
    public int BreakDescriptionId { get; set; }

    [Column(TypeName = "decimal(18, 1)")]
    public decimal BreakQty { get; set; }

    public bool IsReported { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
