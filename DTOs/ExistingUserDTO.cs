
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record ExistingUserDTO
    (
        [EmailAddress]
        [Required]
        string Email,
        [Required]
        string Password
    );
}
