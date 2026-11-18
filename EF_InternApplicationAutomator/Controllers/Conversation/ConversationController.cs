
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.Conversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Intern;
using Shared.Message;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/Conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        ConversationBL _Conversation;
        Business.PersonBL _People;

        public ConversationController(ConversationBL conversation, PersonBL people)
        {
            _Conversation = conversation;
            _People = people;
        }


        [Authorize(Roles ="User")]
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


        [Authorize]
        [HttpGet("AllMessages/{ConversationID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<MessageDetailedResponseDTO>> GetAllMessages(int ConversationID)
        {
            List<MessageDetailedResponseDTO> messageList = _Conversation.GetAllMessages(ConversationID);
            return Ok(messageList);
        }


        [Authorize]
        [HttpPost("AddMessage")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<bool> AddMessage(MessageCreateDTO message)
        {
            if (message == null)
            {
                return BadRequest("Mesaj Eklemek için girilen parametreler verilen değil");
            }
            if (!_Conversation.IsConversationExists(message.ConversationID))
            {
                return NotFound("Mesaj eklemek istediğiniz Sohbet bulunamadı.");
            }
            //personID'i DTO'dan değil, token'dan okusun
            // Kimlik ve rol token'dan alınır, gövdedeki değerler kullanılmaz
            int? personID = User.GetPersonID();
            if (personID == null)
            {
                return Unauthorized();
            }
            //intern mesaj ekleyebilir mi? Personel için bir kontrole gerek yok
            bool isStaff = User.IsInRole("User");
            if (!isStaff)
            {
                int? internID = User.GetInternID();

                if (internID == null)
                {
                    return Unauthorized();
                }

                if (!_Conversation.CanInternAddMessageToConversation(
                    internID.Value, message.ConversationID))
                {
                    return Forbid();
                }
            }
            message.PersonID = personID.Value;
          
            int ID = _Conversation.AddMessage(message, isStaff);

            if (ID < 1)
            {
                return BadRequest("Mesaj Eklenemedi");
            }

            return Ok(true);
        }
    
    
    }
}