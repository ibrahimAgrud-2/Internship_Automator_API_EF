
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.Conversation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Conversation;
using EF_InternApplicationAutomator.Business.Conversation;
using Shared.Message;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/Conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        Business.Conversation.ConversationBL _Conversation;
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
            
    

            return Ok(applicationList);
        }


        [Authorize]
        [HttpGet("AllMessages/{conversationID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<MessageDetailedResponseDTO>> GetAllMessages(int conversationID)
        {
            if(!_Conversation.IsConversationExists(conversationID))
            {
                return Ok();
            }
            bool isIntern = User.IsInRole("Intern");
            if (isIntern)
            {
                int? internID = User.GetInternID();

                if (internID == null)
                {
                    return Unauthorized();
                }

                if (!_Conversation.IsInternExistsInConversations(
                    internID.Value, conversationID))
                {
                    return Forbid();
                }

            }

                List<MessageDetailedResponseDTO> messageList = _Conversation.GetAllMessages(conversationID);
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
            if (_People.IsPersonExists(personID.Value)&&_People.IsPersonActive(personID.Value))
            {
                return BadRequest("Hesabınızdan mesaj ekleyemezsiniz. Silinmiş veya aktif olmayabilr");
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

                if (!_Conversation.IsInternExistsInConversations(
                    internID.Value, message.ConversationID))
                {
                    return Forbid();
                }
            }
          
          
            int ID = _Conversation.AddMessage(message,personID.Value, isStaff);

            if (ID < 1)
            {
                return BadRequest("Mesaj Eklenemedi");
            }

            return Ok(true);
        }



        [Authorize]
        [HttpPost("CreateConversation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<bool> CreateConversation(ConversationCreateDTO Conversation)
        {
            if (Conversation == null)
            {
                return BadRequest("Grup Eklemek için girilen parametreler doğru değil");
            }

            //API kısmında bir intern'İn 2 defa sobet oluşturmasını engellicem)
            
            if(User.IsInRole("Intern"))
            {
                int? internID = User.GetInternID();
                if(internID==null|| Conversation.IsGroup)
                {
                    return Unauthorized();
                }
                if(_Conversation.DoesInternHasAPrivateChat(internID.Value))
                {
                    return BadRequest();
                }

               
            }

            int? personID = User.GetPersonID();
            if (personID == null)
            {
                return Unauthorized();
            }
            if (!_People.IsPersonExists(personID.Value) ||! _People.IsPersonActive(personID.Value))
            {
                return BadRequest("Hesabınız Silinmiş veya aktif olmaadığı için yeni grup oluşturamazsınız");
            }

            if (_Conversation.AddNewConversation(Conversation))
            {
                return Ok(true);
            }
           
            return StatusCode(StatusCodes.Status500InternalServerError);
        }


    }
}