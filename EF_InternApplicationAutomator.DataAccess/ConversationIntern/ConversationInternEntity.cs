using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.ConversationIntern
{
    [Table("vw_ConversationIntern")]
    public class vw_ConversationIntern
    {
        [Key]
        public int ConversationID { get; set; }

        [Required]
        public int InternID { get; set; }


        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }

        public vw_ConversationIntern(int conversationID, int internID, string conversationName, bool isGroup)
        {
            ConversationID = conversationID;
            this.InternID = internID;
            ConversationName = conversationName;
            IsGroup = isGroup;
        }
    }
}