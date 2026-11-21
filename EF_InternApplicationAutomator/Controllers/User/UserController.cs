
using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.User;
using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Intern;
using Shared.User;
using System;

namespace EF_InternApplicationAutomator.API.Controllers.User
{


    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        UserBL _User; 
        PersonBL _People;

        public UserController(UserBL u,PersonBL p)
        {
            _User = u;
            _People = p;
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




        //[HttpGet("{ID}", Name = "FindUser")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public ActionResult<UserResponseDTO> Find(int ID)
        //{
        //    if (ID < 1)
        //    {
        //        return BadRequest($"Not Accepted ID {ID}");
        //    }
        //    UserResponseDTO p = _User.Find(ID);
        //    if (p == null)
        //    {
        //        return NotFound($"No Person With ID {ID}");
        //    }
        //    return Ok(p);

        //}


        ////here we use http put method for update
        //[HttpPut("{userID}", Name = "UpdateUser")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult<UserResponseDTO> UpdateUser(int userID, UserCreateDTO userResponseDTO)
        //{
        //    if (userID < 1 || userResponseDTO == null)
        //    {
        //        return BadRequest("Invalid user data.");
        //    }



        //    if (!_User.IsUserExists(userID))
        //    {
        //        return NotFound($"No User With ID {userID}");
        //    }


        //    if (_User.UpdateUser(userID, userResponseDTO) > 0)
        //    {
        //        return Ok(userResponseDTO);
        //    }
        //    else
        //    {

        //        return BadRequest("Cloud Not Updated");
        //    }

        //}



        //[HttpDelete("{userID}", Name = "DeleteUser")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public ActionResult DeleteUser(int userID)
        //{
        //    if (userID < 1)
        //    {
        //        return BadRequest($"Not accepted ID {userID}");
        //    }

        //    if (!_User.IsUserExists(userID))
        //    {
        //        return NotFound($"No User With ID {userID}");
        //    }

        //    if (_User.DeleteUser(userID))

        //        return NoContent();
        //    else
        //        return NotFound($"Delete Process terminated. No rows deleted!");
        //}


        //[HttpGet("exists/{userID}")]
        //public bool IsUserExists(int userID)
        //{
        //    if (userID < 1)
        //    {
        //        return false;
        //    }


        //    if (_User.IsUserExists(userID))
        //    {
        //        return true;
        //    }
        //    return false;

        //}

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
    }


}
