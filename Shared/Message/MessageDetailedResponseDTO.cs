using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Message 
{
    public class MessageDetailedResponseDTO
    {
        [Required]
        public int MessageID { get; set; }

        [Required]
        public string personFullName { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsStaff { get; set; }

        [Required]
        public int ReferencedContentID { get; set; }

        [Required]
        public short ReferencedContentType { get; set; }

        public string? ReferencedContentTitle { get; set; }

        public MessageDetailedResponseDTO(
            int messageID,
           string personFullName,
            int conversationID,
            string content,
            DateTime createdDate,
            bool isStaff,
            int referencedContentID,
            short referencedContentType,
            string? referencedContentTitle)
        {
            MessageID = messageID;
            this.personFullName = personFullName;
            ConversationID = conversationID;
            Content = content;
            CreatedDate = createdDate;
            IsStaff = isStaff;
            ReferencedContentID = referencedContentID;
            ReferencedContentType = referencedContentType;
            ReferencedContentTitle = referencedContentTitle;
        }
    }
}