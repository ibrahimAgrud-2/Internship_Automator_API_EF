using Microsoft.AspNetCore.Mvc;
using EF_InternApplicationAutomator.Business.Application;
using Shared.Application;
using System.Collections.Generic;
using System;

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

        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ApplicationListDTO>> GetAllApplications()
        {
            List<ApplicationListDTO> applicationList = _application.GetAllApplications();

            if (applicationList.Count == 0)
            {
                return NotFound("No Data Available in the Table");
            }

            return Ok(applicationList);
        }


        [HttpGet("{ID}", Name = "FindApplication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ApplicationResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not Accepted ID {ID}");
            }

            ApplicationResponseDTO app = _application.Find(ID);

            if (app == null)
            {
                return NotFound($"No Application With ID {ID}");
            }

            return Ok(app);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApplicationResponseDTO> AddNewApplication(ApplicationCreateSecure newApplicationDTO)
        {
            if (newApplicationDTO == null||newApplicationDTO.PersonInfo==null)
            {
                return BadRequest("Invalid Application data.");
            }

  

            int ID = _application.AddApplication(newApplicationDTO);

            if (ID < 1)
            {
                return BadRequest("Could Not Add Application");
            }

            return CreatedAtRoute("FindApplication", new { id = ID }, newApplicationDTO);
        }


        [HttpPut("Update/AppStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApplicationStatusUpdateDTO> UpdateStatus(ApplicationStatusUpdateDTO updatedApplication)
        {
            if (updatedApplication == null||updatedApplication.ApplicationID < 1)
            {
                return BadRequest("Invalid Application data.");
            }

            if (_application.UpdateStatus(updatedApplication) > 0)
            {
                return Ok(updatedApplication);
            }
            else
            {
                return NotFound("Could Not Update Application");
            }
        }


            [HttpPut("Update/AppStatus/accepted")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public ActionResult<string> Accepted(ApplicationStatusUpdateDTO updatedApplication)
            {
                if (updatedApplication == null || updatedApplication.ApplicationID < 1)
                {
                    return BadRequest("Invalid Application data");
                }

                string[] InsertedPassword = _application.ApplicationAccepted(updatedApplication);
                if (InsertedPassword.Count()!=0)
                {
                   return Ok();
                }
                else
                {

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
                
            }


        [HttpDelete("{id}", Name = "DeleteApplication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteApplication(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ID {id}");
            }

            if (_application.DeleteApplication(id))
            {
                return NoContent();
            }
            else
            {
                return NotFound($"Application with ID {id} not found. No rows deleted!");
            }
        }


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