using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("JobBill", Schema = "dbo")]
[Index("Barcode", Name = "IX_JobBill_4")]
[Index("Billnumber", "Barcode", Name = "IX_JobBill_5", IsUnique = true)]
[Index("JobBarcode", "Billnumber", Name = "IX_JobBill_6", IsUnique = true)]
[Index("DocNo", "EmpCode", Name = "IX_JobBill_7")]
[Index("JobBarcode", "Num", Name = "IX_JobBill_8", IsUnique = true)]
[Index("Lotno", Name = "IX_JobBill_9")]
public partial class JobBill
{
    [Key]
    public int Billnumber { get; set; }

    [StringLength(12)]
    [Unicode(false)]
    public string JobBarcode { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Lotno { get; set; } = null!;

    [StringLength(14)]
    [Unicode(false)]
    public string Article { get; set; } = null!;

    [StringLength(13)]
    [Unicode(false)]
    public string Barcode { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string ArtCode { get; set; } = null!;

    [StringLength(3)]
    [Unicode(false)]
    public string FnCode { get; set; } = null!;

    [StringLength(12)]
    [Unicode(false)]
    public string DocNo { get; set; } = null!;

    public int EmpCode { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ListNo { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime BillDate { get; set; }

    public bool CheckBill { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OkWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal OkQ12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal EpWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ10 { get; set; }

    [Column(TypeName = "decimal(18, 1)")]
    public decimal EpQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal EpQ12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RtWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal RtQ12 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmTtl { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DmWg { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ1 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ2 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ3 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ4 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ5 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ6 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ7 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ8 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ9 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ10 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ11 { get; set; }

    [Column(TypeName = "decimal(10, 1)")]
    public decimal DmQ12 { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [Column("mDate", TypeName = "datetime")]
    public DateTime MDate { get; set; }

    public bool SendPack { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string PackDoc { get; set; } = null!;

    /// <summary>
    /// เลขลำดับที่ ของ JobBarcode เช่น &apos;01&apos;,&apos;02&apos;
    /// </summary>
    [StringLength(2)]
    [Unicode(false)]
    public string Num { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? SendDate { get; set; }

    public bool SizeNoOrd { get; set; }

    [Column("OKQNoOrd", TypeName = "decimal(10, 1)")]
    public decimal OkqnoOrd { get; set; }

    [Column("OKWNoOrd", TypeName = "decimal(18, 2)")]
    public decimal OkwnoOrd { get; set; }

    [Column("EPQNoOrd", TypeName = "decimal(10, 1)")]
    public decimal EpqnoOrd { get; set; }

    [Column("EPWNoOrd", TypeName = "decimal(18, 2)")]
    public decimal EpwnoOrd { get; set; }

    [Column("RTQNoOrd", TypeName = "decimal(10, 1)")]
    public decimal RtqnoOrd { get; set; }

    [Column("RTWNoOrd", TypeName = "decimal(18, 2)")]
    public decimal RtwnoOrd { get; set; }

    [Column("DMQNoOrd", TypeName = "decimal(10, 1)")]
    public decimal DmqnoOrd { get; set; }

    [Column("DMWNoOrd", TypeName = "decimal(18, 2)")]
    public decimal DmwnoOrd { get; set; }

    /// <summary>
    /// จบงานช่าง ส่งงานครั้งสุดท้าย
    /// </summary>
    public bool CloseLast { get; set; }

    /// <summary>
    /// เศษเนื้อเงิน
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Silver { get; set; }

    [Column("Receive_ok", TypeName = "decimal(10, 1)")]
    public decimal ReceiveOk { get; set; }

    [Column("Damage_RT", TypeName = "decimal(10, 1)")]
    public decimal DamageRt { get; set; }

    [Column("Damage_DM", TypeName = "decimal(10, 1)")]
    public decimal DamageDm { get; set; }

    [Column("wg_over")]
    public bool WgOver { get; set; }

    public bool SendStock { get; set; }

    public bool SendMelt { get; set; }

    [Column("RT_SendStock")]
    public bool RtSendStock { get; set; }

    [Column("RT_SendMelt")]
    public bool RtSendMelt { get; set; }

    [Column("DM_SendStock")]
    public bool DmSendStock { get; set; }

    [Column("DM_SendMelt")]
    public bool DmSendMelt { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string SendStockDoc { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string SendMeltDoc { get; set; } = null!;

    [Column("ok_qtySendPack", TypeName = "decimal(18, 2)")]
    public decimal OkQtySendPack { get; set; }

    [Column("ok_wgSendPack", TypeName = "decimal(18, 2)")]
    public decimal OkWgSendPack { get; set; }

    [Column("ok_qtySentStock", TypeName = "decimal(18, 2)")]
    public decimal OkQtySentStock { get; set; }

    [Column("ok_wgSendStock", TypeName = "decimal(18, 2)")]
    public decimal OkWgSendStock { get; set; }

    [Column("ok_qtySendMelt", TypeName = "decimal(18, 2)")]
    public decimal OkQtySendMelt { get; set; }

    [Column("ok_wgSendMelt", TypeName = "decimal(18, 2)")]
    public decimal OkWgSendMelt { get; set; }

    [Column("RT_qtySendStock", TypeName = "decimal(18, 2)")]
    public decimal RtQtySendStock { get; set; }

    [Column("RT_wgSendStock", TypeName = "decimal(18, 2)")]
    public decimal RtWgSendStock { get; set; }

    [Column("RT_qtySendMelt", TypeName = "decimal(18, 2)")]
    public decimal RtQtySendMelt { get; set; }

    [Column("RT_wgSendMelt", TypeName = "decimal(18, 2)")]
    public decimal RtWgSendMelt { get; set; }

    [Column("DM_qtySendStock", TypeName = "decimal(18, 2)")]
    public decimal DmQtySendStock { get; set; }

    [Column("DM_wgSendStock", TypeName = "decimal(18, 2)")]
    public decimal DmWgSendStock { get; set; }

    [Column("DM_qtySendMelt", TypeName = "decimal(18, 2)")]
    public decimal DmQtySendMelt { get; set; }

    [Column("DM_wgSendMelt", TypeName = "decimal(18, 2)")]
    public decimal DmWgSendMelt { get; set; }

    public int? Qcid { get; set; }

    [Column("sendjobwork")]
    public bool Sendjobwork { get; set; }

    [ForeignKey("JobBarcode, Barcode")]
    [InverseProperty("JobBill")]
    public virtual JobDetail JobDetail { get; set; } = null!;
}
