using Data_JobWeb.Entity;
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
        [HttpGet("getjobcategory")]
        public IActionResult GetJobcategory()
        {
            return Ok(db.JobSeekerJobCategories.ToList());
        }
        [HttpGet("getjoblevel")]
        public IActionResult GetJoblevel()
        {
            return Ok(db.JobSeekerJobLevels.ToList());
        }
    }
}
