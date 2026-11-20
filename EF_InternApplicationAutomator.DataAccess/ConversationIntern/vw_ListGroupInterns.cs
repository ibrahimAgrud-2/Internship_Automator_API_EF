using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.ConversationIntern
{
    [Table("vw_ListGroupInterns")]
    public class vw_ListGroupInterns
    {

        [Key]
        public int InternID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public vw_ListGroupInterns(int conversationID, int internID, string firstName, string lastName)
        {
            ConversationID = conversationID;
            this.InternID = internID;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}