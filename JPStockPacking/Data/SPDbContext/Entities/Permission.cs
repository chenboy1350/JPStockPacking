﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Data.SPDbContext.Entities;

public partial class Permission
{
    [Key]
    [Column("PermissionID")]
    public int PermissionId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    public bool IsMenu { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }
}
