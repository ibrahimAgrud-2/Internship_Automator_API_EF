using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.User;

namespace EF_InternApplicationAutomator.API.Controllers.User
{


    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        UserBL _User; 
        PersonBL _Person;

        public UserController(UserBL u,PersonBL p)
        {
            _User = u;
            _Person = p;
        }

        [Authorize(Roles ="User")]
        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<UserListResponseDTO>> GetAllUser()
        {

            List<UserListResponseDTO> StudentList = _User.GetAllUser();
            return Ok(StudentList);
        }


        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<int> AddNewUser(UserCreateDTO createDTO)
        {

            if(createDTO==null)
            {
                return BadRequest("Girilen veriler doğru değil");
            }

            if(string.IsNullOrEmpty(createDTO.UserName)||_User.IsUserExists(createDTO.UserName))
            {
                return BadRequest("Kullanici ismi geçerli değil veya zaten kullanılıyor");
            }
           
            int ID = _User.AddUser(createDTO);

            if (ID < 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(ID);

        }







        [Authorize(Roles ="User")]
        [HttpDelete("{userID}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteUser(int userID)
        {
            if (userID < 1)
            {
                return BadRequest($"User ID Geçerli değil {userID}");
            }

            if (!_User.IsUserExists(userID))
            {
                return NotFound($"User ID bulunamadi {userID}");
            }

            if (_User.DeleteUser(userID))

                return NoContent();
            else
                return StatusCode(StatusCodes.Status500InternalServerError);
        }


        
        [Authorize(Roles = "User")]
        [HttpPut("{userID}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<bool> UpdateUser(int userID, UserCreateDTO userUpdate)
        {


            if (userID < 1 || userUpdate == null)
            {
                return BadRequest("Girilen veriler doğru değil");
            }
           
            if (!_User.IsUserExists(userID))
            {
                return NotFound($"Kullanıcı bulunamadı {userID}");
            }
            UserResponseDTO user = _User.Find(userID);
            PersonResponseSDTO? person = _Person.Find(user.PersonID);
            if(person==null)
            {
                return NotFound($"Kullanıcıya ait kişi bilgileri bulunamadı - kullanici ID: {userID}");
            }
            if (_User.UpdateUser(userID,person.PersonID, userUpdate) > 0)
            {
                return Ok();
            }
            else
            {

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }



    }


}
