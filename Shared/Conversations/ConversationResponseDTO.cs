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
        public int? InternID { get; set; }

        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }


        // Yapıcı Metot (Constructor)
        public ConversationResponseDTO(int conversationID, int? internID, string conversationName, bool isGroup)
        {
            ConversationID = conversationID;
            InternID = internID;
            ConversationName = conversationName;
            IsGroup = isGroup;
        }

    }
}
