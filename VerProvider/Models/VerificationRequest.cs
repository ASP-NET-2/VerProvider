namespace VerProvider.Models;

public class VerificationRequest
{
    public string Email { get; set; } = null!;
    public string? Code { get; set; }
}
