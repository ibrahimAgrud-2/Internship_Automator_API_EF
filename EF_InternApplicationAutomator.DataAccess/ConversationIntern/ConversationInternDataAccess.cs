using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Data;
using EF_InternApplicationAutomator.DataAccess.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualBasic;
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

            return _Context.vw_ConversationIntern
                .AsNoTracking().Where(c => c.InternID == internID)
                .Select(c => new ConversationInternResponseDTO(c.ConversationID,c.InternID,c.ConversationName,c.IsGroup))
                .ToList();


        }

        public int DoesInternHasAPrivateChat(int internID)
        {
            vw_ConversationIntern conversationIntern = _Context.vw_ConversationIntern.FirstOrDefault(c => c.InternID == internID && !c.IsGroup);
            
           
            return (conversationIntern==null)?-1: conversationIntern.ConversationID;
        }

        public bool AddInternToConversation(int conversationID,int internID)
        {

            ConversationInternEntity conversationInternEntity = new ConversationInternEntity(conversationID,internID);

            EntityEntry<ConversationInternEntity> addedIntern = _Context.ConversationIntern.Add(conversationInternEntity);

            if (_Context.SaveChanges() > 0)
            {
                _Context.Entry(conversationInternEntity).State = EntityState.Detached;
                return true;
            }

            return false;
        }


    }
}
