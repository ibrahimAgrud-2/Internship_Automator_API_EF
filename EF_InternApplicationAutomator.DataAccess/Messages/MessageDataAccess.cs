using EF_InternApplicationAutomator.DataAccess.Conversation;
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

    }
}
