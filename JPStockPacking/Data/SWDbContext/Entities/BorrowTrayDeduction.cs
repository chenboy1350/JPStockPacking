using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class BorrowTrayDeduction
{
    [Key]
    [Column("BorrowTrayDeductionID")]
    public int BorrowTrayDeductionId { get; set; }

    [Column("BorrowDetailID")]
    public int BorrowDetailId { get; set; }

    [Column("TrayItemID")]
    public int TrayItemId { get; set; }

    [Column(TypeName = "decimal(18, 1)")]
    public decimal DeductedQty { get; set; }

    public double DeductedWg { get; set; }
}
