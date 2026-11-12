using System.ComponentModel.DataAnnotations;

namespace Shared.Intern.ConversationIntern
{
    //Conversation response DTO
    public class ConversationInternResponseDTO
    {
        [Required]
        public int ConversationID { get; set; }

        [Required]
        public int InternID { get; set; }


        [Required]
        public string ConversationName { get; set; }

        [Required]
        public bool IsGroup { get; set; }

        public ConversationInternResponseDTO(int ConversationID,int internID,string ConversationName, bool IsGroup)
        {
            this.ConversationID = ConversationID;
            this.IsGroup = IsGroup;
            this.ConversationName = ConversationName;
            this.InternID = internID;
        }

    }
}
