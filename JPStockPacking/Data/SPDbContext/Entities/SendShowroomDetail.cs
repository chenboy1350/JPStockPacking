using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class SendShowroomDetail
{
    [Key]
    [Column("SendShowroomID")]
    public int SendShowroomId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LotNo { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Doc { get; set; } = null!;

    [Column(TypeName = "numeric(18, 1)")]
    public decimal TtQty { get; set; }

    public double TtWg { get; set; }

    public bool IsSended { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
