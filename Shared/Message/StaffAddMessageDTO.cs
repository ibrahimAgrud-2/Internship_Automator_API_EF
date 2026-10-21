using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Message
{
    public class StaffAddMessageDTO
    {




        [Required]
        public int UserID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        [Required]
        public int? ReferencedContentID { get; set; }

        [Required]
        public byte? ReferencedContentType { get; set; }
        public StaffAddMessageDTO(int userID, int conversationID, string content,int? referencedContentID, byte? ReferencedContentType)
        {
            this.UserID = userID;
            ConversationID = conversationID;
            Content = content;
            this.ReferencedContentID = referencedContentID;
            this.ReferencedContentType = ReferencedContentType;
        }
    }
}
