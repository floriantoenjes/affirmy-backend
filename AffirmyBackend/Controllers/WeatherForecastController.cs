using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AffirmyBackend.Areas.Identity.Data;
using AffirmyBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AffirmyBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserManager<AffirmyBackendUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly CouchDbService _couchDbService;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            UserManager<AffirmyBackendUser> userManager,
            IConfiguration configuration,
            CouchDbService couchDbService
        )
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _couchDbService = couchDbService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var userExists = await _userManager.FindByEmailAsync(registerModel.Email);
            if (userExists != null)
            {
                return Forbid();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var newUser = new AffirmyBackendUser()
            {
                UserName = registerModel.Email,
                Email = registerModel.Email,
            };

            var result = await _userManager.CreateAsync(newUser, registerModel.Password);
            if (!result.Succeeded)
            {
                return Problem();
            }

            var userDbName = Guid.NewGuid();

            var affirmationDbResult = await _couchDbService.CreateDatabases(userDbName + "-affirmations" );
            if (affirmationDbResult.IsSuccessStatusCode)
            {
                var scheduleDbResult = await _couchDbService.CreateDatabases(userDbName + "-schedules");
                if (!scheduleDbResult.IsSuccessStatusCode)
                {
                    return StatusCode(500);
                }
            }
            else
            {
                return StatusCode(500);
            }

            return Ok();
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    _configuration["JWT:ValidIssuer"],
                    _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized();
        } 
    }

    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
