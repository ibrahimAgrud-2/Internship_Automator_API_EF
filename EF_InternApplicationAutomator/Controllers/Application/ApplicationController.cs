using Microsoft.AspNetCore.Mvc;
using EF_InternApplicationAutomator.Business.Application;
using Shared.Application;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using EF_InternApplicationAutomator.API.Service;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/Application")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        ApplicationBL _application;

        public ApplicationController(ApplicationBL application)
        {
            _application = application;
        }

        //add herkese açık
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<bool> AddNewApplication(ApplicationCreateSecure newApplicationDTO)
        {
            if (newApplicationDTO == null || newApplicationDTO.PersonInfo == null)
            {
                return BadRequest("Invalid Application data.");
            }

            int ID = _application.AddApplication(newApplicationDTO);

            if (ID < 1)
            {
                return BadRequest("sistemden kaynaklana bir sorundan ötürü başvuru eklenemedi");
            }

            return Ok(true);
        }



        [Authorize(Roles ="User")]
        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ApplicationDetailedListDTO>> GetAllApplications()
        {
            List<ApplicationDetailedListDTO> applicationList = _application.GetAllApplications();

            if (applicationList.Count == 0)
            {
                return NotFound("No Data Available in the Table");
            }

            return Ok(applicationList);
        }



        [Authorize(Roles = "User")]
        [HttpGet("{ApplicationID}", Name = "FindApplication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ApplicationResponseDTO> Find(int ApplicationID)
        {
            if (ApplicationID < 1)
            {
                return BadRequest($"Girilen ApplicationID {ApplicationID} geçersiz");
            }

            ApplicationResponseDTO app = _application.Find(ApplicationID);

            if (app == null)
            {
                return NotFound($"No Application With ApplicationID {ApplicationID}");
            }

            return Ok(app);
        }





        //sadece status upadte edilebilir
        [Authorize(Roles = "User")]
        [HttpPut("Update/AppStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApplicationStatusUpdateDTO> UpdateStatus(ApplicationStatusUpdateDTO updatedApplication)
        {
            if (updatedApplication == null || updatedApplication.ApplicationID < 1)
            {
                return BadRequest("Invalid Application data.");
            }
            int? reviewedByUserID = User.GetUserID();
            if (reviewedByUserID == null)
            {
                return Unauthorized();
            }
            if (_application.UpdateStatus(updatedApplication.ApplicationID, reviewedByUserID.Value,(ApplicationBL.enStatus)updatedApplication.status,updatedApplication.Notes) )
            {
                return Ok(updatedApplication);
            }
            else
            {
                return NotFound("Başvurunun durumu (status) güncellenemedi");
            }
        }



        [Authorize(Roles = "User")]
        [HttpPut("Update/AppStatus/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Accepted(int applicationID,string? note)
        {
            ApplicationResponseDTO application = _application.Find(applicationID);
            if (application == null)
            {
                return BadRequest("başvuru bulunamadı");
            }
            if (application.Status != (int)ApplicationBL.enStatus.newApp)
            {
                return BadRequest("başvuru geçerli değil -  kabul edilmiş veya iptal edilmiş");

            }
            int? reviewedByUserID = User.GetUserID();
            if(reviewedByUserID==null)
            {
                return Unauthorized();
            }
            if (_application.ApplicationAccepted(applicationID, reviewedByUserID.Value,note))
            {
                return Ok();
            }
            else
            {

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }




        [Authorize(Roles = "User")]
        [HttpDelete("{id}", Name = "DeleteApplication")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteApplication(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ApplicationID {id}");
            }

            if (_application.DeleteApplication(id))
            {
                return NoContent();
            }
            else
            {
                return NotFound($"Application with ApplicationID {id} not found. No rows deleted!");
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("exists/{appID}")]
        public bool IsUserExists(int appID)
        {
            if (appID < 1)
            {
                return false;
            }


            if (_application.IsApplicationExists(appID))
            {
                return true;
            }
            return false;

        }
    }
}