using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.OngoingProjects;
using EF_InternApplicationAutomator.Business.Tasks;
using EF_InternApplicationAutomator.Business.User;
using Microsoft.AspNetCore.Authorization;
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
        private readonly Business.User.UserBL _User;
        private readonly Business.OngoingProjects.OngoingProjectsBL _OngoingProjects;

        public TasksController(TasksBL tasks, InternBL intern, OngoingProjectsBL ongoingProjects, UserBL user)
        {
            _Tasks = tasks;
            _Intern = intern;
            _OngoingProjects = ongoingProjects;
            _User = user;
        }

        [Authorize(Roles = "Intern")]
        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TaskResponseDTO>> GetAll()
        {

            int? internID = User.GetInternID();

            if ((internID ?? 0) <= 0)
            {
                return Unauthorized($"{internID} Geçerli bir ID değil");
            }
            if (!_Intern.IsInternExists(internID.Value))
            {
                return NotFound($"Verilen ID'de ({internID}) Stajyer bulunmamaktadır");
            }
            List<TaskResponseDTO> tasks = _Tasks.GetAllTasks(internID.Value);
            return Ok(tasks);
        }

        [Authorize(Roles = "User")]
        [HttpGet("AllInternTasks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TaskResponseDTO>> GetAllInternTasks(int internID)
        {


            if (internID <= 0)
            {
                return Unauthorized();
            }
            if (!_Intern.IsInternExists(internID))
            {
                return NotFound($"Verilen ID'de ({internID}) Stajyer bulunmamaktadır");
            }
            List<TaskResponseDTO> tasks = _Tasks.GetAllTasks(internID);
            return Ok(tasks);
        }


        [Authorize(Roles = "User")]
        [HttpPost("Add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TaskResponseDTO> AddNewTask(TaskCreateDTO newTask)
        {

            if (newTask == null || newTask.DueDate < DateTime.Now)
            {
                return BadRequest("Girilen Task verileri geçersiz");
            }
            if (!_OngoingProjects.IsOngoingProjectExists(newTask.OngoingProjectID))
            {
                return BadRequest("Bu devam projeye görev atanamaz.");
            }
            int? createdByUserID = User.GetUserID();
            if (createdByUserID == null)
            {
                return Unauthorized();
            }
            if (!_User.IsUserExists(createdByUserID.Value))
            {
                return BadRequest("Kullanici Hesabınız bulunamadığı için görev eklenemedi");
            }
            newTask.CreatedByUserID = createdByUserID.Value;
            int taskID = _Tasks.AddTask(newTask);
            if (taskID < 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            TaskResponseDTO insertedTask = _Tasks.Find(taskID);
            if (insertedTask == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            //TODO: daha iyi bir yol kullanılabilir (sonra)
            var createdTask = new TaskResponseDTO(taskID, insertedTask.CreatedByUserID, insertedTask.OngoingProjectID, insertedTask.Title, insertedTask.Description, insertedTask.AssignedDate, insertedTask.DueDate, insertedTask.Completed, insertedTask.CompletedDate);

            return CreatedAtRoute("FindTask", new { id = taskID }, createdTask);
        }





        [Authorize(Roles = "User")]
        [HttpGet("FindTask/{id}", Name = "FindTask")]
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


        [Authorize(Roles = "User")]
        [HttpPut("Complete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<bool> CompleteTask(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Güncelleme için verilen ID geçerli değil");
            }

            TaskResponseDTO? task = _Tasks.Find(id);
            if (task == null)
            {
                return NotFound($"Görev bulunamadı");
            }
            if (task.Completed)
            {
                return BadRequest("Görev zaten tamamlanmış");
            }
            int? userID = User.GetUserID();
            if(userID==null)
            {
                return Unauthorized();
            }
            if (_Tasks.CompleteTask(id, userID.Value))
            {
                return Ok(true);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


        [Authorize(Roles = "User")]
        [HttpDelete("Delete/{ID}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteTask(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Verilen ID geçersiz");
            }

            if (!_Tasks.IsTaskExists(ID))
            {
                return NotFound($"Görev Bulunamadi");
            }
            if (_Tasks.DeleteTask(ID))
            {
                return NoContent();
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


        [Authorize(Roles ="User")]
        [HttpPut("Update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> UpdateTask(int id, TaskUpdateDTO taskDTO)
        {
            if (id < 1 || taskDTO == null)
            {
                return BadRequest("Görev ID'si geçersiz");
            }

            if (!_Tasks.IsTaskExists(id))
            {
                return NotFound($"task bulunamadı");
            }

            int? updatedByUserID = User.GetUserID();
            if(updatedByUserID==null)
            {
                return Unauthorized();
            }
            if (!_User.IsUserExists(updatedByUserID.Value))
            {
                return BadRequest("Hesabınıza erişilemediği için Görev güncellenemedi!");
            }
            taskDTO.LastUpdatedByUserID = updatedByUserID.Value;
            if (_Tasks.UpdateTask(id, taskDTO))
            {
                return Ok(true);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


    }
}
