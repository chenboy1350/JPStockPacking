using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class SampleRecieved
{
    [Key]
    [Column("SampleReceiveID")]
    public int SampleReceiveId { get; set; }

    public int ReceiveId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string ReceiveNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    public int BillNumber { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? TtQty { get; set; }

    public double? TtWg { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Mdate { get; set; }

    public bool IsReceived { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
