using BE_JobWeb.PasswordHasher;
using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            //Tạo token
            //AuthenticationRole a = db.AuthenticationRoles.FirstOrDefault(p => p.RoleId == c.RoleId);
            Token t = new Token();
            string token = t.GenerateToken("c.FullName", "2", "c.Id.ToString()");

            return Ok(token);
        }
        [HttpGet("demo")]
        [Authorize(Roles = "2")]
        public IActionResult getdemo()
        {
            JobSeekerContext context = new JobSeekerContext();
            return Ok(context.AuthenticationRoles.ToList());
        }
    }
}
