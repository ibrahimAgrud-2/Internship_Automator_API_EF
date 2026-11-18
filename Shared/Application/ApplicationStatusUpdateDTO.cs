using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application
{
    public class ApplicationStatusUpdateDTO
    {
        public int ApplicationID { get; set; }
        public byte status { get; set; }

        public string? Notes { get; set; }
        public ApplicationStatusUpdateDTO(int applicationID,byte status, string? notes)
        {
            this.ApplicationID = applicationID;
            this.status = status;
            Notes = notes;
        }



    }
}
