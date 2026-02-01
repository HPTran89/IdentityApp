using API.DTOs;
using API.Services.IServices;
using RestSharp;
using RestSharp.Authenticators;
namespace API.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration configuration = configuration;

        public async Task<bool> SendEmailAsync(EmailSendDto email)
        {
            try
            {
                var options = new RestClientOptions("https://api.mailgun.net")
                {
                    Authenticator = new HttpBasicAuthenticator("api", configuration["Mailgun:key"] ?? "API_KEY")
                };

                var client = new RestClient(options);
                var request = new RestRequest("/v3/sandboxfb82b43e55e3478e913203a50a71e265.mailgun.org/messages", Method.Post);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("from", configuration["Emailgun:From"]);
                request.AddParameter("to", email.To);
                request.AddParameter("subject", !string.IsNullOrWhiteSpace(email.Subject) ? email.Subject : "A Message From Identity App");
                request.AddParameter("text", email.Body);
                await client.ExecuteAsync(request);
                return true;
            
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
