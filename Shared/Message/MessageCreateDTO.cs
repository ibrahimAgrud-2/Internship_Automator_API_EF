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
        public string Content { get; set; }


        [Required]
        public bool IsStaff { get; set; }

        [Required]
        public int ReferencedContentID { get; set; }

        [Required]
        public short ReferencedContentType { get; set; }


        public MessageCreateDTO(
           int personID,
            int conversationID,
            string content,
            bool isStaff,
            int referencedContentID,
            short referencedContentType
           )
        {
          
            ConversationID = conversationID;
            Content = content;
            IsStaff = isStaff;
            ReferencedContentID = referencedContentID;
            ReferencedContentType = referencedContentType;
            this.PersonID = personID;
        }
    }
}