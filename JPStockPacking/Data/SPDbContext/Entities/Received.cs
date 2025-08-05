using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class Received
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string ReceiveNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [Column("TTQty", TypeName = "numeric(18, 0)")]
    public decimal? Ttqty { get; set; }

    [Column("TTwg", TypeName = "numeric(18, 0)")]
    public decimal? Ttwg { get; set; }

    public bool IsReceived { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
