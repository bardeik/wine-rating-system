namespace WineApp.Shared.Dtos;

public class MobileUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
    public bool IsJudge => Roles.Contains("Judge");
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsWineProducer => Roles.Contains("WineProducer");
}
