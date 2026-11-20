
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business.Conversation;
using EF_InternApplicationAutomator.Business.ConversationIntern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Conversation;
using Shared.Intern;
using Shared.Intern.ConversationIntern;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/ConversationIntern")]
    [ApiController]
    public class ConversationInternConroller : ControllerBase
    {
        ConversationInternBL _ConversationInternBL;
       Business.Conversation.ConversationBL _ConversationBL;

        public ConversationInternConroller(ConversationInternBL conversationInternBL, ConversationBL conversationBL)
        {
            _ConversationInternBL = conversationInternBL;
            _ConversationBL = conversationBL;
        }

        //bu sadee intern için dahil olduğı sohbetleri listelemek için
        //personel kısmında ise GetALlConversations var
        [Authorize(Roles ="Intern")]
        [HttpGet("AllConversation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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




        [Authorize(Roles = "User")]
        [HttpGet("AllConversationIntern/{conversationID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public ActionResult<IEnumerable<InterGroupListResponseDTO>> GetAllConversationIntern(int conversationID)
        {


            if(!_ConversationBL.IsConversationExists(conversationID))
            {
                return BadRequest("Sohbet bulunamadı");
            }

            List<InterGroupListResponseDTO> conversationIntern = _ConversationInternBL.GetAllConversationIntern(conversationID);
            return Ok(conversationIntern);
        }


    }
}