using System.ComponentModel.DataAnnotations;

namespace Shared.Intern
{
    public class InterGroupListResponseDTO
    {
        [Required]
        public int InternID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public InterGroupListResponseDTO(int conversationID, int internID, string firstName, string lastName)
        {
            ConversationID = conversationID;
            this.InternID = internID;
            FirstName = firstName;
            LastName = lastName;
        }

    }
}
