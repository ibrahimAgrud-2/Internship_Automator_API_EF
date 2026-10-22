using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.ConversationInterns
{
    [Table("ConversationInterns")]
    public class ConversationInternsEntity
    {

    
        [Key]
        public int ID { get; set; }

        [Required]
        public int InternID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        public ConversationInternsEntity(int ID,int internID,int conversationID)
        {
            this.ID = ID;
            this.InternID = internID;
            this.ConversationID = conversationID;
        }
    }
}
