using EF_InternApplicationAutomator.DataAccess.Tasks;
using Shared.Tasks;

namespace EF_InternApplicationAutomator.Business.Tasks
{
    public class TasksBL
    {
        private readonly TaskDataAccess _TaskDataAccess;

        public TasksBL(TaskDataAccess taskDataAccess)
        {
            _TaskDataAccess = taskDataAccess;
        }

        public List<TaskResponseDTO> GetAllTasks(int internID)
        {
            return _TaskDataAccess.GetAllInternTasks(internID);
        }

        public int AddTask(TaskCreateDTO newTask)
        {
            return _TaskDataAccess.AddTask(newTask);
        }

        public TaskResponseDTO? Find(int taskID)
        {
            return _TaskDataAccess.Find(taskID);
        }

        //public int UpdateTask(int taskID, TaskCreateDTO taskCreateDTO)
        //{
        //    return _TaskDataAccess.UpdateTask(taskID, taskCreateDTO);
        //}



        //public bool DeleteTask(int taskID)
        //{
        //    return _TaskDataAccess.DeleteTask(taskID);
        //}

        //public bool IsTaskExists(int taskID)
        //{
        //    return _TaskDataAccess.IsTaskExists(taskID);
        //}
    }
}
