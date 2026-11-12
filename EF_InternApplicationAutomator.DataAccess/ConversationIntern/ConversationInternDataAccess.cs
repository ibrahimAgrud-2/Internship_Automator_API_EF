using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application;
using Shared.Intern;

using Shared.Intern.ConversationIntern;
using Shared.Message;


namespace EF_InternApplicationAutomator.DataAccess.ConversationIntern
{
    public class ConversationInternDataAccess
    {

        
        private readonly IAADbContext _Context;
       
        public ConversationInternDataAccess(IAADbContext context)
        {
            _Context = context;
          
        }
        public List<ConversationInternResponseDTO> GetAllInternConversation(int internID)
        {

            return _Context.ConversationIntern
                .AsNoTracking().Where(c => c.InternID == internID)
                .Select(c => new ConversationInternResponseDTO(c.ConversationID,c.InternID,c.ConversationName,c.IsGroup))
                .ToList();
        }

       

        

    }
}
