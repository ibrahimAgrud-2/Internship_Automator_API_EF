using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Message
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

        [Required]
        public bool IsStaff { get; set; }

    
        public int? ReferencedContentID { get; set; }

        public short? ReferencedContentType { get; set; }

        public MessageEntity(int messageID, int personID, int conversationID, string content, DateTime createdDate, bool isStaff, int? referencedContentID, short? referencedContentType)
        {
            MessageID = messageID;
            PersonID = personID;
            ConversationID = conversationID;
            Content = content;
            CreatedDate = createdDate;
            IsStaff = isStaff;
            ReferencedContentID = referencedContentID;
            ReferencedContentType = referencedContentType;
        }
    }
}