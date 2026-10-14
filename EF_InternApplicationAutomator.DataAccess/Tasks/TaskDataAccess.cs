using Microsoft.EntityFrameworkCore;
using Shared.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Tasks
{
    public class TaskDataAccess
    {
        private readonly IAADbContext _Context;

        public TaskDataAccess(IAADbContext context)
        {
            _Context = context;
        }

        public List<TaskResponseDTO> GetAllInternTasks(int internID)
        {
            List<TaskEntity> tasks = _Context.Tasks.Include(t => t.OngoingProject).Where(t => t.OngoingProject.InternID == internID).ToList();
            List<TaskResponseDTO> taskDTOs = new List<TaskResponseDTO>();

            foreach (var task in tasks)
            {
                taskDTOs.Add(ToResponseDTO(task));
            }

            return taskDTOs;
        }

        //public TaskResponseDTO? Find(int taskID)
        //{
        //    TaskEntity? task = _Context.Tasks.Find(taskID);
        //    return task == null ? null : ToResponseDTO(task);
        //}

        //public int AddTask(TaskCreateDTO task)
        //{
        //    TaskEntity taskEntity = new TaskEntity(
        //        0,
        //        task.CreatedByUserID,
        //        task.OngoingProjectID,
        //        task.Title,
        //        task.CreatedAt,
        //        task.DueDate);
        //    _Context.Tasks.Add(taskEntity);

        //    if (_Context.SaveChanges() > 0)
        //    {
        //        return taskEntity.TaskID;
        //    }

        //    return -1;
        //}

        //public int UpdateTask(int taskID, TaskCreateDTO taskDTO)
        //{
        //    TaskEntity? task = _Context.Tasks.Find(taskID);
        //    if (task == null || taskDTO == null)
        //    {
        //        return -1;
        //    }

        //    task.CreatedByUserID = taskDTO.CreatedByUserID;
        //    task.OngoingProjectID = taskDTO.OngoingProjectID;
        //    task.Title = taskDTO.Title;
        //    task.CreatedAt = taskDTO.CreatedAt;
        //    task.DueDate = taskDTO.DueDate;

        //    if (_Context.SaveChanges() > 0)
        //    {
        //        return taskID;
        //    }

        //    return -1;
        //}

        //public bool DeleteTask(int taskID)
        //{
        //    return _Context.Tasks
        //        .Where(task => task.TaskID == taskID)
        //        .ExecuteDelete() > 0;
        //}

        //public bool IsTaskExists(int taskID)
        //{
        //    return _Context.Tasks.Any(task => task.TaskID == taskID);
        //}

        private static TaskResponseDTO ToResponseDTO(TaskEntity task)
        {
            return new TaskResponseDTO(task.TaskID,task.CreatedByUserID,task.OngoingProjectID,task.Title,task.Description,task.AssignedDate,task.DueDate,task.Completed,task.CompletedDate);
        }
    }
}
