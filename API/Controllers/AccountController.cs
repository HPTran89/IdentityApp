using API.DTOs.Account;
using API.Extensions;
using API.Models;
using API.Services.IServices;
using API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IConfiguration config) : ControllerBase
    {
        private readonly UserManager<AppUser> userManager = userManager;
        private readonly SignInManager<AppUser> signInManager = signInManager;
        private readonly ITokenService tokenService = tokenService;
        private readonly IConfiguration config = config;

        [HttpGet("auth-status")]
        public IActionResult IsLoggedIn()
        {
            return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if(await CheckEmailExsitsAsync(registerDto.Email))
            {
                return BadRequest("email taken");
            }
            if (await CheckUsernameExsits(registerDto.Username))
            {
                return BadRequest("username taken");
            }

            var userToAdd = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                EmailConfirmed = true,
                //Roles = new AppUserRoleBridge { }
            };

            var result = await userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded) { return BadRequest(result.Errors); }
            return Ok("Your account has been created, you can now login");
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUserDto>> Login(LoginDto loginDto)
        {
            // allowing the user to login by their email or username
            var user = await userManager.Users.Where(x => x.UserName == loginDto.Username || x.Email == loginDto.Username).FirstOrDefaultAsync();

            if (user == null)
            {
                // no username or email found
                return Unauthorized("Invalid username and/or password");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if(!result.Succeeded)
            {
                RemoveJwtCookie();
                return Unauthorized("Inavlid username and/or password");
            }

            return CreateAppUserDto(user);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            RemoveJwtCookie();
            return NoContent();
        }

        [Authorize]
        [HttpGet("refresh-appuser")]
        public async Task<ActionResult<AppUserDto>> RefreshAppUser()
        {
            var user = await userManager.Users.Where(x => x.Id == User.GetUserId()).FirstOrDefaultAsync();

            if(user == null)
            {
                RemoveJwtCookie();
                return Unauthorized();
            }

            return CreateAppUserDto(user);
        }

        #region private methods
        private async Task<bool> CheckEmailExsitsAsync(string email)
        {
            return await userManager.Users.AnyAsync(x => x.Email == email);
        }
        private async Task<bool> CheckUsernameExsits(string username)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == username);
        }
        private AppUserDto CreateAppUserDto(AppUser user)
        {
            // creating jwt token using TokenService
            string jwt = tokenService.CreateJWT(user);
            SetJwtCookie(jwt);

            return new AppUserDto
            {
                Username = user.UserName,
                JWT = jwt,
            };
        }
        private void SetJwtCookie(string jwt) 
        { 
            var cookieOptions = new CookieOptions
            {
                IsEssential = true,
                HttpOnly= true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:ExpiresInDays"])),
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append(SD.IdentityAppCookie, jwt, cookieOptions);
        }
        private void RemoveJwtCookie()
        {
            Response.Cookies.Delete(SD.IdentityAppCookie);
        }
        #endregion
    }
}
