using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.Business.Project;
using EF_InternApplicationAutomator.Business.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Project;

namespace EF_InternApplicationAutomator.API.Controllers.Project
{
    [Route("api/Projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectsBL _Projects;
        private readonly UserBL _User;
        private readonly OngoingProjectsBL _OngoingProjects;

        public ProjectsController(ProjectsBL projects, OngoingProjectsBL ongoingProjects,UserBL user)
        {
            _Projects = projects;
            _OngoingProjects = ongoingProjects;
            _User = user;
        }


        /// <summary>
        /// Proje listesinin hem user hem intern görebilir
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ProjectListResponseDTO>> GetAllProjects()
        {
            List<ProjectListResponseDTO> projects = _Projects.GetAllProjects();
           
            return Ok(projects);
        }


        /// <summary>
        /// User veya intern Find yapabilir. ProjectListResponseDTO web için gereken en uygun DTO olduğu için bunu kullanıtyou<
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{ID}", Name = "FindProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProjectResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"ID geçersiz ID {ID}");
            }

            ProjectResponseDTO? project = _Projects.Find(ID);
            if (project == null)
            {
                return NotFound($"Verilen ID'de proje bulunmado -  {ID}");
            }

            return Ok(project);
        }


        /// <summary>
        /// sadece user Update yapabilir. Update yaparken UpdateByUserID gibi bilgi gerek yok
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="projectDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [HttpPut("Update/{ID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<bool> UpdateProject(int ID, ProjectUpdateDTO projectDTO)
        {
            if ( projectDTO == null||ID < 1||string.IsNullOrEmpty(projectDTO.title)|| string.IsNullOrEmpty(projectDTO.description))
            {
                return BadRequest("Invalid Project data.");
            }
            int? userID = User.GetUserID();
            if (userID==null)
            {
                return Unauthorized();
            }
            if(!_User.IsUserActive(userID.Value))
            {
                return Forbid();
            }
            if (!_Projects.IsProjectExists(ID))
            {
                return NotFound($"No Project With ID {ID}");
            }

            if (_Projects.UpdateProject(ID, projectDTO))
            {
                return Ok(true);
            }

            return BadRequest("Could Not Update Project");
        }


        /// <summary>
        /// Add işlemi sadeec User tarafında yapılabilir.
        /// </summary>
        /// <param name="newProject"></param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ProjectListResponseDTO> AddNewProject(ProjectCreateDTO newProject)
        {
            if (newProject == null)
            {
                return BadRequest("Invalid Project data.");
            }
            int? userID = User.GetUserID();
            if (userID == null)
            {
                return Unauthorized();
            }
            if (!_User.IsUserActive(userID.Value))
            {
                return Forbid();
            }
            int projectID = _Projects.AddNewProject(newProject,userID.Value);
            if (projectID < 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var createdProject = new ProjectResponseDTO(
                projectID,
                newProject.Title, newProject.Description,
                newProject.Technologies);

            return CreatedAtRoute("FindProject", new { ID = projectID }, createdProject);
        }


        /// <summary>
        /// delete sadece user tarafında yapılablir
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [Authorize(Roles ="User")]
        [HttpDelete("Delete/{ID}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult DeleteProject(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not Accepted ID {ID}");
            }

            if (!_Projects.IsProjectExists(ID))
            {
                return NotFound($"No Project With ID {ID}");
            }
            if(_OngoingProjects.GetAssignedInternCountForProject(ID)>0)
            {
                return    BadRequest($"Bu proje stajyerlere atanmış olduğu için silinemez.");
            }
            if (_Projects.DeleteProject(ID))
            {
                return NoContent();
            }

            return    StatusCode(StatusCodes.Status500InternalServerError);
        }



        [Authorize(Roles = "User")]
        [HttpGet("exists/{projectID}")]
        public bool IsProjectExists(int projectID)
        {
            return projectID > 0 && _Projects.IsProjectExists(projectID);
        }

        [Authorize(Roles = "User")]
        [HttpGet("AssignedInternCount")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<int> AssignedInternCount(int projectID)
        {
            if(!_Projects.IsProjectExists(projectID))
            {
                return NotFound($"Project with ID {projectID} does not exists");
            }
            return Ok(_OngoingProjects.GetAssignedInternCountForProject(projectID));
        }
   
    
    }
}
