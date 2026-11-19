using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Data;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.Message;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application;
using Shared.Intern;
using Shared.OngoingProjects;
using System;
using System.Drawing;
using System.Security.Policy;

namespace EF_InternApplicationAutomator.DataAccess.Message
{
    public class MessagesDataAccess
    {
        private readonly IAADbContext _Context;
   


        public MessagesDataAccess(IAADbContext context)
        {
            _Context = context;
        
        }

       
        public int AddMessage(Shared.Message.MessageCreateDTO message, int personID, bool isStaff)
        {
            MessageEntity messageEntity = new MessageEntity(0, personID, message.ConversationID,message.Content,DateTime.Now,isStaff, message.ReferencedContentID,message.ReferencedContentType);
            EntityEntry<MessageEntity> addedIntern = _Context.Messages.Add(messageEntity);

            if (_Context.SaveChanges() > 0)
            {
                return messageEntity.MessageID;
            }

            return -1;
        }





    }
}