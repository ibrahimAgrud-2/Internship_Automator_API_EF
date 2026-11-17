using EF_InternApplicationAutomator.Business.Intern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Intern;
namespace EF_InternApplicationAutomator.API.Controllers.Intern
{
    [Route("api/Intern")]
    [ApiController]
    public class InternController : ControllerBase
    {
        private readonly InternBL _Intern;



        public InternController(InternBL intern)
        {
            _Intern = intern;
        }

        [Authorize(Roles ="User")]
        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<InternDetailedResponseDTO>> GetAllInterns()
        {
            List<InternDetailedResponseDTO> internList = _Intern.GetDetailedInternRecord();
            return Ok(internList);
        }


        [Authorize(Roles = "User")]
        [HttpGet("{ID}", Name = "FindIntern")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<InternCardResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Verilen ID {ID} geçerli değil");
            }

            InternCardResponseDTO intern = _Intern.Find(ID);
            if (intern == null)
            {
                return NotFound($"No Intern With ID {ID}");
            }

            return Ok(intern);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InternCreateDTO> AddNewIntern(InternCreateDTO newIntern)
        {
            if (newIntern == null)
            {
                return BadRequest("Invalid Intern data.");
            }

            int ID = _Intern.AddIntern(newIntern);
            if (ID < 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            newIntern.CreatedDate = DateTime.Now;
            return CreatedAtRoute("FindIntern", new { id = ID }, newIntern);
        }



        [Authorize(Roles = "User")]
        [HttpPut("{id}", Name = "UpdateIntern")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
         public ActionResult<InternResponseDTO> UpdateIntern(int id, InternCreateDTO internDTO)
        {
            if (id < 1 || internDTO == null)
            {
                return BadRequest("Invalid Intern data.");
            }

            if (!_Intern.IsInternExists(id))
            {
                return NotFound($"No Intern With ID {id}");
            }

            if (_Intern.UpdateIntern(id, internDTO) > 0)
            {
                return Ok(internDTO);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }



        [Authorize(Roles = "User")]
        [HttpDelete("{id}", Name = "DeleteIntern")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult DeleteIntern(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ID {id}");
            }

            if (!_Intern.IsInternExists(id))
            {
                return NotFound($"No Intern With ID {id}");
            }

            if (_Intern.DeleteIntern(id))
            {
                return NoContent();
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


        [Authorize(Roles = "User")]
        [HttpGet("exists/{internID}")]
        public bool IsInternExists(int internID)
        {
            if (internID < 1)
            {
                return false;
            }

            return _Intern.IsInternExists(internID);
        }

        [Authorize(Roles = "User")]
        [HttpPut("Separate/{internID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<string> SeparateIntern(int internID)
        {
            if (internID < 1)
            {
                return BadRequest("Invalid Intern ID ");
            }
            if (!_Intern.IsInternExists(internID))
            {
                return NotFound("Intern Does Not Exists");
            }
            if (!_Intern.IsInternActive(internID))
            {
                return BadRequest("İntern Zaten ayrılmış");
            }
            if(_Intern.SeparateIntern(internID))
            {
                return Ok();
            }
            return StatusCode(StatusCodes.Status500InternalServerError);

        }

    }
}