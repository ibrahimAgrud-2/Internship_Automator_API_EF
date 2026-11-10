using Microsoft.AspNetCore.Mvc;
using EF_InternApplicationAutomator.Business.Application;
using Shared.Application;
using System.Collections.Generic;
using System;
using Shared.Intern;
using EF_InternApplicationAutomator.Business.Conversation;
using Shared.Message;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/Conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        ConversationBL _Conversation;

        public ConversationController(ConversationBL conversation)
        {
            _Conversation = conversation;
        }

        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ConversationResponseDTO>> GetAllConversation()
        {
            List<ConversationResponseDTO> applicationList = _Conversation.GetAllConversation();

            if (applicationList.Count == 0)
            {
                return NotFound("Kayıt Bulunamadı");
            }

            return Ok(applicationList);
        }

        [HttpGet("AllMessages/{ConversationID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<MessageDetailedResponseDTO>> GetAllMessages(int ConversationID)
        {
            List<MessageDetailedResponseDTO> messageList = _Conversation.GetAllMessages(ConversationID);

            if (messageList.Count == 0)
            {
                return NotFound("Bu sohbette mesaj bulunmamaktadır");
            }

            return Ok(messageList);
        }

    }
}