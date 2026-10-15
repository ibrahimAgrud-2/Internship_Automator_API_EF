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
        public ActionResult<IEnumerable<ProjectListResponseDTO>> GetAllProjects()
        {
            List<ProjectListResponseDTO> projects = _Projects.GetAllProjects();
           
            return Ok(projects);
        }

    
        
        [HttpGet("{ID}", Name = "FindProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProjectListResponseDTO> Find(int ID)
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
        
        //[HttpPut("{ID}", Name = "UpdateProject")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult<ProjectListResponseDTO> UpdateProject(int ID, ProjectCreateDTO projectDTO)
        //{
        //    if (ID < 1 || projectDTO == null)
        //    {
        //        return BadRequest("Invalid Project data.");
        //    }

        //    if (!_Projects.IsProjectExists(ID))
        //    {
        //        return NotFound($"No Project With ID {ID}");
        //    }

        //    if (_Projects.UpdateProject(ID, projectDTO) > 0)
        //    {
        //        return Ok(new ProjectListResponseDTO(
        //            ID,
        //            projectDTO.CreatedByUserID,
        //            projectDTO.Title,projectDTO.Description,
        //            projectDTO.Technologies));
        //    }

        //    return BadRequest("Could Not Update Project");
        //}



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProjectListResponseDTO> AddNewProject(ProjectCreateDTO newProject)
        {
            if (newProject == null)
            {
                return BadRequest("Invalid Project data.");
            }

            int projectID = _Projects.AddNewProject(newProject);
            if (projectID < 1)
            {
                return BadRequest("Could Not Add Project");
            }

            var createdProject = new ProjectResponseDTO(
                projectID,
                newProject.Title, newProject.Description,
                newProject.Technologies);

            return CreatedAtRoute("FindProject", new { ID = projectID }, createdProject);
        }

        [HttpDelete("Delete/{ID}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                return    BadRequest($"Proje stajyer tarafında yapıldığı için silinemez");
            }
            if (_Projects.DeleteProject(ID))
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
