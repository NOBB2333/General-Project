using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformUserSaveInput
{
    [Required]
    [MaxLength(64)]
    [RegularExpression("^[A-Za-z][A-Za-z0-9._-]{2,63}$", ErrorMessage = "账号必须以英文字母开头，只能包含英文、数字、点、下划线或中划线，长度 3-64 位。")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? EmployeeNo { get; set; }

    [MaxLength(64)]
    public string? ExternalSource { get; set; }

    [MaxLength(128)]
    public string? ExternalUserId { get; set; }

    [MaxLength(128)]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? OrganizationUnitId { get; set; }

    [Phone]
    [MaxLength(32)]
    [RegularExpression("^1[3-9]\\d{9}$", ErrorMessage = "手机号格式不正确，请输入 11 位中国大陆手机号。")]
    public string? PhoneNumber { get; set; }

    public List<string> RoleNames { get; set; } = [];
}
