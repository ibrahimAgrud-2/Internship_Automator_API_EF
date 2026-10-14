using EF_InternApplicationAutomator.Business.Intern;
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

        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<InternDetailedResponseDTO>> GetAllInterns()
        {
            List<InternDetailedResponseDTO> internList = _Intern.GetDetailedInternRecord();
            return Ok(internList);
        }

        [HttpGet("{ID}", Name = "FindIntern")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<InternResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not Accepted ID {ID}");
            }

            InternResponseDTO intern = _Intern.Find(ID);
            if (intern == null)
            {
                return NotFound($"No Intern With ID {ID}");
            }

            return Ok(intern);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InternResponseDTO> AddNewIntern(InternCreateDTO newIntern)
        {
            if (newIntern == null)
            {
                return BadRequest("Invalid Intern data.");
            }

            int ID = _Intern.AddIntern(newIntern);
            if (ID < 1)
            {
                return BadRequest("Could Not Add Intern");
            }

            return CreatedAtRoute("FindIntern", new { id = ID }, newIntern);
        }

        [HttpPut("{id}", Name = "UpdateIntern")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            return BadRequest("Could Not Update Intern");
        }

        [HttpDelete("{id}", Name = "DeleteIntern")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            return NotFound("Delete Process terminated. No rows deleted!");
        }

        [HttpGet("exists/{internID}")]
        public bool IsInternExists(int internID)
        {
            if (internID < 1)
            {
                return false;
            }

            return _Intern.IsInternExists(internID);
        }


        [HttpPost("InternLogin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<InterLoginResponseDTO> InternLogin(InternLoginDTO internLogin)
        {
            if (string.IsNullOrEmpty(internLogin.Email)|| string.IsNullOrEmpty(internLogin.Password))
            {
                return BadRequest($"E-posta ve şifre zorunludur.");
            }

            InterLoginResponseDTO intern = _Intern.FindByPasswordAndEmail(internLogin.Email,internLogin.Password);
            if(intern==null)
            {
                return NotFound("E-posta veya şifre hatalı");
            }
            else if(intern.Status == (byte)Business.Intern.InternBL.enStatus.separated)
            {
               return StatusCode(403, new { message = "Stajyer hesabınız aktif değil " });
            }

            return Ok(intern);
        }
    }
}