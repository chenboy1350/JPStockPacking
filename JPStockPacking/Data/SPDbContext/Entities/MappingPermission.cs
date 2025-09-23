using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class MappingPermission
{
    [Key]
    [Column("MappingPermissionID")]
    public int MappingPermissionId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("PermissionID")]
    public int PermissionId { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
