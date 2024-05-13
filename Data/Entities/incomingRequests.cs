
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class incomingRequests
{
    [Key]
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMinutes(10);
}
