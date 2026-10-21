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


        [ForeignKey(nameof(PersonID))]
        public PersonEntity Person { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsStaff { get; set; }

        [Required]
        public int? ReferencedContentID { get; set; }

        [Required]
        public byte? ReferencedContentType { get; set; }

        public MessageEntity(int messageID, int personID, int conversationID, string content, DateTime createdDate,bool isStaff, int? referencedContentID, byte? ReferencedContentType)
        {
            MessageID = messageID;
            PersonID = personID;
            ConversationID = conversationID;
            Content = content;
            CreatedDate = createdDate;
            this.IsStaff = isStaff;
            this.ReferencedContentID = referencedContentID;
            this.ReferencedContentType = ReferencedContentType;
        }
    }
}
