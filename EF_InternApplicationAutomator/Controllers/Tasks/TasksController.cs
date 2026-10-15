using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.Business.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.OngoingProjects;
using Shared.Tasks;

namespace EF_InternApplicationAutomator.API.Controllers.Tasks
{
    [Route("api/Tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TasksBL _Tasks;
        private readonly Business.Intern.InternBL _Intern;
        private readonly Business.OngoingProjects.OngoingProjectsBL _OngoingProjects;

        public TasksController(TasksBL tasks, InternBL intern, OngoingProjectsBL ongoingProjects)
        {
            _Tasks = tasks;
            _Intern = intern;
            _OngoingProjects = ongoingProjects;
        }

        [HttpGet("AllInternTasks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TaskResponseDTO>> GetAllInternTasks(int internID)
        {
            if(internID<=0)
            {
                return BadRequest($"Intern ID {internID} Is Not Valid");
            }
            if(!_Intern.IsInternExists(internID))
            {
                return BadRequest("Such intern Does not exist. ");
            }
            List<TaskResponseDTO> tasks = _Tasks.GetAllTasks(internID);
            return Ok(tasks);
        }


        [HttpPost("Add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TaskResponseDTO> AddNewTask(TaskCreateDTO newTask)
        {

            if (newTask == null|| newTask.DueDate<DateTime.Now)
            {
                return BadRequest("Invalid Task data.");
            }
            if(!_OngoingProjects.IsOngoingProjectExists(newTask.OngoingProjectID))
            {
                return BadRequest("Bu devam projeye görev atanamaz.");
            }
             int taskID = _Tasks.AddTask(newTask);
            if (taskID < 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            TaskResponseDTO insertedTask = _Tasks.Find(taskID);
            if(insertedTask==null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var createdTask = new TaskResponseDTO(taskID, insertedTask.CreatedByUserID, insertedTask.OngoingProjectID, insertedTask.Title, insertedTask.Description, insertedTask.AssignedDate, insertedTask.DueDate, insertedTask.Completed, insertedTask.CompletedDate);

            return CreatedAtRoute("FindTask", new { id = taskID }, createdTask);
        }


        [HttpGet("FindTask/{id}",Name = "FindTask")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TaskResponseDTO> Find(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ID {id}");
            }

            TaskResponseDTO? task = _Tasks.Find(id);
            if (task == null)
            {
                return NotFound($"No Task With ID {id}");
            }

            return Ok(task);
        }



        //[HttpPut("{id}", Name = "UpdateTask")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult<TaskResponseDTO> UpdateTask(int id, TaskCreateDTO taskDTO)
        //{
        //    if (id < 1 || taskDTO == null)
        //    {
        //        return BadRequest("Invalid Task data.");
        //    }

        //    if (!_Tasks.IsTaskExists(id))
        //    {
        //        return NotFound($"No Task With ID {id}");
        //    }

        //    if (_Tasks.UpdateTask(id, taskDTO) > 0)
        //    {
        //        return Ok(new TaskResponseDTO(
        //            id,
        //            taskDTO.CreatedByUserID,
        //            taskDTO.OngoingProjectID,
        //            taskDTO.title,
        //            taskDTO.CreatedAt,
        //            taskDTO.DueDate));
        //    }

        //    return BadRequest("Could Not Update Task");
        //}

        //[HttpDelete("{id}", Name = "DeleteTask")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult DeleteTask(int id)
        //{
        //    if (id < 1)
        //    {
        //        return BadRequest($"Not Accepted ID {id}");
        //    }

        //    if (!_Tasks.IsTaskExists(id))
        //    {
        //        return NotFound($"No Task With ID {id}");
        //    }

        //    if (_Tasks.DeleteTask(id))
        //    {
        //        return NoContent();
        //    }

        //    return NotFound("Delete Process terminated. No rows deleted!");
        //}

        //[HttpGet("exists/{taskID}")]
        //public bool IsTaskExists(int taskID)
        //{
        //    return taskID > 0 && _Tasks.IsTaskExists(taskID);
        //}



    }
}
