using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMangoAPI.Data;
using RedMangoAPI.Models;

namespace RedMangoAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private ApiResponse _response;
        private string secretKey;

        public AuthController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _response = new ApiResponse();
        }
    }
}
