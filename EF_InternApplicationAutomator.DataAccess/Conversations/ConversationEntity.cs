using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_InternApplicationAutomator.DataAccess.Conversation
{
    [Table("Conversations")]
    public class ConversationEntity
    {
        [Key]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(150)]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }


        public ConversationEntity(int conversationID, string conversationName, bool isGroup)
        {
            ConversationID = conversationID;
            ConversationName = conversationName;
            IsGroup = isGroup;
        }
    }
}