using System.ComponentModel.DataAnnotations;


namespace DTOs
{
    public record PostUserDTO
    (
        int UserId,
        [EmailAddress]
        [Required]
        string Email,
        string FirstName,
        string LastName,
        [Required]
        string Password
    );
}
