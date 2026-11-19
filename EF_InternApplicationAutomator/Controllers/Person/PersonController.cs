
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared;
using Shared.Common;
using Shared.User;
using System;
using System.Security.Claims;
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
        TokenService _TokenService;
        public PersonController(PersonBL p, TokenService tokenService)
        {
            _person = p;
            _TokenService = tokenService;
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
        public ActionResult<LoginResponseDTO> Login(LoginDTO loginDTO)
        {
            if (loginDTO==null||string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest($"email  ve şifre zorunludur");
            }

            PersonResponseWithIDsDTO? person = _person.Find(loginDTO.Email);
            if (person == null)
            {
                return NotFound("email  veya şifre hatalı");
            }
            else if(!CommonTools.VerifyPassword(loginDTO.Password, person.Password))
            {
                return NotFound("email  veya şifre hatalı");
            }
            else if (!person.IsActive)
            {
                return StatusCode(403, new { message = "Hesabınız aktif değil" });
            }


            if (person.Role.ToLower()=="user"&& person.UserID<1)
            {
                return NotFound("Hesabiniza erişilemedi");
            }
            else if (person.Role.ToLower() == "intern"&&person.InternID < 1)
            {
                return NotFound("Hesabiniza erişilemedi");
            }

            string token = _TokenService.CreateToken(person.PersonID.ToString(),person.Role,person.UserID.ToString(),person.InternID.ToString());


                return Ok(new LoginResponseDTO(token, person.PersonID, person.Email, person.FirstName, person.LastName, person.Role, person.InternID, person.UserID,person.MustChangePassword));
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
       
           if(changePasswordDTO == null||string.IsNullOrEmpty(changePasswordDTO.NewPassword)|| string.IsNullOrEmpty(changePasswordDTO.OldPassword))
            {
                return BadRequest("Verilen bilgiler geçersiz");
            }
            int? personID = User.GetPersonID();

            if(personID==null)
            {
                return Unauthorized();
            }
            if(!_person.IsPersonExists(personID.Value))
            {
                return NotFound("Bu hesap sistemde bulunmadığı için şifre değiştirilemez");
            }
            if (!_person.IsPersonActive(personID.Value))
            {
                return NotFound("Bu hesap aktif olmadı için şifre değiştirilemez");
            }
            PersonResponseSDTO person = _person.Find(personID.Value);
            if (!CommonTools.VerifyPassword(changePasswordDTO.OldPassword, person.Password))
            {
                return NotFound("Verdiğiniz eski şifre doğru değil");
            }

            if (_person.ChangePassword(personID.Value,changePasswordDTO.NewPassword,false))
            {
                return Ok(true);
            }

              return   StatusCode(StatusCodes.Status500InternalServerError);
        }

    

    }
}
