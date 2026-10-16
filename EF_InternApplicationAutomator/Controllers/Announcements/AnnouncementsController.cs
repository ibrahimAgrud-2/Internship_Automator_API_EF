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



    }
}
