using API.Data;
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
    public class AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ApiCoreController
    {
        private readonly UserManager<AppUser> userManager = userManager;
        private readonly SignInManager<AppUser> signInManager = signInManager;

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
                EmailConfirmed = false,
                Name = registerDto.Name,
                LockoutEnabled = true
            };

            var result = await userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded) { return BadRequest(result.Errors); }

            try
            {
                if (await SendConfirmationEmailAsync(userToAdd))
                {
                    return Ok(new ApiResponse(201, title: SM.T_AccountCreated, message: SM.M_AccountCreated));
                }

                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed, displayByDefault: true));

            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed, displayByDefault: true));
            }
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

        [HttpPut("confirm-email")]
        public async Task<ActionResult<ApiResponse>> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse(401, title: SM.T_InvallidToken, message: SM.M_InavlidToken, displayByDefault: true));
            }

            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse(400, title: SM.M_AccountSuspended, message: SM.M_AccountSuspended, displayByDefault: true));
            }

            if (user.EmailConfirmed == true)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_AccountWasConfirmed, message: SM.M_AccountWasConfirmed, displayByDefault: true));
            }

            var appUserToken = await context.AppUserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.EC);

            if (appUserToken == null || appUserToken.Expires <= DateTime.UtcNow) {

                if (appUserToken != null)
                {
                    // we are removing the token becuase it is expired .
                    context.AppUserTokens.Remove(appUserToken);
                    await context.SaveChangesAsync();
                }

                return Unauthorized(new ApiResponse(401, title: SM.T_InvallidToken, message: SM.M_InavlidToken, displayByDefault: true));
            }

            context.AppUserTokens.Remove(appUserToken);
            user.EmailConfirmed = true;
            await context.SaveChangesAsync();

            return Ok(new ApiResponse(200, title: SM.T_EmailConfirmed, message: SM.M_EmailConfirmed));

        }

        [HttpPost("resend-confirmation-email")]
        public async Task<ActionResult<ApiResponse>> ResendConfirmationEmail(EmailDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                // sending a vague response with a fake delay
                PauseResponse();
                return Ok(new ApiResponse(200, title: SM.T_EmailSent, message: SM.M_ConfirmEmailSend));
            }

            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse(400, title: SM.M_AccountSuspended, message: SM.M_AccountSuspended, displayByDefault: true));
            }
            if (user.EmailConfirmed == true)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_AccountWasConfirmed, message: SM.M_AccountWasConfirmed, displayByDefault: true));
            }

            try
            {
                if (await SendConfirmationEmailAsync(user))
                {
                    return Ok(new ApiResponse(200, title: SM.T_EmailSent, message: SM.M_ConfirmEmailSend));
                }

                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed, displayByDefault: true));

            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed, displayByDefault: true));
            }
        }

        [HttpPost("forgot-username-or-password")]
        public async Task<ActionResult<ApiResponse>> ForgotUsernameOrPassword(EmailDto email)
        {
            var user = await userManager.FindByEmailAsync(email.Email);
            if (user == null)
            {
                // sending a vague response with a fake delay
                PauseResponse();
                return Ok(new ApiResponse(200, title: SM.T_EmailSent, message: SM.M_ConfirmEmailSend));
            }

            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse(401, title: SM.M_AccountSuspended, message: SM.M_AccountSuspended, displayByDefault: true));
            }
            if (!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_ConfirmEmailFirst, message: SM.M_ConfirmEmailFirst, displayByDefault: true));
            }

            try
            {
                if(await SendUsernameOrPasswordEmail(user))
                {
                    return Ok(new ApiResponse(200, title: SM.T_EmailSent, message: SM.M_ForgotUsernamePasswordSent));
                }

                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed, displayByDefault: true));
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed, displayByDefault: true));
            }
        }

        [HttpPut("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse(401, title: SM.T_InvallidToken, message: SM.M_InavlidToken,
                    displayByDefault: true));
            }

            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse(401, title: SM.T_AccountSuspended, message: SM.M_AccountSuspended,
                    displayByDefault: true));
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_ConfirmEmailFirst, message: SM.M_ConfirmEmailFirst,
                    displayByDefault: true));
            }

            var appUserToken = await context.AppUserTokens
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.FUP && x.Value == model.Token);
            if (appUserToken == null || appUserToken.Expires <= DateTime.UtcNow)
            {
                if (appUserToken != null)
                {
                    context.RemoveRange(appUserToken);
                    await context.SaveChangesAsync();
                }

                return Unauthorized(new ApiResponse(401, title: SM.T_InvallidToken, message: SM.M_InavlidToken,
                    displayByDefault: true));
            }

            context.AppUserTokens.Remove(appUserToken);
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, model.NewPassword);

            return Ok(new ApiResponse(200, title: SM.T_PasswordRest, message: SM.M_PasswordRest));
        }
        public IActionResult Logout()
        {
            RemoveJwtCookie();
            return NoContent();
        }

        //[HttpGet("test-email")]
        //public async Task<IActionResult> TestEmail()
        //{
        //    var email = new EmailSendDto("d", "s", "a");
        //    await Services.EmailService.SendEmailAsync(email);
        //    return Ok();
        //}

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
            string jwt = Services.TokenService.CreateJWT(user);
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

        private async Task<bool> SendConfirmationEmailAsync(AppUser user)
        {
            var userToken = await context.AppUserTokens.Where(x => x.UserId == user.Id && x.Name == SD.EC).FirstOrDefaultAsync();

            var tokenExpiresInMinute = TokenExpiresInMinute();

            if (userToken == null)
            {
                var userTokenToAdd = new AppUserToken
                {
                    UserId = user.Id,
                    Name = SD.EC,
                    Value = SD.GenerateRandomString(),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinute),
                    LoginProvider = string.Empty
                };

                context.AppUserTokens.Add(userTokenToAdd);
                userToken = userTokenToAdd;
            }
            else
            {
                userToken.Value = SD.GenerateRandomString();
                userToken.Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinute);
            }
            await context.SaveChangesAsync();

            using StreamReader streamReader = System.IO.File.OpenText("EmailTemplates/confirm_email.html");
            string htmlBody = streamReader.ReadToEnd();
            string clientUrl = GetClientUrl();

            string messageBody = string.Format(htmlBody, clientUrl, user.Name, user.UserName, user.Email, userToken.Value, tokenExpiresInMinute);

            var emailSend = new EmailSendDto(user.Email, "Verfiy Your Email", messageBody);

            return await Services.EmailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> SendUsernameOrPasswordEmail(AppUser user)
        {
            var userToken = await context.AppUserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.FUP);
            var tokenExpiresInMinutes = TokenExpiresInMinute();

            if (userToken == null)
            {
                var userTokenToAdd = new AppUserToken
                {
                    UserId = user.Id,
                    Name = SD.FUP,
                    Value = SD.GenerateRandomString(),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes),
                    LoginProvider = string.Empty
                };
                context.AppUserTokens.Add(userTokenToAdd);
                userToken = userTokenToAdd;
            }
            else
            {
                userToken.Value = SD.GenerateRandomString();
                userToken.Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes);
            }

            await context.SaveChangesAsync();

            using StreamReader sr = System.IO.File.OpenText("EmailTemplates/forgot_username_password.html");
            string htmlBody = sr.ReadToEnd();

            string messageBody = string.Format(htmlBody, GetClientUrl(), user.Name, user.UserName, user.Email, userToken.Value, tokenExpiresInMinutes);

            var emailSend = new EmailSendDto(user.Email, "Forgot username or password", messageBody);

            return await Services.EmailService.SendEmailAsync(emailSend);
        }
        #endregion
    }
}
