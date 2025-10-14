using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Lijan;

[Table("AdminUser")]
[Index("Email", Name = "UQ__AdminUse__A9D1053454604762", IsUnique = true)]
public partial class AdminUser
{
    [Key]
    public long AdminUserId { get; set; }

    [StringLength(200)]
    public string Email { get; set; } = null!;

    [StringLength(200)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(200)]
    public string? DisplayName { get; set; }

    [StringLength(20)]
    public string RoleName { get; set; } = null!;

    public bool IsActive { get; set; }

    public long? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
