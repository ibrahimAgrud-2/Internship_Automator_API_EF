using EF_InternApplicationAutomator.DataAccess.Conversation;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Conversations;
using Shared.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Messages
{
    public class MessageDataAccess
    {
        private readonly IAADbContext _Context;

        public MessageDataAccess(IAADbContext context)
        {
            _Context = context;
        }



        public bool AddMessage(MessageCreateDTO message)
        {
            MessageEntity messageEntity = new MessageEntity(0, message.PersonID, message.ConversationID, message.Content, DateTime.Now, message.IsStaff, message.ReferencedContentID, message.ReferencedContentType);
            EntityEntry<MessageEntity> addedIntern = _Context.Messages.Add(messageEntity);

            if (_Context.SaveChanges() > 0)
            {

                return true;
            }

            return true;
        }


    }
}
