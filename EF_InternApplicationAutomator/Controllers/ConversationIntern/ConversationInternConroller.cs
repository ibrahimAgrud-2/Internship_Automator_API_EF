
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business.ConversationIntern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Intern.ConversationIntern;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/ConversationInternConroller")]
    [ApiController]
    public class ConversationInternConroller : ControllerBase
    {
        ConversationInternBL _ConversationInternBL;

        public ConversationInternConroller(ConversationInternBL conversationInternBL)
        {
            _ConversationInternBL = conversationInternBL;
        }

        //bu sadee intern için dahil olduğı sohbetleri listelemek için
        //personel kısmında ise GetALlConversations var
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