using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("DocNo", "EmpCode")]
[Table("JobHead", Schema = "dbo")]
public partial class JobHead
{
    [Key]
    [StringLength(12)]
    [Unicode(false)]
    public string DocNo { get; set; } = null!;

    public int JobNum { get; set; }

    [Key]
    public int EmpCode { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string EmpName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string JobName { get; set; } = null!;

    public int JobType { get; set; }

    public int OldType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime JobDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DueDate { get; set; }

    public bool BillCancel { get; set; }

    public bool BillReturn { get; set; }

    public bool BillComplete { get; set; }

    public bool ChkReturn { get; set; }

    public bool ChkAccount { get; set; }

    public bool ChkAccountNot { get; set; }

    public bool ChkFoundry { get; set; }

    public bool ChkFoundryNot { get; set; }

    /// <summary>
    /// เช็คราคาค่าแรงรวมค่าเนื้อเงิน
    /// </summary>
    public bool ChkSilver { get; set; }

    /// <summary>
    /// เช็คราคาค่าแรงรวมค่าพลอย
    /// </summary>
    public bool ChkGem { get; set; }

    public int PrintNo { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string PrintBy { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [Column("mDate", TypeName = "datetime")]
    public DateTime MDate { get; set; }

    public int BuryJob { get; set; }

    public int ChkGram { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CancelDate { get; set; }

    public bool Bill { get; set; }

    [Column("Ord_Foundry")]
    public bool OrdFoundry { get; set; }

    [Column("Ord_Foundry_Component")]
    public bool OrdFoundryComponent { get; set; }

    public int AccType { get; set; }

    public int ChkExam { get; set; }

    [Column("Sendback_cancel")]
    public bool SendbackCancel { get; set; }

    [Column("Type_Other")]
    [StringLength(2)]
    [Unicode(false)]
    public string TypeOther { get; set; } = null!;

    [Column("C_OTHER1")]
    public int COther1 { get; set; }

    [StringLength(230)]
    [Unicode(false)]
    public string? SvBatch { get; set; }

    [Unicode(false)]
    public string? CancelRemark { get; set; }

    public int Runid { get; set; }
}
