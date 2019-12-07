using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository r_Repo;

        public AuthController(IAuthRepository repo)
        {
            r_Repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            // BM validate request
            username = username.ToLower();

            if (await r_Repo.UserExists(username))
            {
                return BadRequest("Usename already exists");
            }

            var userToCreate = new User
            {
                Username = username
            };

            var createdUser = await r_Repo.Register(userToCreate, password);

            return StatusCode(201);
        }

    }
}