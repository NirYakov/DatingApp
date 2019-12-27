using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository r_Repo;
        private readonly IMapper r_Mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            r_Repo = repo;
            r_Mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await r_Repo.GetUsers();

            var usersToReturn = r_Mapper.Map<IEnumerable<UserForListDto>>(users);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await r_Repo.GetUser(id);

            var userToReturn = r_Mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            var userFromRepo = await r_Repo.GetUser(id); // end [0]

            r_Mapper.Map(userForUpdateDto, userFromRepo);

            if (await r_Repo.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Updaing user {id} failed on save");
        }

    }
}