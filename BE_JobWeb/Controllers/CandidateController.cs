using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpGet("getlistcandidate")]
        public IActionResult GetListCandidate()
        {
            return Ok(db.JobSeekerCandidateProfiles.OrderByDescending(p => p.IsCreatedAt).ToList());
        }
    }
}
