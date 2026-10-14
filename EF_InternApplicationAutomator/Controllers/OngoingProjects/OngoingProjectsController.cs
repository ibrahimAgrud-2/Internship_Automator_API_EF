using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.OngoingProjects;
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

        public OngoingProjectsController(OngoingProjectsBL ongoingProjects, InternBL intern)
        {
            _OngoingProjects = ongoingProjects;
            _Intern = intern;
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<OngoingProjectResponseDTO> AddNewOngoingProject(
            OngoingProjectCreateDTO newOngoingProject)
        {
            if (newOngoingProject == null)
            {
                return BadRequest("Invalid Ongoing Project data.");
            }

            int ongoingProjectID = _OngoingProjects.AddOngoingProject(newOngoingProject);
            if (ongoingProjectID < 1)
            {
                return BadRequest("Could Not Add Ongoing Project");
            }

            var createdOngoingProject = new OngoingProjectResponseDTO(
                ongoingProjectID,
                newOngoingProject.InternID,
                newOngoingProject.ProjectID,
                newOngoingProject.StartDate,
                newOngoingProject.DueDate);

            return CreatedAtRoute(
                "FindOngoingProject",
                new { id = ongoingProjectID },
                createdOngoingProject);
        }

        [HttpPut("{id}", Name = "UpdateOngoingProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OngoingProjectResponseDTO> UpdateOngoingProject(
            int id,
            OngoingProjectCreateDTO ongoingProjectDTO)
        {
            if (id < 1 || ongoingProjectDTO == null)
            {
                return BadRequest("Invalid Ongoing Project data.");
            }

            if (!_OngoingProjects.IsOngoingProjectExists(id))
            {
                return NotFound($"No Ongoing Project With ID {id}");
            }

            if (_OngoingProjects.UpdateOngoingProject(id, ongoingProjectDTO) > 0)
            {
                return Ok(new OngoingProjectResponseDTO(
                    id,
                    ongoingProjectDTO.InternID,
                    ongoingProjectDTO.ProjectID,
                    ongoingProjectDTO.StartDate,
                    ongoingProjectDTO.DueDate));
            }

            return BadRequest("Could Not Update Ongoing Project");
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
