using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.Business.Project;
using Microsoft.AspNetCore.Mvc;
using Shared.OngoingProjects;
using Shared.Tasks;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.API.Controllers.OngoingProjects
{
    [Route("api/OngoingProjects")]
    [ApiController]
    public class OngoingProjectsController : ControllerBase
    {
        private readonly OngoingProjectsBL _OngoingProjects;
        private readonly Business.Intern.InternBL _Intern;
        private readonly Business.Project.ProjectsBL _Project;

        public OngoingProjectsController(OngoingProjectsBL ongoingProjects, InternBL intern, ProjectsBL project)
        {
            _OngoingProjects = ongoingProjects;
            _Intern = intern;
            _Project = project ;
        }

        [HttpGet("InternProjects")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<OngoingInternProjectResponseDTO>> GetAllInternProjects(int internID)
        {
            if (internID <= 0)
            {
                return BadRequest($"Intern ID {internID} Is Not Valid");
            }
            if (!_Intern.IsInternExists(internID))
            {
                return BadRequest("Such intern Does not exist. ");
            }
            List<OngoingInternProjectResponseDTO> tasks = _OngoingProjects.GetInternOngoingProjects(internID);

            return Ok(tasks);
        }




        [HttpGet("{id}", Name = "FindOngoingProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OngoingProjectResponseDTO> Find(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ID {id}");
            }

            OngoingProjectResponseDTO? ongoingProject = _OngoingProjects.Find(id);
            if (ongoingProject == null)
            {
                return NotFound($"No Ongoing Project With ID {id}");
            }

            return Ok(ongoingProject);
        }

      
        
        [HttpPost("Add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<OngoingProjectResponseDTO> AddNewOngoingProject(
            OngoingProjectCreateDTO ongoingProjectDTO)
        {
            if (ongoingProjectDTO.DueDate < DateTime.Now.Date || ongoingProjectDTO == null)
            {
                return BadRequest("Girilen veri formatı yanlış");
            }

            if (!_Intern.IsInternExists(ongoingProjectDTO.InternID) || !_Intern.IsInternActive(ongoingProjectDTO.InternID))
            {
                return BadRequest($"Ayrıla veya olmayan Stajyere proje atanamaz");
            }
            if (!_Project.IsProjectExists(ongoingProjectDTO.ProjectID))
            {
                return BadRequest($"Atamak istediğiniz proje kayıtlı değil. Önce projeyi kayıt edin");
            }
            if (_Intern.IsProjectAssignedToIntern(ongoingProjectDTO.ProjectID, ongoingProjectDTO.InternID))
            {
                return BadRequest($"Stajyer zaten bu proje üzerinde çalışıyor.");
            }
            int insertedID = _OngoingProjects.AddOngoingProject(ongoingProjectDTO);

            if(insertedID>0)
            {
                return CreatedAtRoute(
                   "FindOngoingProject",
                   new { id = insertedID }, new OngoingProjectResponseDTO(
                                       insertedID,
                       ongoingProjectDTO.InternID,
                       ongoingProjectDTO.ProjectID,
                       DateTime.Now.Date,
                       ongoingProjectDTO.DueDate));
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }



        [HttpPut("Update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OngoingProjectResponseDTO> UpdateOngoingProject( int id,  OngoingProjectCreateDTO ongoingProjectDTO)
        {
            if (id < 1||ongoingProjectDTO.DueDate<DateTime.Now.Date|| ongoingProjectDTO == null)
            {
                return BadRequest("Girilen veri formatı yanlış");
            }
          
            if (!_Intern.IsInternExists(ongoingProjectDTO.InternID) ||!_Intern.IsInternActive(ongoingProjectDTO.InternID))
            {
                return BadRequest($"Ayrıla veya olmayan Stajyere proje atanamaz");
            }
            if(!_Project.IsProjectExists(ongoingProjectDTO.ProjectID))
            {
                return BadRequest($"Atamak istediğiniz proje kayıtlı değil. Önce projeyi kayıt edin");
            }
            if (_OngoingProjects.UpdateOngoingProject(id, ongoingProjectDTO) > 0)
            {
                return Ok(new OngoingProjectResponseDTO(
                    id,
                    ongoingProjectDTO.InternID,
                    ongoingProjectDTO.ProjectID,
                    DateTime.Now.Date,
                    ongoingProjectDTO.DueDate));
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("{id}", Name = "DeleteOngoingProject")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteOngoingProject(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ID {id}");
            }

            if (!_OngoingProjects.IsOngoingProjectExists(id))
            {
                return NotFound($"No Ongoing Project With ID {id}");
            }

            if (_OngoingProjects.DeleteOngoingProject(id))
            {
                return NoContent();
            }

            return NotFound("Delete Process terminated. No rows deleted!");
        }

        [HttpGet("exists/{ongoingProjectID}")]
        public bool IsOngoingProjectExists(int ongoingProjectID)
        {
            return ongoingProjectID > 0 && _OngoingProjects.IsOngoingProjectExists(ongoingProjectID);
        }


    }
}
