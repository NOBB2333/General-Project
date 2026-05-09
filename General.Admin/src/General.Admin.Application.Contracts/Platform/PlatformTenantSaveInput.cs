using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformTenantSaveInput
{
    public Guid? AdminUserId { get; set; }

    [MaxLength(256)]
    public string? AdminEmail { get; set; }

    [MaxLength(128)]
    public string? AdminPassword { get; set; }

    [MaxLength(64)]
    [RegularExpression("^[A-Za-z][A-Za-z0-9._-]{2,63}$", ErrorMessage = "管理员账号必须以英文字母开头，只能包含英文、数字、点、下划线或中划线，长度 3-64 位。")]
    public string? AdminUserName { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? DefaultConnectionString { get; set; }

    [MaxLength(256)]
    public string? Remark { get; set; }
}
