using API.DTOs;

namespace API.Services.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailSendDto email);
    }

}
