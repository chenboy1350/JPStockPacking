using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("Ordno", "Type")]
[Table("OrdOrder", Schema = "dbo")]
public partial class OrdOrder
{
    [StringLength(50)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CustDate { get; set; }

    public int SalesNo { get; set; }

    public bool Complete { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CompleteDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SeldDate1 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SeldDate2 { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string? Company { get; set; }

    /// <summary>
    /// เลขที่ Order
    /// </summary>
    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string Ordno { get; set; } = null!;

    /// <summary>
    /// รหัสลูกค้า
    /// </summary>
    [StringLength(7)]
    [Unicode(false)]
    public string Custcode { get; set; } = null!;

    /// <summary>
    /// ประเภทงาน =&apos;0&apos; เงิน,=&apos;1&apos; พลอย ,=&apos;2&apos; ทั้งหมด,เงินใหม่=&apos;3&apos;,พลอยใหม่=&apos;4&apos;,Set=&apos;5&apos;
    /// </summary>
    [Key]
    [StringLength(1)]
    [Unicode(false)]
    public string Type { get; set; } = null!;

    /// <summary>
    /// =0 งานOrder =1 ตั๋วมือ
    /// </summary>
    public bool MakeOrder { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OrderQty { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MakeQty { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CompleteQty { get; set; }

    /// <summary>
    /// รายการตั๋ว Order ที่ลงจำนวนเอง =1 ,0 ดึงรายการจาก Packing
    /// </summary>
    [Column("QOrder")]
    public bool Qorder { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ProductDate { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Jobuser { get; set; } = null!;

    [Column("p_Make", TypeName = "decimal(18, 2)")]
    public decimal PMake { get; set; }

    [Column("p_Complete", TypeName = "decimal(18, 2)")]
    public decimal PComplete { get; set; }

    [Column("num")]
    public int Num { get; set; }

    [ForeignKey("Custcode")]
    [InverseProperty("OrdOrder")]
    public virtual CusProfile CustcodeNavigation { get; set; } = null!;
}
