using System.ComponentModel.DataAnnotations;

namespace Shared.Conversation
{
   
    public class GroupConversationCreateDTO
    {

        [Required]
        public string ConversationName { get; set; }

      public  HashSet<int> InterIDs {get; set; }
        public GroupConversationCreateDTO(string ConversationName, HashSet<int> interIDs)
        {
            //this.InterIDs = new HashSet<int>();
            this.InterIDs = interIDs ;
            this.ConversationName = ConversationName;
        }

    }
}
