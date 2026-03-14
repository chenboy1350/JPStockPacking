using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class WithdrawalDetail
{
    [Key]
    [Column("WithdrawalDetailID")]
    public int WithdrawalDetailId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? WithdrawalNo { get; set; }

    [Column("StockID")]
    public int StockId { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal Qty { get; set; }

    public double Wg { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? Remark { get; set; }

    public int WithdrawnBy { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    public int? UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
