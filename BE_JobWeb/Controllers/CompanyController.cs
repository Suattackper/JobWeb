using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpPost("addcompany")]
        public IActionResult AddCompany(JobSeekerEnterprise p)
        {
            //Thêm công ty
            db.JobSeekerEnterprises.Add(p);
            db.SaveChanges();

            return Ok("Add company success!");
        }
        [HttpPost("updatecompany")]
        //[Authorize(Roles = "3,1")] nhiu quyen
        [Authorize(Roles = "1,2")]
        public IActionResult UpdateCompany(JobSeekerEnterprise e)
        {
            //Cật nhật công ty
            JobSeekerEnterprise jobSeekerEnterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == e.EnterpriseId);

            if (jobSeekerEnterprise == null) return BadRequest("Không tìm thấy hồ sơ công ty!");

            jobSeekerEnterprise.FullCompanyName = e.FullCompanyName;
            jobSeekerEnterprise.JobFieldId = e.JobFieldId;
            jobSeekerEnterprise.EnterpriseSize = e.EnterpriseSize;
            jobSeekerEnterprise.TaxCode = e.TaxCode;
            jobSeekerEnterprise.FoundedDate = e.FoundedDate;
            jobSeekerEnterprise.CompanyEmail = e.CompanyEmail;
            jobSeekerEnterprise.CompanyPhoneContact = e.CompanyPhoneContact;
            jobSeekerEnterprise.Descriptions = e.Descriptions;
            jobSeekerEnterprise.LogoUrl = e.LogoUrl;
            jobSeekerEnterprise.CoverImgUrl = e.CoverImgUrl;
            jobSeekerEnterprise.FacebookUrl = e.FacebookUrl;
            jobSeekerEnterprise.LinkedinUrl = e.LinkedinUrl;
            jobSeekerEnterprise.WebsiteUrl = e.WebsiteUrl;
            jobSeekerEnterprise.City = e.City;
            jobSeekerEnterprise.District = e.District;
            jobSeekerEnterprise.Ward = e.Ward;
            jobSeekerEnterprise.Address = e.Address;
            jobSeekerEnterprise.IsUpdatedAt = DateTime.UtcNow; // Ghi nhận thời gian cập nhật

            db.SaveChanges();

            return Ok("Update company success!");
        }
        [HttpGet("getlistpostjob")]
        public IActionResult GetListJobpost()
        {
            return Ok(db.JobSeekerJobPostings.OrderByDescending(p => p.IsCreatedAt).ToList());
        }
        [HttpPost("addpostjob")]
        [Authorize(Roles = "1,2")]
        public IActionResult AddJobpost(JobSeekerJobPosting p)
        {
            //Thêm công ty
            db.JobSeekerJobPostings.Add(p);
            db.SaveChanges();

            return Ok("Add job post success!");
        }
        [HttpPost("updatepostjob")]
        [Authorize(Roles = "1,2")]
        public IActionResult UpdateJobpost(JobSeekerJobPosting p)
        {
            //Thêm công ty
            JobSeekerJobPosting o = db.JobSeekerJobPostings.FirstOrDefault(pr => pr.Id == p.Id);
            if(o == null) return BadRequest("Không tìm thấy JobPosting!");

            o.JobTitle = p.JobTitle;
            o.Quantity = p.Quantity;
            o.JobCategoryId = p.JobCategoryId;
            o.WorkingType = p.WorkingType;
            o.ExpRequirement = p.ExpRequirement;
            o.JobLevelCode = p.JobLevelCode;
            o.SalaryMin = p.SalaryMin;
            o.SalaryMax = p.SalaryMax;
            o.AcademicLevel = p.AcademicLevel;
            o.GenderRequire = p.GenderRequire;
            o.Province = p.Province;
            o.District = p.District;
            o.Ward = p.Ward;
            o.ExpiredTime = p.ExpiredTime;
            o.Address = p.Address;
            o.JobDesc = p.JobDesc;
            o.JobRequirement = p.JobRequirement;
            o.BenefitEnjoyed = p.BenefitEnjoyed;
            o.StatusCode = p.StatusCode;
            o.IsUpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            return Ok("Add job post success!");
        }
        [HttpDelete("deletepostjob/{id}")]
        [Authorize(Roles = "1,2")]
        public IActionResult DeleteJobpost(Guid id)
        {
            JobSeekerJobPosting e = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == id);
            if (e == null) return BadRequest("Không tìm thấy thấy thông tin job post!");

            db.JobSeekerJobPostings.Remove(e);
            db.SaveChanges();

            return Ok("Delete job post success!");
        }
        [HttpGet("getlistjobapply")]
        public IActionResult GetListJobApply()
        {
            return Ok(db.JobSeekerJobPostingApplies.ToList());
        }
    }
}
