using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
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
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await r_Repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await r_Repo.GetUsers(userParams);

            var usersToReturn = r_Mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
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

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            var like = await r_Repo.GetLike(id, recipientId);

            if (like != null)
            {
                return BadRequest("You already like this user");
            }

            if (await r_Repo.GetUser(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like()
            {
                LikerId = id,
                LikeeId = recipientId
            };

            r_Repo.Add<Like>(like);

            if (await r_Repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed To like user");

        }



    }
}