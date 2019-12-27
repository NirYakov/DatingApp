using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository r_Repo;
        private readonly IMapper r_Mapper;
        private readonly IOptions<CloudinarySettings> r_CloudinaryConfig;

        public readonly Cloudinary r_Cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this.r_Repo = repo;
            this.r_Mapper = mapper;
            this.r_CloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                r_CloudinaryConfig.Value.CloudName,
                r_CloudinaryConfig.Value.ApiKey,
                r_CloudinaryConfig.Value.ApiSecret
            );

            r_Cloudinary = new Cloudinary(acc);

        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await r_Repo.GetPhoto(id);

            var photo = r_Mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,
        // [FromForm]
         PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            var userFromRepo = await r_Repo.GetUser(userId); // end [0]

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                const int boxSizeLen = 500;

                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(boxSizeLen).Height(boxSizeLen).Crop("fill").Gravity("face")
                    };

                    uploadResult = r_Cloudinary.Upload(uploadParams);
                }
            }
            // string strUrl  = uploadResult.Uri.ToString();

            photoForCreationDto.Url = uploadResult.Uri.ToString(); // BM : Bug??
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = r_Mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(pphoto => pphoto.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await r_Repo.SaveAll())
            {
                var photoToReturn = r_Mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
                // return Ok();
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            var user = await r_Repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photoFromRepo = await r_Repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("This is already the main photo");
            }

            var currentMainPhoto = await r_Repo.GetMainPhotoForUser(userId);

            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await r_Repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // BM start [0] : the same block as in the second
            {
                return Unauthorized();
            }

            var user = await r_Repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photoFromRepo = await r_Repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("You cannot delete your main photo");
            }

            if (photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = r_Cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    r_Repo.Delete(photoFromRepo);
                }
            }

            if (photoFromRepo.PublicId == null)
            {
                r_Repo.Delete(photoFromRepo);
            }

            if (await r_Repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to delete the photo");

        }

    }
}