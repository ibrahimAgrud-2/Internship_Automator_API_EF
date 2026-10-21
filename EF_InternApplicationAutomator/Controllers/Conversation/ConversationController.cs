using EF_InternApplicationAutomator.Business.conversation;
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



        public ConversationController(ConversationBL conversation, MessageBL message, UserBL user)
        {
            _ConversationBL = conversation;
            _Message = message;
            _User = user;
        }



        [HttpGet("GetAllMessages/{ConversationID}/{messageCount}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<vw_MessagesWithContentDTO>> GetAllMessagesForConversation(int ConversationID, int messageCount = 50)
        {
            List<vw_MessagesWithContentDTO> internList = _ConversationBL.GetAllMessagesForConversation(ConversationID, messageCount);
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


        //[HttpGet("All")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public ActionResult<IEnumerable<ConversationResponseDTO>> GetAllInterns()
        //{
        //    List<ConversationResponseDTO> internList = _ConversationBL.GetAll();
        //    return Ok(internList);
        //}

       
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public ActionResult<InternResponseDTO> AddNewIntern(InternCreateDTO newIntern)
        //{
        //    if (newIntern == null)
        //    {
        //        return BadRequest("Invalid Intern data.");
        //    }

        //    int ID = _Intern.AddIntern(newIntern);
        //    if (ID < 1)
        //    {
        //        return BadRequest("Could Not Add Intern");
        //    }

        //    return CreatedAtRoute("FindIntern", new { id = ID }, newIntern);
        //}

        //[HttpPut("{id}", Name = "UpdateIntern")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult<InternResponseDTO> UpdateIntern(int id, InternCreateDTO internDTO)
        //{
        //    if (id < 1 || internDTO == null)
        //    {
        //        return BadRequest("Invalid Intern data.");
        //    }

        //    if (!_Intern.IsInternExists(id))
        //    {
        //        return NotFound($"No Intern With ID {id}");
        //    }

        //    if (_Intern.UpdateIntern(id, internDTO) > 0)
        //    {
        //        return Ok(internDTO);
        //    }

        //    return BadRequest("Could Not Update Intern");
        //}

        //[HttpDelete("{id}", Name = "DeleteIntern")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult DeleteIntern(int id)
        //{
        //    if (id < 1)
        //    {
        //        return BadRequest($"Not Accepted ID {id}");
        //    }

        //    if (!_Intern.IsInternExists(id))
        //    {
        //        return NotFound($"No Intern With ID {id}");
        //    }

        //    if (_Intern.DeleteIntern(id))
        //    {
        //        return NoContent();
        //    }

        //    return NotFound("Delete Process terminated. No rows deleted!");
        //}

        //[HttpGet("exists/{internID}")]
        //public bool IsInternExists(int internID)
        //{
        //    if (internID < 1)
        //    {
        //        return false;
        //    }

        //    return _Intern.IsInternExists(internID);
        //}


        //[HttpPost("InternLogin")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public ActionResult<InterLoginResponseDTO> InternLogin(InternLoginDTO internLogin)
        //{
        //    if (string.IsNullOrEmpty(internLogin.Email) || string.IsNullOrEmpty(internLogin.Password))
        //    {
        //        return BadRequest($"E-posta ve şifre zorunludur.");
        //    }

        //    InterLoginResponseDTO intern = _Intern.FindByPasswordAndEmail(internLogin.Email, internLogin.Password);
        //    if (intern == null)
        //    {
        //        return NotFound("E-posta veya şifre hatalı");
        //    }
        //    else if (intern.Status == (byte)Business.Intern.InternBL.enStatus.separated)
        //    {
        //        return StatusCode(403, new { message = "Stajyer hesabınız aktif değil " });
        //    }

        //    return Ok(intern);
        //}


        //[HttpPut("Separate/{internID}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public ActionResult<string> SeparateIntern(int internID)
        //{
        //    if (internID < 1)
        //    {
        //        return BadRequest("Invalid Intern ID ");
        //    }
        //    if (!_Intern.IsInternExists(internID))
        //    {
        //        return NotFound("Intern Does Not Exists");
        //    }
        //    if (_Intern.IsInternActive(internID))
        //    {
        //        return Ok();
        //    }
        //    else
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }

        //}

    }
}