using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.ConversationIntern
{
    [Table("ConversationInterns")]
    public class ConversationInternEntity
    {
        [Key]
        public int ConversationID { get; set; }

        [Required]
        public int InternID { get; set; }


        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        public ConversationInternEntity(int conversationID, int internID, string conversationName)
        {
            ConversationID = conversationID;
            this.InternID = internID;
            ConversationName = conversationName;

        }
    }
}