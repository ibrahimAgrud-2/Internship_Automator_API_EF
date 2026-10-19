using EF_InternApplicationAutomator.DataAccess.Conversation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Messages
{
    [Table("Messages")]
    public class MessageEntity
    {
        [Key]
        public int MessageID { get; set; }

        [Required]
        public int PersonID { get; set; }


        [Required]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        public MessageEntity(int messageID, int personID, int conversationID, string content, DateTime createdDate)
        {
            MessageID = messageID;
            PersonID = personID;
            ConversationID = conversationID;
            Content = content;
            CreatedDate = createdDate;
        }
    }
}
