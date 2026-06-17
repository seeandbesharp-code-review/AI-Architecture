using AutoMapper;
using DTOs;
using Entities;
using Repositories;

namespace Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordServices _passwordServices;
        private readonly IMapper _mapper;

        public UserServices(IUserRepository repository, IPasswordServices passwordServices, IMapper mapper)
        {
            _repository = repository;
            _passwordServices = passwordServices;
            _mapper = mapper;
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
            if (!_passwordServices.IsPasswordStrong(user.Password))
                return null;

            User userEntity = _mapper.Map<User>(user);
            userEntity.Password = _passwordServices.HashPassword(user.Password);
            return _mapper.Map<UserDTO>(await _repository.Register(userEntity));
        }

        public async Task<bool> Update(int id, PostUserDTO updateUser)
        {
            if (!_passwordServices.IsPasswordStrong(updateUser.Password))
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
    }
}

