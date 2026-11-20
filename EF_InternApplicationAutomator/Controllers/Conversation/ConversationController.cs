
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.Conversation;
using EF_InternApplicationAutomator.Business.ConversationIntern;
using EF_InternApplicationAutomator.Business.Intern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Conversation;
using Shared.Message;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/Conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        Business.Conversation.ConversationBL _Conversation;
        Business.ConversationIntern.ConversationInternBL _ConversationInternBL;
        Business.Intern.InternBL _InterBL;
        Business.PersonBL _People;

        public ConversationController(ConversationBL conversation, PersonBL people, ConversationInternBL conversationInternBL, InternBL interBL)
        {
            _Conversation = conversation;
            _People = people;
            _ConversationInternBL = conversationInternBL;
            _InterBL = interBL;
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

                if (!_ConversationInternBL.DoesInternExistsInConversations(
                    conversationID, internID.Value))
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
            if (!_People.IsPersonExists(personID.Value)&&!_People.IsPersonActive(personID.Value))
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

                if (!_ConversationInternBL.DoesInternExistsInConversations(
                    message.ConversationID, internID.Value))
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




        [Authorize(Roles ="User")]
        [HttpPost("CreatePrivateConversation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> CreatePrivateConversation(ConversationCreateDTO Conversation,int internID)
        {
            if (Conversation == null)
            {
                return BadRequest("Grup Eklemek için girilen parametreler doğru değil");
            }
            int? personID = User.GetPersonID();
            if (personID == null)
            {
                return Unauthorized();
            }
            if (!_People.IsPersonExists(personID.Value) || !_People.IsPersonActive(personID.Value))
            {
                return BadRequest("Hesabınız Silinmiş veya aktif olmaadığı için yeni grup oluşturamazsınız");
            }
            if (!_InterBL.IsInternExists(internID))
            {
                return BadRequest("Sohbet oluşturmak istediğiniz intern bulunamadi");
            }
            //o intern ile zaaten bir sohbet varsa onun İD'sini dönder
            int conversationID = _Conversation.DoesInternHasAPrivateChat(internID);
            Console.WriteLine($"\n\n*************{conversationID}**********\n\n");
            if (conversationID>0)
            {
                return Ok(conversationID.ToString());
            }
            int createdConversationID = _Conversation.AddNewConversation(Conversation);
            if (createdConversationID > 0)
            {
                if (_ConversationInternBL.AddInternToConversation(createdConversationID, internID))
                {
                    return Ok(createdConversationID.ToString());
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


        [Authorize(Roles = "User")]
        [HttpPost("CreateGroupConversation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> CreateGroupConversation(GroupConversationCreateDTO groupConversation)
        {
            if (groupConversation == null)
            {
                return BadRequest("Grup Oluşturmak için girilen parametreler doğru değil");
            }
            int? personID = User.GetPersonID();
            if (personID == null)
            {
                return Unauthorized();
            }
            if (!_People.IsPersonExists(personID.Value) || !_People.IsPersonActive(personID.Value))
            {
                return BadRequest("Hesabınız Silinmiş veya aktif olmaadığı için yeni grup oluşturamazsınız");
            }
            int createdGroupID = _Conversation.AddNewGroupConversation(groupConversation);
            ;
            if (createdGroupID > 0)
            {
                return Ok(createdGroupID.ToString());
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [Authorize(Roles = "User")]
        [HttpPost("AddIntern")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> AddInternToConversation(int conversationID, int internID)
        {
            int? personID = User.GetPersonID();
            if (personID == null)
            {
                return Unauthorized();
            }
            if (!_People.IsPersonExists(personID.Value) || !_People.IsPersonActive(personID.Value))
            {
                return BadRequest("Hesabınız Silinmiş veya aktif olmaadığı için yeni grup oluşturamazsınız");
            }
            if (!_Conversation.IsConversationExists(conversationID))
            {
                return BadRequest("Sohbet bulunamdi");
            }
            if (!_InterBL.IsInternExists(internID))
            {
                return BadRequest("Eklemek istediğiniz stajyer bulunamadi");
            }
            if(_ConversationInternBL.DoesInternExistsInConversations(conversationID,internID))
            {
                return Ok();
            }
            if (_ConversationInternBL.AddInternToConversation(conversationID, internID))
            {
                return Ok();
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
      
        
        [Authorize(Roles = "User")]
        [HttpPost("RemoveIntern")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> RemoveInternFromConversation(int conversationID, int internID)
        {
            int? personID = User.GetPersonID();
            if (personID == null)
            {
                return Unauthorized();
            }
            if (!_People.IsPersonExists(personID.Value) || !_People.IsPersonActive(personID.Value))
            {
                return BadRequest("Hesabınız Silinmiş veya aktif olmaadığı için yeni grup oluşturamazsınız");
            }
            if (!_Conversation.IsConversationExists(conversationID))
            {
                return BadRequest("Sohbet bulunamdi");
            }
            if (!_InterBL.IsInternExists(internID))
            {
                return BadRequest("Eklemek istediğiniz stajyer bulunamadi");
            }

            if (!_ConversationInternBL.RemoveInternConversation(conversationID,internID))
            {
                return Ok();
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }



    }
}