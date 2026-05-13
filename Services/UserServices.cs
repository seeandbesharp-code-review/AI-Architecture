using AutoMapper;
using DTOs;
using Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Services
{
    public class UserServices : IUserServices
    {
        private const int MinimumPasswordScore = 2;
        private readonly IUserRepository _repository;
        private readonly IPasswordServices _passwordServices;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserServices(IUserRepository repository, IPasswordServices passwordServices, IMapper mapper, IConfiguration configuration)
        {
            _repository = repository;
            _passwordServices = passwordServices;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<UserDTO?> GetUserById(int id)
        {
            return _mapper.Map<User?, UserDTO?>(await _repository.GetUserById(id));
        }
        public async Task<UserDTO?> Login(ExistingUserDTO existingUser)
        {
            User? user = await _repository.GetUserByEmail(existingUser.Email);
            if (user == null) return null;
            if (!_passwordServices.VerifyPassword(existingUser.Password, user.Password))
                return null;
            return _mapper.Map<UserDTO>(user);
        }
        public async Task<UserDTO?> Register(PostUserDTO user)
        {
            int passScore = _passwordServices.PasswordScore(user.Password);
            if (passScore < 2)
                return null;

            User userEntity = _mapper.Map<User>(user);
            userEntity.Password = _passwordServices.HashPassword(user.Password);
            return _mapper.Map<UserDTO>(await _repository.Register(userEntity));
        }

        public async Task<bool> Update(int id, PostUserDTO updateUser)
        {
            int passScore = _passwordServices.PasswordScore(updateUser.Password);
            if (passScore < 2)
                return false;
            User user = _mapper.Map<User>(updateUser);
            user.UserId = id;
            user.Password = _passwordServices.HashPassword(updateUser.Password);
            await _repository.Update(id, user);
            return true;
        }

        public async Task<IEnumerable<UserDTO>> GetUsers()
        {
            return _mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(await _repository.GetUsers());
        }
        public async Task<bool> UserWithSameEmail(string email, int id = -1)
        {
            return await _repository.UserWithSameEmail(email, id);
        }
        public bool IsPasswordStrong(string password)
        {
            int passScore = _passwordServices.PasswordScore(password);
            return passScore >= MinimumPasswordScore;
        }
        public string GenerateToken(UserDTO user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

            if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
