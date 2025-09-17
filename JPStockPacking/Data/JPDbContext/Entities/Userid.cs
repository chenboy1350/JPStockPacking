using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.JPDbContext.Entities;

[Table("Userid", Schema = "dbo")]
public partial class Userid
{
    [Column("USERID")]
    [StringLength(10)]
    [Unicode(false)]
    public string Userid1 { get; set; } = null!;

    [Column("PASSWORD")]
    [StringLength(10)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [Column("DESCRIPTION")]
    [StringLength(30)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [Column("USERIDGROUP")]
    [StringLength(15)]
    [Unicode(false)]
    public string Useridgroup { get; set; } = null!;

    [Column("mdate", TypeName = "datetime")]
    public DateTime Mdate { get; set; }

    [Key]
    public int Num { get; set; }
}
