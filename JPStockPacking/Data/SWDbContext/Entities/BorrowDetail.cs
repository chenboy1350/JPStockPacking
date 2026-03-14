using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class BorrowDetail
{
    [Key]
    [Column("BorrowDetailID")]
    public int BorrowDetailId { get; set; }

    [Column("StockID")]
    public int StockId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BorrowNo { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal BorrowQty { get; set; }

    public double BorrowWg { get; set; }

    public int BorrowedBy { get; set; }

    public bool IsReturned { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BorrowDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReturnDate { get; set; }

    public int? UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
