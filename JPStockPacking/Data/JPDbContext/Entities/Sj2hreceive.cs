using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Keyless]
[Table("SJ2HReceive", Schema = "dbo")]
public partial class Sj2hreceive
{
    [Column("ReceiveNO")]
    [StringLength(10)]
    [Unicode(false)]
    public string ReceiveNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Insure { get; set; } = null!;

    [Column(TypeName = "text")]
    public string Remark { get; set; } = null!;

    [Column("TTPrice", TypeName = "decimal(18, 2)")]
    public decimal Ttprice { get; set; }

    [Column("MUpdate")]
    public bool Mupdate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("upday", TypeName = "datetime")]
    public DateTime Upday { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string Department { get; set; } = null!;

    [StringLength(8)]
    [Unicode(false)]
    public string Docno { get; set; } = null!;

    [Column("cancel")]
    public bool Cancel { get; set; }

    [Column("return_found")]
    public bool ReturnFound { get; set; }
}
