namespace AccountProvider.Models;

public class VerificationsRequest
{
    public string Email { get; set; } = null!;

    public string Code { get; set; } = null!;
}
