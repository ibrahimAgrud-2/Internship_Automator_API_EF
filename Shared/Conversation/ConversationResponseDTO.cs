using System.ComponentModel.DataAnnotations;

namespace Shared.Conversation
{
    //Conversation ConversationResponseDTO DTO
    public class ConversationResponseDTO
    {
        [Required]
        public int ConversationID { get; set; }

        [Required]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }

        public ConversationResponseDTO(int ConversationID,string ConversationName, bool IsGroup)
        {
            this.ConversationID = ConversationID;
            this.IsGroup = IsGroup;
            this.ConversationName = ConversationName;
        }

    }
}
