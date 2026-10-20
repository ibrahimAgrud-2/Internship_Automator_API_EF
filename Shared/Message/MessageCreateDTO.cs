using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Message
{
    public class MessageCreateDTO
    {

 

        [Required]
        public int PersonID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        [Required]
        public bool IsStaff { get; set; }

        public MessageCreateDTO( int personID, int conversationID, string content, bool isStaff)
        {
            PersonID = personID;
            ConversationID = conversationID;
            Content = content;
            this.IsStaff = isStaff;
        }

    }
}
