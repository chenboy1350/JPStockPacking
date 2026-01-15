using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class SampleOrder
{
    [Key]
    [StringLength(50)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? CustCode { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FactoryDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SeldDate1 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrdDate { get; set; }

    public bool IsSuccess { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
