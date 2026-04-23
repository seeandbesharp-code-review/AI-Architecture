using Entities;
using DTOs;
namespace Services
{
    public interface IUserServices
    {
        Task<UserDTO?> GetUserById(int id);
        Task<UserDTO?> Login(ExistingUserDTO existingUser);
        Task<UserDTO?> Register(PostUserDTO user);
        Task<bool> Update(int id, PostUserDTO updateUser);
        Task<IEnumerable<UserDTO>> GetUsers();
        Task<bool> UserWithSameEmail(string email, int id = -1);
        public bool IsPasswordStrong(string password);
    }
}