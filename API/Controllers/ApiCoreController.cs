using API.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    /// <summary>
    /// For common dependency injections for all other controllers
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCoreController : ControllerBase
    {
        private Context _context;
        private IConfiguration _config;

        protected IConfiguration Configuration => _config ??= HttpContext.RequestServices.GetService<IConfiguration>();

        protected Context context => _context ??= HttpContext.RequestServices.GetService<Context>();
    }
}
