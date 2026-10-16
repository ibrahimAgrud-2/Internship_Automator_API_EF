using EF_InternApplicationAutomator.Business.Announcements;
using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.Business.Project;
using Microsoft.AspNetCore.Mvc;
using Shared.Announcement;
using Shared.OngoingProjects;
using Shared.Tasks;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.API.Controllers.Announcements
{

    [Route("api/Announcement")]
    [ApiController]
    public class AnnouncementsController:ControllerBase
    {

        private readonly AnnouncementsBL _AnouncementsBL;


        public AnnouncementsController(AnnouncementsBL anouncementsBL)
        {
            _AnouncementsBL = anouncementsBL;
           
        }


        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<AnnouncementResponseDTO>> GetAll()
        {
          
           
            List<AnnouncementResponseDTO> anns = _AnouncementsBL.GetAll();

            return Ok(anns);
        }


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






        [HttpPost("Add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AnnouncementResponseDTO> AddNewAnnouncement( AnnouncementCreateDTO announcementCreateDTO)
        {
            if (announcementCreateDTO == null)
            {
                return BadRequest("Girilen veri formatı yanlış");
            }

            int insertedID = _AnouncementsBL.AddAnnouncement(announcementCreateDTO);

            if (insertedID > 0)
            {
                return CreatedAtRoute(
                   "FindAnnouncement",
                   new { id = insertedID }, new AnnouncementResponseDTO(
                                     announcementCreateDTO.Title, announcementCreateDTO.Content, announcementCreateDTO.CreatedDate, announcementCreateDTO.CreatedByUserID));
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}
