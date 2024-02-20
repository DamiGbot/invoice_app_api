using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Models.Repository;
using InvoiceApp.Data.Requests;
using InvoiceApp.Data.Responses;
using InvoiceApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAppApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InvoiceAppDbContext _context;
        private readonly ITokenService _tokenService;

        public UsersController(UserManager<ApplicationUser> userManager, InvoiceAppDbContext context, ITokenService tokenService, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegistrationRequestDTO requestDto)
        {
            RegistrationRequest request = new RegistrationRequest() 
            { 
                Email = requestDto.Email,
                Username = requestDto.Username,
                Password = requestDto.Password,
                Role = Role.User
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManager.CreateAsync(
                new ApplicationUser { UserName = request.Username, Email = request.Email, Role = Role.User },
                request.Password!
            );

            if (result.Succeeded)
            {
                request.Password = "";
                return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managedUser = await _userManager.FindByEmailAsync(request.Email!);
            if (managedUser == null)
            {
                return BadRequest("Bad credentials");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password!);
            if (!isPasswordValid)
            {
                return BadRequest("Bad credentials");
            }

            var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (userInDb is null)
            {
                return Unauthorized();
            }

            var accessToken = _tokenService.CreateToken(userInDb);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Username = userInDb.UserName,
                Email = userInDb.Email,
                Token = accessToken,
            });
        }
    }
}
