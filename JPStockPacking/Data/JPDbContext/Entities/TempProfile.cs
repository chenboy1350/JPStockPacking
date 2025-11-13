using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("TEmpProfile", Schema = "dbo")]
[Index("Name", Name = "IX_TEmpProfile", IsUnique = true)]
[Index("EmpCode", "Name", Name = "IX_TEmpProfile_1")]
public partial class TempProfile
{
    public int EmpLink { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? TitleName { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    public bool Foundry { get; set; }

    public bool Dress { get; set; }

    public bool Polish { get; set; }

    public bool Bury { get; set; }

    public bool Bathe { get; set; }

    public bool Complete { get; set; }

    public bool Lee { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Detail { get; set; } = null!;

    [Key]
    public int EmpCode { get; set; }

    public int? RunDoc { get; set; }

    [Column("status")]
    public bool Status { get; set; }

    [Column("empcode_head")]
    public int EmpcodeHead { get; set; }

    [Column("mdate", TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [Column("username")]
    [StringLength(10)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("BType")]
    [StringLength(9)]
    [Unicode(false)]
    public string Btype { get; set; } = null!;

    [Column("wg_Foundry", TypeName = "decimal(18, 2)")]
    public decimal WgFoundry { get; set; }

    [Column("wg_Dress", TypeName = "decimal(18, 2)")]
    public decimal WgDress { get; set; }

    [Column("wg_Polish", TypeName = "decimal(18, 2)")]
    public decimal WgPolish { get; set; }

    [Column("wg_Bury", TypeName = "decimal(18, 2)")]
    public decimal WgBury { get; set; }

    [Column("wg_Bathe", TypeName = "decimal(18, 2)")]
    public decimal WgBathe { get; set; }

    [Column("wg_Complete", TypeName = "decimal(18, 2)")]
    public decimal WgComplete { get; set; }

    [Column("wg_Lee", TypeName = "decimal(18, 2)")]
    public decimal WgLee { get; set; }

    [Column("Bury_Select")]
    public int BurySelect { get; set; }

    public int Item { get; set; }

    [Column("Not_Open")]
    public bool NotOpen { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Silverhold { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SilverOver { get; set; }

    public bool SilverUnlimit { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Remark { get; set; } = null!;

    [Column("Rate_Cal", TypeName = "decimal(18, 6)")]
    public decimal RateCal { get; set; }

    [Column("Lock_wg_Over")]
    public bool LockWgOver { get; set; }

    public bool Company { get; set; }

    [Column("DEmpType")]
    [StringLength(2)]
    [Unicode(false)]
    public string? DempType { get; set; }
}
