using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.Business.Project;
using Microsoft.AspNetCore.Mvc;
using Shared.Project;

namespace EF_InternApplicationAutomator.API.Controllers.Project
{
    [Route("api/Projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectsBL _Projects;
        private readonly OngoingProjectsBL _OngoingProjects;

        public ProjectsController(ProjectsBL projects, OngoingProjectsBL ongoingProjects)
        {
            _Projects = projects;
            _OngoingProjects = ongoingProjects;
        }

        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProjectResponseDTO>> GetAllProjects()
        {
            List<ProjectResponseDTO> projects = _Projects.GetAllProjects();
            if (projects.Count == 0)
            {
                return NotFound("No Data Available in the Table");
            }

            return Ok(projects);
        }

        [HttpGet("{ID}", Name = "FindProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProjectResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not Accepted ID {ID}");
            }

            ProjectResponseDTO? project = _Projects.Find(ID);
            if (project == null)
            {
                return NotFound($"No Project With ID {ID}");
            }

            return Ok(project);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProjectResponseDTO> AddNewProject(ProjectCreateDTO newProject)
        {
            if (newProject == null)
            {
                return BadRequest("Invalid Project data.");
            }

            int projectID = _Projects.AddProject(newProject);
            if (projectID < 1)
            {
                return BadRequest("Could Not Add Project");
            }

            var createdProject = new ProjectResponseDTO(
                projectID,
                newProject.CreatedByUserID,
                newProject.Title,newProject.Description,
                newProject.Technologies);

            return CreatedAtRoute("FindProject", new { ID = projectID }, createdProject);
        }

        [HttpPut("{id}", Name = "UpdateProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProjectResponseDTO> UpdateProject(int id, ProjectCreateDTO projectDTO)
        {
            if (id < 1 || projectDTO == null)
            {
                return BadRequest("Invalid Project data.");
            }

            if (!_Projects.IsProjectExists(id))
            {
                return NotFound($"No Project With ID {id}");
            }

            if (_Projects.UpdateProject(id, projectDTO) > 0)
            {
                return Ok(new ProjectResponseDTO(
                    id,
                    projectDTO.CreatedByUserID,
                    projectDTO.Title,projectDTO.Description,
                    projectDTO.Technologies));
            }

            return BadRequest("Could Not Update Project");
        }

        [HttpDelete("{id}", Name = "DeleteProject")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteProject(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ID {id}");
            }

            if (!_Projects.IsProjectExists(id))
            {
                return NotFound($"No Project With ID {id}");
            }

            if (_Projects.DeleteProject(id))
            {
                return NoContent();
            }

            return NotFound("Delete Process terminated. No rows deleted!");
        }

        [HttpGet("exists/{projectID}")]
        public bool IsProjectExists(int projectID)
        {
            return projectID > 0 && _Projects.IsProjectExists(projectID);
        }


        [HttpGet("AssignedInternCount")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<int> AssignedInternCount(int projectID)
        {
            if(!_Projects.IsProjectExists(projectID))
            {
                return BadRequest($"Project with ID {projectID} does not exists");
            }
            return Ok(_OngoingProjects.GetAssignedInternCountForProject(projectID));
        }
    }
}
