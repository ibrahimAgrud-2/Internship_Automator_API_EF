using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ConversationInterns
{
    public   class ConversationInternsResponseDTO
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int InternID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        public ConversationInternsResponseDTO(int ID, int internID, int conversationID)
        {
            this.ID = ID;
            this.InternID = InternID;
            this.ConversationID = conversationID;
        }
    }
}
