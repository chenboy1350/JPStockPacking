using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class SendQtyToPackDetail
{
    [Key]
    [Column("SendQtyToPackDetailID")]
    public int SendQtyToPackDetailId { get; set; }

    [Column("SendQtyToPackID")]
    public int SendQtyToPackId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [Column(TypeName = "decimal(18, 1)")]
    public decimal TtQty { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
