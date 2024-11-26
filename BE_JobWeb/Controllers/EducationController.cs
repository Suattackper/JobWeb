using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpPost("addeducation")]
        //[Authorize(Roles = "3,1")] nhiu quyen
        [Authorize(Roles = "1,3")]
        public IActionResult AddỄprience(JobSeekerEducationDetail e)
        {
            db.JobSeekerEducationDetails.Add(e);
            db.SaveChanges();

            return Ok("Add education success!");
        }
        [HttpPost("updateeducation")]
        [Authorize(Roles = "1,3")]
        public IActionResult UpdateỄprience(JobSeekerEducationDetail e)
        {
            JobSeekerEducationDetail c = db.JobSeekerEducationDetails.FirstOrDefault(p => p.EducationId == e.EducationId);
            if (c == null) return BadRequest("Không tìm thấy thấy thông tin giáo dục!");

            c.SchoolName = e.SchoolName;
            c.Major = e.Major;
            c.Degree = e.Degree;
            c.Description = e.Description;
            c.StartDate = e.StartDate;
            c.EndDate = e.EndDate;
            c.CandidateId = e.CandidateId;
            // Cập nhật thời gian chỉnh sửa
            c.IsUpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            return Ok("Update education success!");
        }
        [HttpDelete("deleteeducation/{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult DeleteExperience(Guid id)
        {
            JobSeekerEducationDetail e = db.JobSeekerEducationDetails.FirstOrDefault(p => p.EducationId == id);
            if (e == null) return BadRequest("Không tìm thấy thấy thông tin học vấn!");

            db.JobSeekerEducationDetails.Remove(e);
            db.SaveChanges();

            return Ok("Delete education success!");
        }
    }
}
