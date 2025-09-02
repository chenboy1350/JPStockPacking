using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class Returned
{
    [Key]
    [Column("ReturnID")]
    public int ReturnId { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? LostQty { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? BreakQty { get; set; }

    [Column(TypeName = "numeric(18, 1)")]
    public decimal? ReturnTtQty { get; set; }

    public bool HasLost { get; set; }

    public bool HasRepair { get; set; }

    public bool IsSuccess { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
