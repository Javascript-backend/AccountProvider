namespace AccountProvider.Models;

public class VerificationsRequest
{
    public string Email { get; set; } = null!;

    public string VerificationCode { get; set; } = null!;
}
