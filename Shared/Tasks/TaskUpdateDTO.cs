using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Tasks
{
    public class TaskUpdateDTO
    {

        [MaxLength(100)]
        [Required]
        public string Title { get; set; }

        [MaxLength(200)]
        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public TaskUpdateDTO(string title, string description,  DateTime dueDate)
        {
          
            Title = title;
            DueDate = dueDate;
            this.Description = description;
        }
    }
}
