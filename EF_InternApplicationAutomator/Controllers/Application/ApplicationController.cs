using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.Business.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF_InternApplicationAutomator.API.Controllers.Application
{
    [Route("api/Application")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        ApplicationBL _application;
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        private readonly string _uploadDirectory;
        public ApplicationController(ApplicationBL application, IWebHostEnvironment env)
        {
            _application = application;
            _uploadDirectory = Path.Combine(env.ContentRootPath, "Uploads");
        }


        //add herkese açık
        [HttpPost("Apply")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AddNewApplication([FromForm] ApplicationCreateSecure newApplicationDTO)
        {
            if (newApplicationDTO == null || newApplicationDTO.PersonInfo == null)
            {
                return BadRequest("Invalid Application data.");
            }
            var cv = newApplicationDTO.CV;

            // 1) Dosya geldi mi?
            if (cv == null || cv.Length == 0)
            {
                return BadRequest("Dosya seçilmedi");
            }
            // 2) Boyut (Length bayt cinsindendir)
            if (cv.Length > MaxFileSizeBytes)
            {
                return BadRequest("Dosya en fazla 5 MB olabilir");
            }

            // 3) Uzantı .pdf mi?
            if (!string.Equals(Path.GetExtension(cv.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Sadece PDF dosyası yüklenebilir");
            }

            // 4) İçerik gerçekten PDF mi? Gerçek PDF dosyaları "%PDF-" ile başlar
            byte[] header = new byte[5];
            using (var stream = cv.OpenReadStream())
            {
                int read = stream.Read(header, 0, header.Length);
                if (read < header.Length || Encoding.ASCII.GetString(header) != "%PDF-")
                {
                    return BadRequest("Dosya geçerli bir PDF değil");
                }
            }

            // Dosya adını sunucu üretir: rastgele ad + sabit uzantı
            string fileName = $"{Guid.NewGuid()}.pdf";

            // Klasör varsa bir şey yapmaz, yoksa oluşturur
            Directory.CreateDirectory(_uploadDirectory);

            string fullPath = Path.Combine(_uploadDirectory, fileName);

            // using: iş bitince dosya kapanır, kilitli kalmaz
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                cv.CopyTo(stream);
            }

            int ID = _application.AddApplication(newApplicationDTO, fileName);
            if (ID < 1)
            {
                System.IO.File.Delete(fullPath);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(true);
        }


        [Authorize(Roles ="User")]
        [HttpGet("All")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ApplicationDetailedListDTO>> GetAllApplications()
        {
            List<ApplicationDetailedListDTO> applicationList = _application.GetAllApplications();

            if (applicationList.Count == 0)
            {
                return NotFound("No Data Available in the Table");
            }

            return Ok(applicationList);
        }



        [Authorize(Roles = "User")]
        [HttpGet("{ApplicationID}", Name = "FindApplication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ApplicationResponseDTO> Find(int ApplicationID)
        {
            if (ApplicationID < 1)
            {
                return BadRequest($"Girilen ApplicationID {ApplicationID} geçersiz");
            }

            ApplicationResponseDTO app = _application.Find(ApplicationID);

            if (app == null)
            {
                return NotFound($"No Application With ApplicationID {ApplicationID}");
            }

            return Ok(app);
        }





        //sadece status upadte edilebilir
        [Authorize(Roles = "User")]
        [HttpPut("Update/AppStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApplicationStatusUpdateDTO> UpdateStatus(ApplicationStatusUpdateDTO updatedApplication)
        {
            if (updatedApplication == null || updatedApplication.ApplicationID < 1)
            {
                return BadRequest("Invalid Application data.");
            }
            int? reviewedByUserID = User.GetUserID();
            if (reviewedByUserID == null)
            {
                return Unauthorized();
            }
            if (_application.UpdateStatus(updatedApplication.ApplicationID, reviewedByUserID.Value,(ApplicationBL.enStatus)updatedApplication.status,updatedApplication.Notes) )
            {
                return Ok(updatedApplication);
            }
            else
            {
                return NotFound("Başvurunun durumu (status) güncellenemedi");
            }
        }



        [Authorize(Roles = "User")]
        [HttpPut("Update/AppStatus/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Accepted(int applicationID,string? note)
        {
            ApplicationResponseDTO application = _application.Find(applicationID);
            if (application == null)
            {
                return BadRequest("başvuru bulunamadı");
            }
            if (application.Status != (int)ApplicationBL.enStatus.newApp)
            {
                return BadRequest("başvuru geçerli değil -  kabul edilmiş veya iptal edilmiş");

            }
            int? reviewedByUserID = User.GetUserID();
            if(reviewedByUserID==null)
            {
                return Unauthorized();
            }
            if (_application.ApplicationAccepted(applicationID, reviewedByUserID.Value,note))
            {
                return Ok();
            }
            else
            {

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }




        [Authorize(Roles = "User")]
        [HttpDelete("{id}", Name = "DeleteApplication")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteApplication(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not Accepted ApplicationID {id}");
            }

            if (_application.DeleteApplication(id))
            {
                return NoContent();
            }
            else
            {
                return NotFound($"Application with ApplicationID {id} not found. No rows deleted!");
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("exists/{ApplicationID}")]
        public bool IsUserExists(int ApplicationID)
        {
            if (ApplicationID < 1)
            {
                return false;
            }


            if (_application.IsApplicationExists(ApplicationID))
            {
                return true;
            }
            return false;

        }



        
       // [Authorize(Roles = "User")]
        [HttpGet("{ApplicationID}/CV")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetCV(int ApplicationID)
        {
            if (ApplicationID < 1)
            {
                return BadRequest($"Girdiğiniz başvuru numarası geçersiz - {ApplicationID}");
            }
            if (!_application.IsApplicationExists(ApplicationID))
            {
                return NotFound("Başvuru bulunamadi");
            }
            string? fileName = _application.GetCvPath(ApplicationID);
            if(fileName==null)
            {
                return NotFound("CV bulunamadi");
            }


            string fullPath = Path.Combine(_uploadDirectory, fileName);

            // Kayıt var ama dosya diskte yok
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("CV bulunamadi");
            }

            return PhysicalFile(fullPath, "application/pdf");

        }


    }
}