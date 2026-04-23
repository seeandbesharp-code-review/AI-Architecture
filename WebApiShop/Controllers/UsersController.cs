using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly ILogger<UsersController> _logger;
        public UsersController(IUserServices userServices, ILogger<UsersController> logger)
        {
            _userServices = userServices;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Get()
        {
            IEnumerable<UserDTO> users = await _userServices.GetUsers();
            if (users != null && users.Any())
                return Ok(users);
            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            UserDTO? user = await _userServices.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login([FromBody] ExistingUserDTO existingUser)
        {

            UserDTO? user = await _userServices.Login(existingUser);
            if (user== null)
                return Unauthorized("Invalid email or password");

            _logger.LogInformation($"login attempted id:{user.UserId} email:{user.Email} first name:{user.FirstName} last name:{user.LastName}");
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> Register([FromBody] PostUserDTO newUser)
        {
            if (!await _userServices.UserWithSameEmail(newUser.Email))
                return BadRequest("The email already exists. Please try again.");
            if (!_userServices.IsPasswordStrong(newUser.Password))
                return BadRequest("The password is too weak. Please try again.");
            UserDTO? returnedUser = await _userServices.Register(newUser);
            if (returnedUser == null)
                return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = returnedUser.UserId }, returnedUser);


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PostUserDTO updateUser)
        {
            if (!_userServices.IsPasswordStrong(updateUser.Password))
                return BadRequest("The password is too weak. Please try again.");
            await _userServices.Update(id, updateUser);
            return NoContent();
        }
    }
}


