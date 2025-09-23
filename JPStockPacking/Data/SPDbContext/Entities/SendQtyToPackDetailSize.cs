using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class SendQtyToPackDetailSize
{
    [Key]
    [Column("SendQtyToPackDetailSizeID")]
    public int SendQtyToPackDetailSizeId { get; set; }

    [Column("SendQtyToPackDetailID")]
    public int SendQtyToPackDetailId { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? TtQty { get; set; }

    public int SizeIndex { get; set; }

    public bool IsUnderQuota { get; set; }

    public int? Approver { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
