using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("Billnumber", "SendType", "ItemSend")]
[Table("JobBill_SendStock", Schema = "dbo")]
[Index("Billnumber", Name = "IX_JobBill_SendStock")]
[Index("Billnumber", "SendType", Name = "IX_JobBill_SendStock_1")]
[Index("SendType", Name = "IX_JobBill_SendStock_2")]
public partial class JobBillSendStock
{
    public int Numsend { get; set; }

    [Key]
    [Column("billnumber")]
    public int Billnumber { get; set; }

    [Key]
    [StringLength(2)]
    [Unicode(false)]
    public string SendType { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Doc { get; set; } = null!;

    [Column("ttqty", TypeName = "decimal(18, 2)")]
    public decimal Ttqty { get; set; }

    [Column("ttwg", TypeName = "decimal(18, 2)")]
    public decimal Ttwg { get; set; }

    public bool SizeNoOrd { get; set; }

    [Column("q1", TypeName = "decimal(18, 2)")]
    public decimal Q1 { get; set; }

    [Column("q2", TypeName = "decimal(18, 2)")]
    public decimal Q2 { get; set; }

    [Column("q3", TypeName = "decimal(18, 2)")]
    public decimal Q3 { get; set; }

    [Column("q4", TypeName = "decimal(18, 2)")]
    public decimal Q4 { get; set; }

    [Column("q5", TypeName = "decimal(18, 2)")]
    public decimal Q5 { get; set; }

    [Column("q6", TypeName = "decimal(18, 2)")]
    public decimal Q6 { get; set; }

    [Column("q7", TypeName = "decimal(18, 2)")]
    public decimal Q7 { get; set; }

    [Column("q8", TypeName = "decimal(18, 2)")]
    public decimal Q8 { get; set; }

    [Column("q9", TypeName = "decimal(18, 2)")]
    public decimal Q9 { get; set; }

    [Column("q10", TypeName = "decimal(18, 2)")]
    public decimal Q10 { get; set; }

    [Column("q11", TypeName = "decimal(18, 2)")]
    public decimal Q11 { get; set; }

    [Column("q12", TypeName = "decimal(18, 2)")]
    public decimal Q12 { get; set; }

    [Column("mdateSend", TypeName = "datetime")]
    public DateTime MdateSend { get; set; }

    [Key]
    [Column("itemSend")]
    [StringLength(2)]
    [Unicode(false)]
    public string ItemSend { get; set; } = null!;

    [Column("return_found")]
    public bool ReturnFound { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Userid { get; set; }

    [Column("SDate", TypeName = "datetime")]
    public DateTime? Sdate { get; set; }

    [Column("SENDPACK")]
    public bool Sendpack { get; set; }
}
