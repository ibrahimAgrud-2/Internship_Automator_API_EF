using EF_InternApplicationAutomator.Business.Announcements;
using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Announcement;
using Shared.Application;
using Shared.OngoingProjects;
using Shared.Tasks;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using EF_InternApplicationAutomator.Business.User;

namespace EF_InternApplicationAutomator.API.Controllers.Announcements
{

    [Route("api/Announcement")]
    [ApiController]
  
    public class AnnouncementsController:ControllerBase
    {

        private readonly AnnouncementsBL _AnouncementsBL;

        private readonly UserBL _User;


        public AnnouncementsController(AnnouncementsBL anouncementsBL,UserBL user)
        {
            _AnouncementsBL = anouncementsBL;
            _User = user;
        }

        [Authorize]
        [HttpGet("All/{topNewest=15}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<AnnouncementResponseDTO>> GetAll(int topNewest=15)
        {
            List<AnnouncementResponseDTO> anns = _AnouncementsBL.GetAll(topNewest);
            return Ok(anns);
        }



        [Authorize]
        [HttpGet("Find/{id}", Name= "FindAnnouncement")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AnnouncementResponseDTO> Find(int id)
        {
            if (id < 1)
            {
                return BadRequest($"{id} - ID Geçeli değil");
            }

            AnnouncementResponseDTO? ann = _AnouncementsBL.Find(id);
            if (ann == null)
            {
                return NotFound($"{id} ID'de duyuru bulunmamaktadır.");
            }

            return Ok(ann);
        }





        [Authorize(Roles = "User")]
        [HttpPost("Add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AnnouncementResponseDTO> AddNewAnnouncement(AnnouncementCreateDTO announcementCreateDTO)
        {
            if (announcementCreateDTO == null)
            {
                return BadRequest("Girilen veri formatı yanlış");
            }

            // Kimlik token'dan alınır, istemcinin gönderdiği değer yok sayılır
            int? userID = User.GetUserID();
            if (userID == null)
            {
                return Unauthorized();
            }
            if(!_User.IsUserExists(userID.Value))
            {
                return BadRequest("Hesabınız bulunamadığı için duyuru yapamazsınız");
            }
           
          
            int insertedID = _AnouncementsBL.AddAnnouncement(announcementCreateDTO, userID.Value);

            if (insertedID > 0)
            {
                return CreatedAtRoute(
                   "FindAnnouncement",
                   new { id = insertedID }, new AnnouncementResponseDTO(insertedID,
                                     announcementCreateDTO.Title, announcementCreateDTO.Content, DateTime.Now, userID.Value));
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }




        [Authorize(Roles = "User")]
        [HttpPut("Update/{ID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<bool> UpdateAnnouncement(int ID,AnnouncementUpdateDTO updatedAnnouncement)
        {
            if (updatedAnnouncement == null || ID < 1)
            {
                return BadRequest("Girilen veriler Geçerli değil.");
            }
            if(!_AnouncementsBL.IsAnnouncementExists(ID))
            {
                return NotFound("Güncellemen istediğiniz duyuru bulunamadı. Silinmiş olabilir");
            }
            // Kimlik token'dan alınır, istemcinin gönderdiği değer yok sayılır
            int? userID = User.GetUserID();
            if (userID == null)
            {
                return Unauthorized();
            }
            if (!_User.IsUserExists(userID.Value))
            {
                return BadRequest("Hesabınız bulunamadığı için duyuru güncelleyemezsiniz");
            }
            if (_AnouncementsBL.UpdateAnnouncement(ID, userID.Value, updatedAnnouncement))
            {
                return Ok(true);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }




       
        [Authorize(Roles = "User")]
        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteAnnouncement(int id)
        {
            if (id < 1)
            {
                return BadRequest($"ID geçerli değil!!");
            }

       

            if (_AnouncementsBL.DeleteAnnouncement(id))

                return NoContent();
            else
                return NotFound($"Duyuru bulunamadı");
        }


    }
}
