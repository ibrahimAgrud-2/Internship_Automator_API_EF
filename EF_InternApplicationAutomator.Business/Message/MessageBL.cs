using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.Messages;
using Shared.Conversations;
using Shared.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.Business.Message
{
    public class MessageBL
    {
        private readonly MessageDataAccess _MessageDataAccess;


        public MessageBL(MessageDataAccess messageDataAccess)
        {
            _MessageDataAccess = messageDataAccess;
        }

       
    }
}
