using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Conversations
{
    public class ConversationCreateDTO
    {

        [Required]
        public int[] InternIDs { get; set; }

        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }


       
        public ConversationCreateDTO(int[] internIDs, string conversationName, bool isGroup)
        {
      
            InternIDs = internIDs;
            ConversationName = conversationName;
            IsGroup = isGroup;
        }

    }
}
