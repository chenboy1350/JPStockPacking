using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("CusZoneType", Schema = "dbo")]
public partial class CusZoneType
{
    [Key]
    public int CusZoneId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? CusZoneName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? RefSale { get; set; }
}
