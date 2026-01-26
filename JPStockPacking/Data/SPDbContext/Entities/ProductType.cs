using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class ProductType
{
    [Key]
    [Column("ProductTypeID")]
    public int ProductTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    public double? BaseTime { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
