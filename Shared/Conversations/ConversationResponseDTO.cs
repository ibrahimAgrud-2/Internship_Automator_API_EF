using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Conversations
{
    public class ConversationResponseDTO
    {
        [Required]
   
        public int ConversationID { get; set; }

   
        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }


        // Yapıcı Metot (Constructor)
        public ConversationResponseDTO(int conversationID, string conversationName, bool isGroup)
        {
            ConversationID = conversationID;
     
            ConversationName = conversationName;
            IsGroup = isGroup;
        }

    }
}
