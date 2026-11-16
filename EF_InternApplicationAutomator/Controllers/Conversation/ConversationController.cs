
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.Application;
using EF_InternApplicationAutomator.Business.Conversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application;
using Shared.Intern;
using Shared.Message;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

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
            int? personID = User.GetPersonID();
            //mesajo gönderen kişi personel (user) mi değil mi
            //ayrıca isStaff bilgisini token'dan okusun
            bool isStaff = User.IsInRole("User");
        
            if (int.TryParse(personID.ToString(),out int result))
            {
                message.PersonID = result;
                int ID = _Conversation.AddMessage(message, isStaff);

                if (ID < 1)
                {
                    return BadRequest("Mesaj Eklenemedi");
                }
                else
                {
                    return Ok(true);
                }

            }
            else
            {
                return NotFound("Sistemde Kaydınız olmadığı için mesaj gönderemezsiniz");

            }
        }
    
    
    }
}