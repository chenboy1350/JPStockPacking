using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class SendQtyToPack
{
    [Key]
    [Column("SendQtyToPackID")]
    public int SendQtyToPackId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
