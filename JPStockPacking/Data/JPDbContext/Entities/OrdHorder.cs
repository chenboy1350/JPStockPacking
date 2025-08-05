using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("OrdHOrder", Schema = "dbo")]
public partial class OrdHorder
{
    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string OrderNo { get; set; } = null!;

    [StringLength(7)]
    [Unicode(false)]
    public string? CustCode { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string? Company { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Grade { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrdDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DelDate { get; set; }

    [Column("GTotal", TypeName = "decimal(18, 4)")]
    public decimal? Gtotal { get; set; }

    [Column("FOB", TypeName = "decimal(18, 4)")]
    public decimal? Fob { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Etc1 { get; set; }

    [Column("vEtc1", TypeName = "decimal(18, 4)")]
    public decimal? VEtc1 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Etc2 { get; set; }

    [Column("vEtc2", TypeName = "decimal(18, 4)")]
    public decimal? VEtc2 { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? PackType { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? InfPack { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? EarPack { get; set; }

    public int? Shipment { get; set; }

    public int? Payment { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? InvPercent { get; set; }

    [Column(TypeName = "text")]
    public string? Special { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ComName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ContactF { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ContactL { get; set; }

    [Column("SAddress")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Saddress { get; set; }

    [Column("SCity")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Scity { get; set; }

    [Column("SCountry")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Scountry { get; set; }

    [Column("SPostal")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Spostal { get; set; }

    [Column("STel")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Stel { get; set; }

    [Column("SFax")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Sfax { get; set; }

    [Column("BAddress")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Baddress { get; set; }

    [Column("BCity")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Bcity { get; set; }

    [Column("BCountry")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Bcountry { get; set; }

    [Column("BPostal")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Bpostal { get; set; }

    [Column("BTel")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Btel { get; set; }

    [Column("BFax")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Bfax { get; set; }

    [Column("EMail")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Website { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Ref { get; set; }

    public bool? Fair { get; set; }

    public bool? Upd { get; set; }

    public bool? Opened { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? UserName { get; set; }

    [Column("MDate", TypeName = "datetime")]
    public DateTime? Mdate { get; set; }

    /// <summary>
    /// ยืนยันผลิต
    /// </summary>
    public bool Factory { get; set; }

    /// <summary>
    /// วันที่เลื่อนนัดจ่ายงานครั้งที่1
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime? Sled1 { get; set; }

    /// <summary>
    /// วันที่เลื่อนนัดจ่ายงานครั้งที่2
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime? Sled2 { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string RevNo { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string SaleRef { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string ValidDate1 { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SilverRate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ExchangeRate { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Discount { get; set; }

    /// <summary>
    /// ทศนิยม
    /// </summary>
    [Column("decimalPrice")]
    public int DecimalPrice { get; set; }

    [Column("Web_idorder")]
    [StringLength(10)]
    [Unicode(false)]
    public string WebIdorder { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string Currency { get; set; } = null!;

    [Column("Cal_Silver")]
    public bool CalSilver { get; set; }

    [Column("USRate", TypeName = "decimal(18, 2)")]
    public decimal Usrate { get; set; }

    [Column("SilverRate_center", TypeName = "decimal(18, 2)")]
    public decimal SilverRateCenter { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string RangeSilver { get; set; } = null!;

    [Column("BYSale")]
    public int Bysale { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string OrdType { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? FactoryDate { get; set; }
}
