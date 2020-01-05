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
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository r_Repo;
        private readonly IMapper r_Mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            r_Mapper = mapper;
            r_Repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            var messagesFromRepo = await r_Repo.GetMessage(id);

            if (messagesFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messagesFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery] MessagesParams messagesParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            messagesParams.UserId = userId;

            var messagesFromRepo = await r_Repo.GetMessagesForUser(messagesParams);

            var messages = r_Mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize,
             messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            var messagesFromRepo = await r_Repo.GetMessageThread(userId, recipientId);

            var messageThread = r_Mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messageThread);
        }


        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = userId;

            var recipient = await r_Repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
            {
                return BadRequest("Could not find user");
            }

            var message = r_Mapper.Map<Message>(messageForCreationDto);

            r_Repo.Add(message);

            if (await r_Repo.SaveAll())
            {
                var messageToReturn = r_Mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { userId, id = message.Id }, messageToReturn);
            }

            throw new System.Exception("Creating the message failed on save");
        }

    }
}