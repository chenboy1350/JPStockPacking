using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("CFnCode", Schema = "dbo")]
public partial class CfnCode
{
    [Key]
    [StringLength(3)]
    [Unicode(false)]
    public string FnCode { get; set; } = null!;

    [Column("EDesFn")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EdesFn { get; set; }

    [Column("TDesFn")]
    [StringLength(100)]
    [Unicode(false)]
    public string? TdesFn { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? FnName { get; set; }

    [Column("MDate", TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// &lt;10
    /// </summary>
    [Column("case1", TypeName = "decimal(18, 2)")]
    public decimal Case1 { get; set; }

    /// <summary>
    /// &gt;=10 and &lt;=20
    /// </summary>
    [Column("case2", TypeName = "decimal(18, 2)")]
    public decimal Case2 { get; set; }

    /// <summary>
    /// &gt;20
    /// </summary>
    [Column("case3", TypeName = "decimal(18, 2)")]
    public decimal Case3 { get; set; }
}
