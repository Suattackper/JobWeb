using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpPost("addcertificate")]
        //[Authorize(Roles = "3,1")] nhiu quyen
        [Authorize(Roles = "1,3")]
        public IActionResult AddỄprience(JobSeekerCertificate e)
        {
            db.JobSeekerCertificates.Add(e);
            db.SaveChanges();

            return Ok("Add certificate success!");
        }
        [HttpPost("updatecertificate")]
        [Authorize(Roles = "1,3")]
        public IActionResult UpdateỄprience(JobSeekerCertificate e)
        {
            JobSeekerCertificate c = db.JobSeekerCertificates.FirstOrDefault(p => p.CertificateId == e.CertificateId);
            if (c == null) return BadRequest("Không tìm thấy thấy thông tin chứng chỉ!");

            c.CertificateName = e.CertificateName;
            c.Organization = e.Organization;
            c.CertificateLink = e.CertificateLink;
            c.Description = e.Description;
            c.CandidateId = e.CandidateId;
            // Cập nhật thời gian chỉnh sửa
            c.IsUpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Ok("Update certificate success!");
        }
        [HttpDelete("deletecertificate/{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult DeleteExperience(Guid id)
        {
            JobSeekerCertificate e = db.JobSeekerCertificates.FirstOrDefault(p => p.CertificateId == id);
            if (e == null) return BadRequest("Không tìm thấy thấy thông tin chứng chỉ!");

            db.JobSeekerCertificates.Remove(e);
            db.SaveChanges();

            return Ok("Delete experience success!");
        }
    }
}
