using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Message
{
    public class MessageResponseDTO
    {
        
    
            public int MessageID { get; set; }

            [Required]
            public int PersonID { get; set; }

        [Required]
        public string PersonFullName { get; set; }

        [Required]
            public int ConversationID { get; set; }

            [Required]
            [MaxLength(500)]
            public string Content { get; set; }

            [Required]
            public DateTime CreatedDate { get; set; }

                [Required]
                public bool IsStaff { get; set; }

        public MessageResponseDTO(int messageID, int personID, int conversationID, string content, DateTime createdDate,bool IsUser,string personFullName)
            {
                MessageID = messageID;
                PersonID = personID;
                ConversationID = conversationID;
                Content = content;
                CreatedDate = createdDate;
               this.IsStaff = IsUser;
            this.PersonFullName = personFullName;
            }
        
    }
}
