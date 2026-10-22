using EF_InternApplicationAutomator.Business.conversation;
using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.Message;
using EF_InternApplicationAutomator.Business.User;
using Microsoft.AspNetCore.Mvc;
using Shared.Application;
using Shared.Conversations;
using Shared.Intern;
using Shared.Message;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace EF_InternApplicationAutomator.API.Controllers.Intern
{
    [Route("api/Conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly ConversationBL _ConversationBL;
        private readonly MessageBL _Message;
        private readonly UserBL  _User;
        private readonly InternBL  _Intern;



        public ConversationController(ConversationBL conversation, MessageBL message, UserBL user,InternBL intern)
        {
            _ConversationBL = conversation;
            _Message = message;
            _User = user;
            _Intern = intern;
        }



        [HttpGet("GetAllMessages/{ConversationID}/{messageCount}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<vw_MessagesWithContentDTO>> GetAllMessagesForConversation(int ConversationID, int messageCount = 50)
        {
            List<vw_MessagesWithContentDTO> internList = _ConversationBL.GetAllMessagesForConversation(ConversationID, messageCount);
            return Ok(internList);
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ConversationResponseDTO>> GetAllConversation()
        {
            List<ConversationResponseDTO> internList = _ConversationBL.GetAllConversation();
            return Ok(internList);
        }


        [HttpPost("StaffAddMessage")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> StaffAddNewMessage(StaffAddMessageDTO newMessage)
        {
            if (newMessage == null||string.IsNullOrEmpty(newMessage.Content))
                return BadRequest("Girilen veri geçersiz");
            
            if(!_ConversationBL.IsConversationExists(newMessage.ConversationID))
            {
                return BadRequest("Mesaj yazmak istediğiniz sohbet odası geçersiz. Oda silinmiş olabilir");
            }
            if (!_User.IsUserExists(newMessage.UserID))
            {
                return BadRequest("Bu sohbet odasında mesaj yazamazsınız. Hesabınız silinmiş olabilir");
            }

            if (_ConversationBL.StaffAddMessage(newMessage))
            {
                return Ok(true);
            }


            return StatusCode(StatusCodes.Status500InternalServerError);

            
        }

        [HttpGet("Find/{ID}", Name = "FindConversation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ConversationResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Girdiğiniz ID ({ID}) geçersiz");
            }

            ConversationResponseDTO? intern = _ConversationBL.Find(ID);
            if (intern == null)
            {
                return NotFound($"{ID} ID'ye sahip Stajyer bulunamadı ");
            }

            return Ok(intern);
        }





        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ConversationResponseDTO> AddNewConversation(ConversationCreateDTO conversation)
        {
            if (conversation == null||conversation.InternID<1||string.IsNullOrEmpty(conversation.ConversationName))
            {
                return BadRequest("Sohbet başlatılmak için gerekli bilgiler yeterli değil veya geçersiz");
            }
            if (!conversation.IsGroup && !_Intern.IsInternExists(conversation.InternID))
            {
                return BadRequest("Sohbet başlatılmak istediğiniz stajyer sistemden silinmiş olabilir.");
            }
            //eğer sohbet grup değil ve intern zaten özel sohbeti varsa yeni sohbet  oluşturmadan o sohbeti dönder 
            if(!conversation.IsGroup&&_ConversationBL.IsInternHasPrivateConversation(conversation.InternID))
            {
                ConversationResponseDTO? conversationResponse = _ConversationBL.FindByInternID(conversation.InternID);
                if(conversationResponse!=null)
                {
                    return CreatedAtRoute("FindConversation", new { id = conversationResponse.ConversationID }, conversationResponse);
                }
            }
            ConversationResponseDTO cr = _ConversationBL.AddNewConversation(conversation);
            if (cr==null)
            {
                return BadRequest("Sohbet eklenemedi");
            }

            return CreatedAtRoute("FindConversation", new { id =  cr.ConversationID }, cr);
        }

    }
}