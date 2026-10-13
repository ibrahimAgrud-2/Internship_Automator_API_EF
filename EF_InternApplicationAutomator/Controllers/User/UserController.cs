
using EF_InternApplicationAutomator.Business;
using EF_InternApplicationAutomator.Business.User;
using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Intern;
using Shared.User;
using System;

namespace EF_InternApplicationAutomator.API.Controllers.User
    {
        [Route("api/User")]
        [ApiController]
        public class UserController: ControllerBase
        {
            UserBL _User;
            public UserController(UserBL u)
            {
                _User = u;
            }

            [HttpGet("All")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public ActionResult<IEnumerable<UserResponseDTO>> GetAllUser()
            {

                List<UserResponseDTO> StudentList = _User.GetAllUser();
                if (StudentList.Count == 0)
                {
                    return NotFound("No Data Available in the Table");
                }

                return Ok(StudentList);
            }

        [HttpGet("{ID}", Name = "FindUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserResponseDTO> Find(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not Accepted ID {ID}");
            }
            UserResponseDTO p = _User.Find(ID);
            if (p == null)
            {
                return NotFound($"No Person With ID {ID}");
            }
            return Ok(p);

        }


        //here we use http put method for update
        [HttpPut("{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserResponseDTO> UpdateUser(int id, UserCreateDTO userResponseDTO)
        {
            if (id < 1 || userResponseDTO == null)
            {
                return BadRequest("Invalid user data.");
            }



            if (!_User.IsUserExists(id))
            {
                return NotFound($"No User With ID {id}");
            }


            if (_User.UpdateUser(id, userResponseDTO) > 0)
            {
                return Ok(userResponseDTO);
            }
            else
            {

                return BadRequest("Cloud Not Updated");
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<UserResponseDTO> AddNewUser(UserCreateDTO newUser)
        {

            if (newUser == null)
            {
                return BadRequest("Invalid User data.");
            }
            int ID = _User.AddUser(newUser);

            if (ID < 1)
            {
                //TODO: use proper status code instead of bad request
                return BadRequest("Could no added");
            }

            return CreatedAtRoute("FindUser", new { id = ID }, newUser);

        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteUser(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (!_User.IsUserExists(id))
            {
                return NotFound($"No User With ID {id}");
            }

            if (_User.DeleteUser(id))

                return NoContent();
            else
                return NotFound($"Delete Process terminated. No rows deleted!");
        }


        [HttpGet("exists/{userID}")]
        public bool IsUserExists(int userID)
        {
            if (userID < 1)
            {
                return false;
            }
      

            if (_User.IsUserExists(userID))
            {
                return true;
            }
            return false;
              
        }



        [HttpPost("UserLogin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserLoginResponseDTO> UserLogin(UserLoginDTO loginDTO)
        {
            if (string.IsNullOrEmpty(loginDTO.UserName) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest($"Kullanıcı Adı ve şifre zorunludur.");
            }

            UserLoginResponseDTO User = _User.FindByPasswordAndUserName(loginDTO.UserName, loginDTO.Password);
            if (User == null)
            {
                return NotFound("Kullanıcı Adı veya şifre hatalı");
            }
            else if (!User.IsActive)
            {
                return StatusCode(403, new { message = "Hesabınız aktif değil"});
            }

            return Ok(User);
        }
    }
}
