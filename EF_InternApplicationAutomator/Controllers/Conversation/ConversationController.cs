using Microsoft.AspNetCore.Mvc;
using EF_InternApplicationAutomator.Business.Application;
using Shared.Application;
using System.Collections.Generic;
using System;
using Shared.Intern;
using EF_InternApplicationAutomator.Business.Conversation;

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
      
    }
}