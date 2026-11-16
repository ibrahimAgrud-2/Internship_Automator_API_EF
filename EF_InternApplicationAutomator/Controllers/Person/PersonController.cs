
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.User;
using System;
using static EF_InternApplicationAutomator.Business.PersonBL;

namespace EF_InternApplicationAutomator.API.Controllers
{
    //Direk perosn üzeridne işlem olmadığı için API'Da gerek yok. İşlem ya user'dan ya Inrtenr'den olur

    [Route("api/Person")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        //PL ile BL Arasında iletişimi obje üzerine sağlayabilmek için objeyi const ile dolduruyoruz.
        PersonBL _person;
        public PersonController(PersonBL p)
        {
            _person = p;
        }



        //[HttpGet("All")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult<IEnumerable<PersonResponseSDTO>> GetAllPeople()
        //{

        //    List<PersonResponseSDTO> StudentList = _person.GetAllPeople();
        //    if (StudentList.Count == 0)
        //    {
        //        return NotFound("No Data Available in the Table");
        //    }

        //    return Ok(StudentList);
        //}


        //[HttpGet("{ID}", Name = "FindPerson")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public ActionResult<PersonResponseSDTO> Find(int ID)
        //{
        //    if (ID < 1)
        //    {
        //        return BadRequest($"Not Accepted ID {ID}");
        //    }
        //    PersonResponseSDTO p = _person.Find(ID);
        //    if (p == null)
        //    {
        //        return NotFound($"No Person With ID {ID}");
        //    }
        //    return Ok(p);

        //}


        ////here we use http put method for update
        //[HttpPut("{id}", Name = "UpdatePerson")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult<PersonResponseSDTO> UpdatePerson(int id, PersonCreateSDTO updatedPerson)
        //{
        //    if (id < 1 || updatedPerson == null)
        //    {
        //        return BadRequest("Invalid Person data.");
        //    }


        //    //TODO: check if exists


        //    if (_person.UpdatePerson(id, updatedPerson) > 0)
        //    {
        //        return Ok(updatedPerson);
        //    }
        //    else
        //    {
        //        //TODO: set a return status code for fail to update instance
        //        return NotFound("Cloud Not Updated");
        //    }

        //}


        ////here we use HttpDelete method
        //[HttpDelete("{id}", Name = "DeletePerson")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult DeletePerson(int id)
        //{
        //    if (id < 1)
        //    {
        //        return BadRequest($"Not accepted ID {id}");
        //    }

        //    // var Person = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
        //    // StudentDataSimulation.StudentsList.Remove(Person);

        //    if (_person.DeletePerson(id))

        //        return NoContent();
        //    else
        //        return NotFound($"Person with ID {id} not found. no rows deleted!");
        //}


        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserLoginResponseDTO> Login(LoginDTO loginDTO)
        {
            if (loginDTO==null||string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest($"email  ve şifre zorunludur");
            }

            PersonResponseSDTO person = _person.FindByEmail(loginDTO.Email);
            if (person == null)
            {
                return NotFound("email  veya şifre hatalı");
            }
            else if (!person.IsActive)
            {
                return StatusCode(403, new { message = "Hesabınız aktif değil" });
            }

            return Ok(person);
        }
    }


}
