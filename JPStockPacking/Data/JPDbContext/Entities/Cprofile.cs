using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("CProfile", Schema = "dbo")]
public partial class Cprofile
{
    [StringLength(4)]
    [Unicode(false)]
    public string ArtCode { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string List { get; set; } = null!;

    [Key]
    [StringLength(14)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [Column("EDesArt")]
    [StringLength(30)]
    [Unicode(false)]
    public string EdesArt { get; set; } = null!;

    [Column("TDesArt")]
    [StringLength(50)]
    [Unicode(false)]
    public string TdesArt { get; set; } = null!;

    [Column("EUnit")]
    [StringLength(6)]
    [Unicode(false)]
    public string Eunit { get; set; } = null!;

    [Column("TUnit")]
    [StringLength(10)]
    [Unicode(false)]
    public string Tunit { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string? Grade { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Wg { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? WgModel { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? RingSizeEx { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? OtherSize { get; set; }

    public bool ChkPhoto { get; set; }

    public bool ChkBig { get; set; }

    public bool Chklong { get; set; }

    public bool? ChkSize { get; set; }

    public bool ProtectArt { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string? EmpCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? EmpName { get; set; }

    [Column(TypeName = "text")]
    public string? MarkOrder { get; set; }

    [Column(TypeName = "text")]
    public string? MarkModel { get; set; }

    [Column(TypeName = "text")]
    public string? MarkJob { get; set; }

    [Column(TypeName = "text")]
    public string? MarkPack { get; set; }

    [Column(TypeName = "text")]
    public string? MarkExport { get; set; }

    public int? GemType { get; set; }

    [Column("MDate", TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [StringLength(25)]
    [Unicode(false)]
    public string ComputerName { get; set; } = null!;

    public int? ChkType { get; set; }

    [Column(TypeName = "text")]
    public string MarkCenter { get; set; } = null!;

    /// <summary>
    /// รหัสงานใหม่ (Z)
    /// </summary>
    [StringLength(14)]
    [Unicode(false)]
    public string LinkArticle { get; set; } = null!;

    public bool Cancel { get; set; }

    [Column("IDPIC")]
    [StringLength(20)]
    [Unicode(false)]
    public string Idpic { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreaDate { get; set; }

    /// <summary>
    /// รหัสงาน(เปลี่ยนรหัสใหม่)
    /// </summary>
    [StringLength(14)]
    [Unicode(false)]
    public string LinkArticle1 { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string SupArticle { get; set; } = null!;

    [Column("GemType_CN")]
    public int GemTypeCn { get; set; }

    [Column("Size_Width", TypeName = "decimal(18, 2)")]
    public decimal SizeWidth { get; set; }

    [Column("Size_Length", TypeName = "decimal(18, 2)")]
    public decimal SizeLength { get; set; }

    [Column("Size_High", TypeName = "decimal(18, 2)")]
    public decimal SizeHigh { get; set; }

    [Column("Picture_scale")]
    [StringLength(100)]
    [Unicode(false)]
    public string PictureScale { get; set; } = null!;

    [Column("Pendant_type")]
    [StringLength(1)]
    [Unicode(false)]
    public string PendantType { get; set; } = null!;

    [InverseProperty("ArticleNavigation")]
    public virtual ICollection<CpriceSale> CpriceSale { get; set; } = new List<CpriceSale>();
}
