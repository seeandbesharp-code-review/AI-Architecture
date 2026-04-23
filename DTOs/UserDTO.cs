using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record UserDTO
    (
        int UserId,
        
        string Email,
        string FirstName,
        string LastName,
        [Required]
        string Password,
        bool IsAdmin,
        ICollection<OrderDTO> Orders
    );
}
