
using EF_InternApplicationAutomator.Business.ConversationIntern;
using Microsoft.AspNetCore.Mvc;
using Shared.Application;
using Shared.Intern;
using Shared.Intern.ConversationIntern;
using Shared.Message;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/ConversationIntern")]
    [ApiController]
    public class ConversationIntern : ControllerBase
    {
        ConversationInternBL _ConversationInternBL;

        public ConversationIntern(ConversationInternBL conversationInternBL)
        {
            _ConversationInternBL = conversationInternBL;
        }

        [HttpGet("AllConversation/{internID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ConversationInternResponseDTO>> GetAllInternConversation(int internID)
        {
            List<ConversationInternResponseDTO> conversationIntern = _ConversationInternBL.GetAllInternConversation(internID);

            if (conversationIntern.Count == 0)
            {
                return NotFound("Kayıt Bulunamadı");
            }

            return Ok(conversationIntern);
        }

    
    }
}