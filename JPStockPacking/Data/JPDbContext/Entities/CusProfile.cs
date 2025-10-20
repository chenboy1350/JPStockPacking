using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("CusProfile", Schema = "dbo")]
public partial class CusProfile
{
    [Key]
    public int Code { get; set; }

    [StringLength(7)]
    [Unicode(false)]
    public string CusCode { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? ComName { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? CusName { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? SurName { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? ConName { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? ConSurName { get; set; }

    [StringLength(240)]
    [Unicode(false)]
    public string? Addr1 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? City1 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? ZipCode1 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Country1 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Tel1 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Fax1 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Addr2 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? City2 { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? Zipcode2 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Country2 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Tel2 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Fax2 { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? Zones { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Website { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BankName { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? BankAcc { get; set; }

    [Column("BAddr")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Baddr { get; set; }

    [Column("BCity")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Bcity { get; set; }

    [Column("BCountry")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Bcountry { get; set; }

    [Column("BZipCode")]
    [StringLength(13)]
    [Unicode(false)]
    public string? BzipCode { get; set; }

    [Column("BTel")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Btel { get; set; }

    [Column("BFax")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Bfax { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? AgentName { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? AgentAcc { get; set; }

    [Column("AAddr")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Aaddr { get; set; }

    [Column("ACity")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Acity { get; set; }

    [Column("ACountry")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Acountry { get; set; }

    [Column("AZipCode")]
    [StringLength(13)]
    [Unicode(false)]
    public string? AzipCode { get; set; }

    [Column("ATel")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Atel { get; set; }

    [Column("AFax")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Afax { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Saler { get; set; }

    public bool? Orders { get; set; }

    public bool? Stock { get; set; }

    public bool? Departmentstore { get; set; }

    public bool? Wholesaler { get; set; }

    public bool? Retailer { get; set; }

    public bool? Importer { get; set; }

    [Column("BOther")]
    public bool? Bother { get; set; }

    [Column("TBother")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Tbother { get; set; }

    public bool? SilverJewelry { get; set; }

    public bool? FashionJewelry { get; set; }

    [Column("JOther")]
    public bool? Jother { get; set; }

    [Column("TJother")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Tjother { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string? Grade { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string? Financial { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Payment { get; set; }

    [Column(TypeName = "text")]
    public string? Mark { get; set; }

    [Column("TGother")]
    [StringLength(4)]
    [Unicode(false)]
    public string? Tgother { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string? Customer { get; set; }

    [Column("cdate", TypeName = "datetime")]
    public DateTime Cdate { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string UserNameF { get; set; } = null!;

    [Column("mDateF", TypeName = "datetime")]
    public DateTime MDateF { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? UserNameL { get; set; }

    [Column("mDateL", TypeName = "datetime")]
    public DateTime? MDateL { get; set; }

    public int? CusZoneId { get; set; }

    public int? ShipmentType { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? CourierName { get; set; }

    public int? FreightType { get; set; }

    public int? TradeType { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? CargoDescription { get; set; }

    [StringLength(240)]
    [Unicode(false)]
    public string? ShipAddress { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? ShipCity { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string? ShipZipCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ShipCountry { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? ShipTel { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? ShipFax { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? CusAccount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdate { get; set; }

    public bool? CurierStatus { get; set; }

    public bool? CargoStatus { get; set; }

    [Column("AGEmail1")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Agemail1 { get; set; }

    [Column("AGEmail2")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Agemail2 { get; set; }

    [Column("AGEmail3")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Agemail3 { get; set; }

    [Column("AGEmail4")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Agemail4 { get; set; }

    [Column("AGEmail5")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Agemail5 { get; set; }

    [Column("AGMobile1")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Agmobile1 { get; set; }

    [Column("AGMobile2")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Agmobile2 { get; set; }

    [Column("AGMobile3")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Agmobile3 { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ContactName1 { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ContactName2 { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ContactName3 { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ContactName4 { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ContactName5 { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Position1 { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Position2 { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Position3 { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Position4 { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Position5 { get; set; }

    [Column("CEmail1")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Cemail1 { get; set; }

    [Column("CEmail2")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Cemail2 { get; set; }

    [Column("CEmail3")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Cemail3 { get; set; }

    [Column("CEmail4")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Cemail4 { get; set; }

    [Column("CEmail5")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Cemail5 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Remark { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ShipContact { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Remark2 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Remark3 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TradeCom { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TradeName { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? TradeAddr { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TradeCity { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TradeZipCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TradeCountry { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? TradeTel { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? TradeFax { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? TradeEmail { get; set; }

    [StringLength(70)]
    [Unicode(false)]
    public string? ShipEmail { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Remark4 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ShipCompany { get; set; }

    public bool? DepositPay { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? DepositValue { get; set; }

    public bool? CreditTerm { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? CreditValue { get; set; }

    [Column("Web_IDCust")]
    [StringLength(10)]
    [Unicode(false)]
    public string WebIdcust { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string SourceFrom { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Bankfee { get; set; } = null!;

    [Column("PathPic_Stamping")]
    [StringLength(100)]
    [Unicode(false)]
    public string PathPicStamping { get; set; } = null!;

    [Column("avail")]
    [StringLength(1)]
    [Unicode(false)]
    public string Avail { get; set; } = null!;

    [Column("User_Del")]
    [StringLength(30)]
    [Unicode(false)]
    public string UserDel { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CreditAmt { get; set; }

    [Column("Commission_Fee", TypeName = "decimal(18, 2)")]
    public decimal? CommissionFee { get; set; }

    [InverseProperty("CustcodeNavigation")]
    public virtual ICollection<OrdOrder> OrdOrder { get; set; } = new List<OrdOrder>();
}
