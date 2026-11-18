using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class PersonResponseWithIDsDTO:PersonResponseSDTO
    {

        [Required]
        public int? InternID { get; set; }
        public int? UserID { get; set; }
        public PersonResponseWithIDsDTO(int personID,string firstName, string lastName, string email,
                string phone, string address, string imagePath,string role,bool isActive,string passwordHash,int? interID,int? userID,bool mustChangePassword)
            :base(personID,firstName,lastName,email,phone,address,imagePath,role,isActive,passwordHash,mustChangePassword)
        {
            this.InternID = interID;
            this.UserID = userID;
        }
    }
}
