using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("CPriceSale", Schema = "dbo")]
[Index("Article", "Barcode", "FnCode", "ListGem", "EpoxyColor", "ListMat", Name = "IX_CPriceSale_3", IsUnique = true)]
public partial class CpriceSale
{
    [StringLength(14)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string EpoxyColor { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string ListGem { get; set; } = null!;

    [StringLength(3)]
    [Unicode(false)]
    public string FnCode { get; set; } = null!;

    [Column("TDesFn")]
    [StringLength(100)]
    [Unicode(false)]
    public string TdesFn { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string ListMat { get; set; } = null!;

    [Key]
    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [StringLength(2)]
    [Unicode(false)]
    public string RingSize { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal WgActual { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumStdSup { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSupA { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSup { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumPsv { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostFactory { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string DisCode { get; set; } = null!;

    [Column("PDis", TypeName = "decimal(18, 2)")]
    public decimal Pdis { get; set; }

    [Column("PFactory", TypeName = "decimal(18, 2)")]
    public decimal Pfactory { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string FactoryCode { get; set; } = null!;

    [Column("PProfitFactory", TypeName = "decimal(18, 2)")]
    public decimal PprofitFactory { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PsvProduct { get; set; }

    [Column("PNormalFn", TypeName = "decimal(18, 2)")]
    public decimal PnormalFn { get; set; }

    [Column("PMoveFn", TypeName = "decimal(18, 2)")]
    public decimal PmoveFn { get; set; }

    [Column("PNormalGem", TypeName = "decimal(18, 2)")]
    public decimal PnormalGem { get; set; }

    [Column("PMoveGem", TypeName = "decimal(18, 2)")]
    public decimal PmoveGem { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string ComCode { get; set; } = null!;

    [Column("PCom", TypeName = "decimal(18, 2)")]
    public decimal Pcom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ThaiSale { get; set; }

    [Column("USSale", TypeName = "decimal(18, 2)")]
    public decimal Ussale { get; set; }

    public bool ProtectPrice { get; set; }

    public int ProductType { get; set; }

    [Column("MDate", TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string ComputerName { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Remark { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string? ArtCode { get; set; }

    public int ChkFinish { get; set; }

    public bool ChkFair { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string LinkBar { get; set; } = null!;

    [Column("picture")]
    [StringLength(50)]
    [Unicode(false)]
    public string Picture { get; set; } = null!;

    [Column("import_article")]
    public bool ImportArticle { get; set; }

    [Column("import_price", TypeName = "decimal(18, 2)")]
    public decimal ImportPrice { get; set; }

    [Column("import_wg", TypeName = "decimal(18, 2)")]
    public decimal ImportWg { get; set; }

    [Column("not_silver")]
    public bool NotSilver { get; set; }

    [Column("not_Fn")]
    public bool NotFn { get; set; }

    [Column("not_Gem")]
    public bool NotGem { get; set; }

    [Column("SumStdSup_US", TypeName = "decimal(18, 2)")]
    public decimal SumStdSupUs { get; set; }

    [Column("picture_S")]
    [StringLength(200)]
    [Unicode(false)]
    public string PictureS { get; set; } = null!;

    [Column("picture_M")]
    [StringLength(200)]
    [Unicode(false)]
    public string PictureM { get; set; } = null!;

    [Column("picture_C")]
    [StringLength(200)]
    [Unicode(false)]
    public string PictureC { get; set; } = null!;

    [Column("picture_L")]
    [StringLength(200)]
    [Unicode(false)]
    public string PictureL { get; set; } = null!;

    [Column("picture_R")]
    [StringLength(200)]
    [Unicode(false)]
    public string PictureR { get; set; } = null!;

    [Column("Silver_wg", TypeName = "decimal(18, 2)")]
    public decimal SilverWg { get; set; }

    [Column("Mat_wg", TypeName = "decimal(18, 2)")]
    public decimal MatWg { get; set; }

    [Column("PNormalMatSilver", TypeName = "decimal(18, 2)")]
    public decimal PnormalMatSilver { get; set; }

    [Column("PNormalMat", TypeName = "decimal(18, 2)")]
    public decimal PnormalMat { get; set; }

    [Column("PMoveMat", TypeName = "decimal(18, 2)")]
    public decimal PmoveMat { get; set; }

    [Column("not_Mat")]
    public bool NotMat { get; set; }

    [Column("not_MatSilver")]
    public bool NotMatSilver { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PercentMat { get; set; }

    [Column("Mat_article_wg", TypeName = "decimal(18, 2)")]
    public decimal MatArticleWg { get; set; }

    [Column("PNormalMat_article", TypeName = "decimal(18, 2)")]
    public decimal PnormalMatArticle { get; set; }

    [Column("PMoveMat_article", TypeName = "decimal(18, 2)")]
    public decimal PmoveMatArticle { get; set; }

    [Column("not_Mat_article")]
    public bool NotMatArticle { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal WgSale { get; set; }

    [Column("FNGemCode")]
    [StringLength(1)]
    [Unicode(false)]
    public string FngemCode { get; set; } = null!;

    [Column("FNGemPer_Price", TypeName = "decimal(18, 2)")]
    public decimal FngemPerPrice { get; set; }

    [Column("typegroup")]
    public int Typegroup { get; set; }

    [Column("factorycode_old")]
    [StringLength(1)]
    [Unicode(false)]
    public string FactorycodeOld { get; set; } = null!;

    [Column("gemwg", TypeName = "decimal(18, 2)")]
    public decimal Gemwg { get; set; }

    [ForeignKey("Article")]
    [InverseProperty("CpriceSale")]
    public virtual Cprofile ArticleNavigation { get; set; } = null!;
}
