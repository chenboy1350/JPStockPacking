using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class TrayItem
{
    [Key]
    [Column("TrayItemID")]
    public int TrayItemId { get; set; }

    [Column("TrayID")]
    public int TrayId { get; set; }

    [Column("StockID")]
    public int StockId { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal Qty { get; set; }

    public double Wg { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    [ForeignKey("TrayId")]
    [InverseProperty("TrayItem")]
    public virtual Tray Tray { get; set; } = null!;
}
