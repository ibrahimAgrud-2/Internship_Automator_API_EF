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
        [Required]
        public int? ReferencedContentID { get; set; }

        [Required]
        public byte? ReferencedContentType { get; set; }
        public MessageCreateDTO( int personID, int conversationID, string content, bool isStaff, int? referencedContentID, byte? ReferencedContentType)
        {
            PersonID = personID;
            ConversationID = conversationID;
            Content = content;
            this.IsStaff = isStaff;
            this.ReferencedContentID = referencedContentID;
            this.ReferencedContentType = ReferencedContentType;
        }

    }
}
