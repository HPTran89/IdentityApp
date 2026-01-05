using API.DTOs;
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
    public class AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService) : ApiCoreController
    {
        private readonly UserManager<AppUser> userManager = userManager;
        private readonly SignInManager<AppUser> signInManager = signInManager;
        private readonly ITokenService tokenService = tokenService;

        [HttpGet("auth-status")]
        public IActionResult IsLoggedIn()
        {
            return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (await CheckEmailExsitsAsync(registerDto.Email))
            {
                return BadRequest(new ApiResponse(401, message: "email taken"));
            }
            if (await CheckNameExists(registerDto.Name))
            {
                return BadRequest(new ApiResponse(401, message: "name taken"));
            }

            var userToAdd = new AppUser
            {
                UserName = registerDto.Name.ToLower(),
                Email = registerDto.Email,
                EmailConfirmed = true,
                Name = registerDto.Name,
                LockoutEnabled = true
                //Roles = new AppUserRoleBridge { }
            };

            var result = await userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded) { return BadRequest(result.Errors); }
            return Ok(new ApiResponse(401, message: "Your account has been created, you can now login"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUserDto>> Login(LoginDto loginDto)
        {
            // allowing the user to login by their email or username
            var user = await userManager.Users.Where(x => x.UserName == loginDto.Username || x.Email == loginDto.Username).FirstOrDefaultAsync();

            if (user == null)
            {
                // no username or email found
                return Unauthorized(new ApiResponse(401, message: "Inavlid username and/or password"));
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, true);

            if (!result.Succeeded)
            {
                RemoveJwtCookie();
                if (result.IsLockedOut)
                {
                    return Unauthorized(new ApiResponse(401, title: "Account locked", message: SD.AccountLockedMessage(user.LockoutEnd.Value.DateTime), isHtmlEnabled: true, displayByDefault: true));
                }
                return Unauthorized(new ApiResponse(401, message: "Inavlid username and/or password"));
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

            if (user == null)
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401));
            }

            return CreateAppUserDto(user);
        }

        [HttpGet("name-taken")]
        public async Task<IActionResult> NameTaken([FromQuery] string name)
        {
            return Ok(new { IsTaken = await CheckNameExists(name) });
        }

        [HttpGet("email-taken")]
        public async Task<IActionResult> EmailTaken([FromQuery] string name)
        {
            return Ok(new { IsTaken = await CheckEmailExsitsAsync(name) });
        }

        #region private methods
        private async Task<bool> CheckEmailExsitsAsync(string email)
        {
            return await userManager.Users.AnyAsync(x => x.Email == email);
        }
        private async Task<bool> CheckNameExists(string name)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == name.ToLower());
        }
        private AppUserDto CreateAppUserDto(AppUser user)
        {
            // creating jwt token using TokenService
            string jwt = tokenService.CreateJWT(user);
            SetJwtCookie(jwt);

            return new AppUserDto
            {
                Name = user.Name,
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
                Expires = DateTime.UtcNow.AddDays(int.Parse(Configuration["JWT:ExpiresInDays"])),
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
