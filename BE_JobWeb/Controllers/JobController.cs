using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpGet("getjobfield")]
        public IActionResult GetJobfield()
        {
            return Ok(db.JobSeekerJobFields.ToList());
        }
        [HttpPost("addjobfield")]
        [Authorize(Roles = "1")]
        public IActionResult AddJobfield(JobSeekerJobField e)
        {
            db.JobSeekerJobFields.Add(e);
            db.SaveChanges();

            return Ok("Add jobfield success!");
        }
        [HttpPost("updatejobfield")]
        [Authorize(Roles = "1")]
        public IActionResult UpdateJobfield(JobSeekerJobField e)
        {
            JobSeekerJobField c = db.JobSeekerJobFields.FirstOrDefault(p => p.JobFieldId == e.JobFieldId);
            if (c == null) return BadRequest("Không tìm thấy hồ sơ lĩnh vực này!");

            c.JobFieldName = e.JobFieldName;

            db.SaveChanges();

            return Ok("Update jobfield success!");
        }
        [HttpDelete("deletejobfield/{id}")]
        [Authorize(Roles = "1")]
        public IActionResult DeleteJobfield(int id)
        {
            JobSeekerJobField c = db.JobSeekerJobFields.FirstOrDefault(p => p.JobFieldId == id);
            if (c == null) return BadRequest("Không tìm thấy hồ sơ lĩnh vực này!");

            db.JobSeekerJobFields.Remove(c);

            db.SaveChanges();

            return Ok("Delete jobfield success!");
        }
        [HttpGet("getjobcategory")]
        public IActionResult GetJobcategory()
        {
            return Ok(db.JobSeekerJobCategories.ToList());
        }
        [HttpPost("addjobcategory")]
        [Authorize(Roles = "1")]
        public IActionResult AddJobcategory(JobSeekerJobCategory e)
        {
            db.JobSeekerJobCategories.Add(e);
            db.SaveChanges();

            return Ok("Add category success!");
        }
        [HttpPost("updatejobcategory")]
        [Authorize(Roles = "1")]
        public IActionResult UpdateJobcategory(JobSeekerJobCategory e)
        {
            JobSeekerJobCategory c = db.JobSeekerJobCategories.FirstOrDefault(p => p.Id == e.Id);

            if (c == null) return BadRequest("Không tìm thấy hồ sơ ngành nghề này!");

            c.JobCategoryName = e.JobCategoryName;
            c.IsUpdatedAt = DateTime.Now; // Ghi nhận thời gian cập nhật

            db.SaveChanges();

            return Ok("Update category success!");
        }
        [HttpDelete("deletejobcategory/{id}")]
        [Authorize(Roles = "1")]
        public IActionResult DeleteJobcategory(int id)
        {
            JobSeekerJobCategory c = db.JobSeekerJobCategories.FirstOrDefault(p => p.Id == id);
            if (c == null) return BadRequest("Không tìm thấy hồ sơ ngành nghề này!");

            db.JobSeekerJobCategories.Remove(c);
            db.SaveChanges();

            return Ok("Delete category success!");
        }
        [HttpGet("getjoblevel")]
        public IActionResult GetJoblevel()
        {
            return Ok(db.JobSeekerJobLevels.ToList());
        }
    }
}
