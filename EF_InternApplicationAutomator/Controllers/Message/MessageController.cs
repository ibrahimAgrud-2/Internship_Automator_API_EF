using EF_InternApplicationAutomator.Business.conversation;
using EF_InternApplicationAutomator.Business.Message;
using Microsoft.AspNetCore.Mvc;
using Shared.Conversations;
using Shared.Message;

namespace EF_InternApplicationAutomator.API.Controllers.Message
{

    [Route("api/Message")]
    [ApiController]
    public class MessageController:ControllerBase
    {

        private readonly MessageBL _MessageBL;



        public MessageController(MessageBL message)
        {
            _MessageBL = message;
        }


    }
}
