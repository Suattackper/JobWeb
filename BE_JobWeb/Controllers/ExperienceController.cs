using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExperienceController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpPost("addexperience")]
        //[Authorize(Roles = "3,1")] nhiu quyen
        [Authorize(Roles = "1,3")]
        public IActionResult AddExperience(JobSeekerWorkingExperience e)
        {
            db.JobSeekerWorkingExperiences.Add(e);
            db.SaveChanges();

            return Ok("Add experience success!");
        }
        [HttpPost("updateexperience")]
        [Authorize(Roles = "1,3")]
        public IActionResult UpdateExperience(JobSeekerWorkingExperience e)
        {
            JobSeekerWorkingExperience c = db.JobSeekerWorkingExperiences.FirstOrDefault(p => p.WorkingExpId == e.WorkingExpId);
            if(c == null) return BadRequest("Không tìm thấy thấy thông tin kinh nghiệm!");

            c.JobTitle = e.JobTitle;
            c.CompanyName = e.CompanyName;
            c.StartDate = e.StartDate;
            c.EndDate = e.EndDate;
            c.Description = e.Description;
            c.CandidateId = e.CandidateId;
            // Cập nhật thời gian chỉnh sửa
            c.IsUpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Ok("Update experience success!");
        }
        [HttpDelete("deleteexperience/{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult DeleteExperience(Guid id)
        {
            JobSeekerWorkingExperience e = db.JobSeekerWorkingExperiences.FirstOrDefault(p => p.WorkingExpId == id);
            if (e == null) return BadRequest("Không tìm thấy thấy thông tin kinh nghiệm!");

            db.JobSeekerWorkingExperiences.Remove(e);
            db.SaveChanges();

            return Ok("Delete experience success!");
        }
    }
}
