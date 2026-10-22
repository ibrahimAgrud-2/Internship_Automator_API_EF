using EF_InternApplicationAutomator.DataAccess.Conversation;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Conversations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.ConversationInterns
{

    public class ConversationInternsDataAccess
    {
        private readonly IAADbContext _Context;

        public ConversationInternsDataAccess(IAADbContext context)
        {
            _Context = context;
        }

      



    }
}
