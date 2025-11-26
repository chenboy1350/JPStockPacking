using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[PrimaryKey("MinvNo", "InvNo")]
[Table("ExHMInv", Schema = "dbo")]
public partial class ExHminv
{
    [Key]
    [Column("MInvNo")]
    [StringLength(9)]
    [Unicode(false)]
    public string MinvNo { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? InvDate { get; set; }

    [Key]
    [StringLength(9)]
    [Unicode(false)]
    public string InvNo { get; set; } = null!;

    [StringLength(7)]
    [Unicode(false)]
    public string? CusCode { get; set; }

    [Column("MDate", TypeName = "datetime")]
    public DateTime? Mdate { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? UserName { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Plus1 { get; set; }

    [Column("PPlus1", TypeName = "decimal(18, 4)")]
    public decimal? Pplus1 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Plus2 { get; set; }

    [Column("PPlus2", TypeName = "decimal(18, 4)")]
    public decimal? Pplus2 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Plus3 { get; set; }

    [Column("PPlus3", TypeName = "decimal(18, 4)")]
    public decimal? Pplus3 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Plus4 { get; set; }

    [Column("PPlus4", TypeName = "decimal(18, 4)")]
    public decimal? Pplus4 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Plus5 { get; set; }

    [Column("PPlus5", TypeName = "decimal(18, 4)")]
    public decimal? Pplus5 { get; set; }

    [Column("TPPlus", TypeName = "decimal(18, 4)")]
    public decimal? Tpplus { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Less1 { get; set; }

    [Column("PLess1", TypeName = "decimal(18, 4)")]
    public decimal? Pless1 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Less2 { get; set; }

    [Column("PLess2", TypeName = "decimal(18, 4)")]
    public decimal? Pless2 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Less3 { get; set; }

    [Column("PLess3", TypeName = "decimal(18, 4)")]
    public decimal? Pless3 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Less4 { get; set; }

    [Column("PLess4", TypeName = "decimal(18, 4)")]
    public decimal? Pless4 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Less5 { get; set; }

    [Column("PLess5", TypeName = "decimal(18, 4)")]
    public decimal? Pless5 { get; set; }

    [Column("TPless", TypeName = "decimal(18, 4)")]
    public decimal? Tpless { get; set; }

    [Column(TypeName = "ntext")]
    public string? Condition { get; set; }

    [Column("currency")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Currency { get; set; }

    [Column("payment")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Payment { get; set; }

    [Column("shipment")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Shipment { get; set; }

    [Column("awb_no")]
    [StringLength(100)]
    [Unicode(false)]
    public string AwbNo { get; set; } = null!;

    [Column("destination")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Destination { get; set; }

    [Column("destination_CITY")]
    [StringLength(100)]
    [Unicode(false)]
    public string? DestinationCity { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Insurance { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Freight { get; set; }

    [Column("PFob", TypeName = "decimal(18, 4)")]
    public decimal? Pfob { get; set; }

    /// <summary>
    /// เก็บ Fob ก่อน Adjust ราคารวม
    /// </summary>
    [Column("PFob1", TypeName = "decimal(18, 4)")]
    public decimal Pfob1 { get; set; }

    [Column("PInsurance", TypeName = "decimal(18, 4)")]
    public decimal? Pinsurance { get; set; }

    [Column("PFreight", TypeName = "decimal(18, 4)")]
    public decimal? Pfreight { get; set; }

    [Column("PPostage", TypeName = "decimal(18, 4)")]
    public decimal? Ppostage { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Until { get; set; }

    [Column("PUntil", TypeName = "decimal(18, 4)")]
    public decimal? Puntil { get; set; }

    [Column("TtPFob", TypeName = "decimal(18, 4)")]
    public decimal? TtPfob { get; set; }

    [Column("TtPPlus", TypeName = "decimal(18, 4)")]
    public decimal? TtPplus { get; set; }

    [Column("TtPLess", TypeName = "decimal(18, 4)")]
    public decimal? TtPless { get; set; }

    [Column("PGrandTt", TypeName = "decimal(18, 4)")]
    public decimal? PgrandTt { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? NetWg { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? GrossWg { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? TtBox { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Adjust { get; set; }

    [Column("Address_send", TypeName = "text")]
    public string AddressSend { get; set; } = null!;

    [Column("Customer_name", TypeName = "text")]
    public string CustomerName { get; set; } = null!;

    [StringLength(5)]
    [Unicode(false)]
    public string Agreement { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Header { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? ToOrderOf { get; set; }

    [Column("Applicant_Name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ApplicantName { get; set; }

    [Column("Applicant_Add1")]
    [StringLength(200)]
    [Unicode(false)]
    public string? ApplicantAdd1 { get; set; }

    [Column("Notify_Name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? NotifyName { get; set; }

    [Column("Notify_Add1")]
    [StringLength(50)]
    [Unicode(false)]
    public string? NotifyAdd1 { get; set; }

    [Column("Notify_Add2")]
    [StringLength(50)]
    [Unicode(false)]
    public string? NotifyAdd2 { get; set; }

    [Column("Notify_Add3")]
    [StringLength(50)]
    [Unicode(false)]
    public string? NotifyAdd3 { get; set; }

    [Column("Notify_Add4")]
    [StringLength(50)]
    [Unicode(false)]
    public string? NotifyAdd4 { get; set; }

    [Column("Consignee_Name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ConsigneeName { get; set; }

    [Column("Consignee_Add1")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ConsigneeAdd1 { get; set; }

    [Column("Consignee_Add2")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ConsigneeAdd2 { get; set; }

    [Column("Consignee_Add3")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ConsigneeAdd3 { get; set; }

    [Column("Consignee_Add4")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ConsigneeAdd4 { get; set; }

    [Column("WareHouse_Name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? WareHouseName { get; set; }

    [Column("WareHouse_Add1")]
    [StringLength(50)]
    [Unicode(false)]
    public string? WareHouseAdd1 { get; set; }

    [Column("WareHouse_Add2")]
    [StringLength(50)]
    [Unicode(false)]
    public string? WareHouseAdd2 { get; set; }

    [Column("WareHouse_Add3")]
    [StringLength(50)]
    [Unicode(false)]
    public string? WareHouseAdd3 { get; set; }

    [Column("WareHouse_Add4")]
    [StringLength(50)]
    [Unicode(false)]
    public string? WareHouseAdd4 { get; set; }

    /// <summary>
    /// ปรับปรุง Stock=1
    /// </summary>
    public bool UpStock { get; set; }

    /// <summary>
    /// เลขที่ใบเบิกตัด Stock
    /// </summary>
    [StringLength(8)]
    [Unicode(false)]
    public string Docno { get; set; } = null!;

    public int Mdecimal { get; set; }

    [StringLength(9)]
    [Unicode(false)]
    public string ProNo { get; set; } = null!;

    /// <summary>
    /// บริหาร
    /// </summary>
    [Column(TypeName = "decimal(18, 4)")]
    public decimal Ttaccount1 { get; set; }

    /// <summary>
    /// บัญชี
    /// </summary>
    [Column(TypeName = "decimal(18, 4)")]
    public decimal Ttaccount2 { get; set; }

    [Column("remark_Dimension")]
    [StringLength(50)]
    [Unicode(false)]
    public string RemarkDimension { get; set; } = null!;

    public bool InvSend { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SendDate { get; set; }

    /// <summary>
    /// Value Weight
    /// </summary>
    [Column("VW", TypeName = "decimal(18, 4)")]
    public decimal Vw { get; set; }

    public bool Cancel { get; set; }

    [Column("USRate", TypeName = "decimal(18, 2)")]
    public decimal Usrate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ExchangeRate { get; set; }

    [Column("Vat_percent", TypeName = "decimal(18, 2)")]
    public decimal VatPercent { get; set; }

    [Column("Vat_price", TypeName = "decimal(18, 2)")]
    public decimal VatPrice { get; set; }

    [Column("inv_type1")]
    public int InvType1 { get; set; }

    [Column("inv_type2")]
    public int InvType2 { get; set; }

    [Column("inv_type3")]
    public int InvType3 { get; set; }

    [Column("inv_repno")]
    [StringLength(30)]
    [Unicode(false)]
    public string InvRepno { get; set; } = null!;

    [Column("inv_Date1", TypeName = "datetime")]
    public DateTime? InvDate1 { get; set; }

    [Column("inv_remark1")]
    [StringLength(50)]
    [Unicode(false)]
    public string InvRemark1 { get; set; } = null!;

    [Column("inv_Date2", TypeName = "datetime")]
    public DateTime? InvDate2 { get; set; }

    [Column("inv_remark2")]
    [StringLength(50)]
    [Unicode(false)]
    public string InvRemark2 { get; set; } = null!;

    [Column("Bank_Item")]
    public int BankItem { get; set; }

    [ForeignKey("CusCode")]
    [InverseProperty("ExHminv")]
    public virtual CusProfile? CusCodeNavigation { get; set; }
}
