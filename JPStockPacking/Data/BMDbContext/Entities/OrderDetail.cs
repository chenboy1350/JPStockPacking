using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.BMDbContext.Entities;

[Table("OrderDetail", Schema = "dbo")]
public partial class OrderDetail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string CustCode { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [Column("REF_NO")]
    [StringLength(30)]
    [Unicode(false)]
    public string RefNo { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string OrdBarcode { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string RefBarcode { get; set; } = null!;

    public DateOnly ShipDate { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string ListNo { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string Ring { get; set; } = null!;

    [Column("SR")]
    [StringLength(30)]
    [Unicode(false)]
    public string Sr { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string EmpCode { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string EmpName { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Picture { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string? Mask { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Box { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? MaskAndBox { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Core { get; set; }

    [StringLength(30)]
    public string? Sticker1 { get; set; }

    [StringLength(30)]
    public string? Sticker2 { get; set; }

    [StringLength(30)]
    public string? Sticker3 { get; set; }

    [StringLength(30)]
    public string? Sticker4 { get; set; }

    [Column(TypeName = "decimal(12, 4)")]
    public decimal MaskPrice { get; set; }

    [Column(TypeName = "decimal(12, 4)")]
    public decimal BoxPrice { get; set; }

    [Column("mnbPrice", TypeName = "decimal(12, 4)")]
    public decimal MnbPrice { get; set; }

    [Column(TypeName = "decimal(12, 4)")]
    public decimal CorePrice { get; set; }

    [Column("stkPrice1", TypeName = "decimal(12, 4)")]
    public decimal StkPrice1 { get; set; }

    [Column("stkPrice2", TypeName = "decimal(12, 4)")]
    public decimal StkPrice2 { get; set; }

    [Column("stkPrice3", TypeName = "decimal(12, 4)")]
    public decimal StkPrice3 { get; set; }

    [Column("stkPrice4", TypeName = "decimal(12, 4)")]
    public decimal StkPrice4 { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Unit { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string TypeUnit { get; set; } = null!;

    [Column(TypeName = "decimal(18, 4)")]
    public decimal PricePerUnit { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? ChkComp { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? ChkPay { get; set; }

    [Column("jobPrice", TypeName = "decimal(18, 4)")]
    public decimal JobPrice { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string ChkIn { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string ChkOut { get; set; } = null!;

    [Column("dateIn")]
    public DateOnly? DateIn { get; set; }

    [Column("dateOut")]
    public DateOnly? DateOut { get; set; }

    [Column("datePay")]
    public DateOnly? DatePay { get; set; }

    [Column("jobMP", TypeName = "decimal(12, 4)")]
    public decimal? JobMp { get; set; }

    [Column("jobBP", TypeName = "decimal(12, 4)")]
    public decimal? JobBp { get; set; }

    [Column("jobMNBP", TypeName = "decimal(12, 4)")]
    public decimal? JobMnbp { get; set; }

    [Column("jobCP", TypeName = "decimal(12, 4)")]
    public decimal? JobCp { get; set; }

    [Column("jobSTKP1", TypeName = "decimal(12, 4)")]
    public decimal? JobStkp1 { get; set; }

    [Column("jobSTKP2", TypeName = "decimal(12, 4)")]
    public decimal? JobStkp2 { get; set; }

    [Column("jobSTKP3", TypeName = "decimal(12, 4)")]
    public decimal? JobStkp3 { get; set; }

    [Column("jobSTKP4", TypeName = "decimal(12, 4)")]
    public decimal? JobStkp4 { get; set; }

    [Column("jobBarcodeP", TypeName = "decimal(12, 4)")]
    public decimal? JobBarcodeP { get; set; }

    [Column("barcodeP", TypeName = "decimal(12, 4)")]
    public decimal? BarcodeP { get; set; }

    [Column("QTYIn")]
    public int? Qtyin { get; set; }

    [Column("dateExport")]
    public DateOnly? DateExport { get; set; }

    [Column("dateReceive")]
    public DateOnly? DateReceive { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? StatusEdit { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? StatusEditEx { get; set; }

    [Column("dateEdit", TypeName = "datetime")]
    public DateTime? DateEdit { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? StatusEditIn { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? StatusEditOut { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? ChkExport { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? ChkReceive { get; set; }

    [Column("finish")]
    [StringLength(20)]
    [Unicode(false)]
    public string Finish { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DeductAmount { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Cancle { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserNameCreate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserNameEdit { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Country { get; set; }
}
