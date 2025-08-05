using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Keyless]
[Table("JobOrder", Schema = "dbo")]
public partial class JobOrder
{
    [StringLength(8)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Owner { get; set; } = null!;

    public bool ChkComplete { get; set; }

    public bool ChkJobWork { get; set; }

    public bool ChkUpDate { get; set; }
}
