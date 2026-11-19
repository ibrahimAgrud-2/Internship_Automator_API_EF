using System.ComponentModel.DataAnnotations;

namespace Shared.Conversation
{
    //Conversation response DTO
    public class ConversationCreateDTO
    {

        [Required]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }

        public ConversationCreateDTO(string ConversationName, bool IsGroup)
        {
   
            this.IsGroup = IsGroup;
            this.ConversationName = ConversationName;
        }

    }
}
