using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_InternApplicationAutomator.DataAccess.Conversation
{
    [Table("Conversations")]
    public class ConversationEntity
    {
        [Key]
        public int ConversationID { get; set; }

        public int? InternID { get; set; }

        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }


        public ConversationEntity(int conversationID, int? internID, string conversationName, bool isGroup)
        {
            ConversationID = conversationID;
            InternID = internID;
            ConversationName = conversationName;
            IsGroup = isGroup;
        }
    }
}