using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Keyless]
[Table("JobBillsize", Schema = "dbo")]
public partial class JobBillsize
{
    public int Billnumber { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OkWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal EpWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ10 { get; set; }

    [Column(TypeName = "decimal(18, 1)")]
    public decimal EpQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RtWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DmWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ12 { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [Column("mDate", TypeName = "datetime")]
    public DateTime MDate { get; set; }

    [Column("s1")]
    [StringLength(10)]
    [Unicode(false)]
    public string S1 { get; set; } = null!;

    [Column("s2")]
    [StringLength(10)]
    [Unicode(false)]
    public string S2 { get; set; } = null!;

    [Column("s3")]
    [StringLength(10)]
    [Unicode(false)]
    public string S3 { get; set; } = null!;

    [Column("s4")]
    [StringLength(10)]
    [Unicode(false)]
    public string S4 { get; set; } = null!;

    [Column("s5")]
    [StringLength(10)]
    [Unicode(false)]
    public string S5 { get; set; } = null!;

    [Column("s6")]
    [StringLength(10)]
    [Unicode(false)]
    public string S6 { get; set; } = null!;

    [Column("s7")]
    [StringLength(10)]
    [Unicode(false)]
    public string S7 { get; set; } = null!;

    [Column("s8")]
    [StringLength(10)]
    [Unicode(false)]
    public string S8 { get; set; } = null!;

    [Column("s9")]
    [StringLength(10)]
    [Unicode(false)]
    public string S9 { get; set; } = null!;

    [Column("s10")]
    [StringLength(10)]
    [Unicode(false)]
    public string S10 { get; set; } = null!;

    [Column("s11")]
    [StringLength(10)]
    [Unicode(false)]
    public string S11 { get; set; } = null!;

    [Column("s12")]
    [StringLength(10)]
    [Unicode(false)]
    public string S12 { get; set; } = null!;
}
