using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class Formula
{
    [Key]
    [Column("FormulaID")]
    public int FormulaId { get; set; }

    [Column("CustomerGroupID")]
    public int CustomerGroupId { get; set; }

    [Column("PackMethodID")]
    public int PackMethodId { get; set; }

    [Column("ProductTypeID")]
    public int ProductTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    public int Items { get; set; }

    public double P1 { get; set; }

    public double P2 { get; set; }

    public double Avg { get; set; }

    public double ItemPerSec { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
