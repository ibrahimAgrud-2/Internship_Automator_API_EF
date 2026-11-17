
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business.ConversationIntern;
using Microsoft.AspNetCore.Authorization;
using EF_InternApplicationAutomator.API.Service;

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

        [Authorize(Roles ="Intern")]
        [HttpGet("AllConversation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ConversationInternResponseDTO>> GetAllInternConversation()
        {

            int? internID= User.GetInternID();

            if(internID==null)
            {
              return Unauthorized();
            }
            
            List<ConversationInternResponseDTO> conversationIntern = _ConversationInternBL.GetAllInternConversation(internID.Value);

            return Ok(conversationIntern);
        }

    
    }
}