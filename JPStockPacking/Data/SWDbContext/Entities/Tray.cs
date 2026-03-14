using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SWDbContext.Entities;

public partial class Tray
{
    [Key]
    [Column("TrayID")]
    public int TrayId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string TrayNo { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    [InverseProperty("Tray")]
    public virtual ICollection<TrayItem> TrayItem { get; set; } = new List<TrayItem>();
}
