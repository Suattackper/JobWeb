using BE_JobWeb.Others;
using Data_JobWeb.Dtos;
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
            jobSeekerEnterprise.IsCensorship = e.IsCensorship;
            jobSeekerEnterprise.IsUpdatedAt = DateTime.Now; // Ghi nhận thời gian cập nhật

            db.SaveChanges();

            return Ok("Update company success!");
        }
        [HttpGet("getlistpostjob")]
        public IActionResult GetListJobpost()
        {
            return Ok(db.JobSeekerJobPostings.OrderByDescending(p => p.IsCreatedAt).ToList());
        }
        [HttpGet("getlistenterprise")]
        public IActionResult GetListEnterprise()
        {
            return Ok(db.JobSeekerEnterprises.OrderByDescending(p => p.IsCreatedAt).ToList());
        }
        [HttpPost("addpostjob")]
        [Authorize(Roles = "1,2")]
        public IActionResult AddJobpost(JobSeekerJobPosting p)
        {
            db.JobSeekerJobPostings.Add(p);
            db.SaveChanges();

            JobSeekerUserLoginDatum admin = db.JobSeekerUserLoginData.FirstOrDefault(p => p.RoleId == 1);
            if (admin == null) return Ok("Add job post success, create notification for admin fail!");

            JobSeekerNotification notification = new JobSeekerNotification();
            notification.Id = Guid.NewGuid().ToString();
            notification.Type = "admin_newpostjob";
            notification.Description = $"Bạn có 1 bài đăng mới đang chờ duyệt!";
            notification.IsSeen = false;
            notification.IsCreatedAt = DateTime.Now;
            notification.IdConcern = p.Id;
            db.JobSeekerNotifications.Add(notification);
            db.SaveChanges();

            EmailService e = new EmailService();
            string title = $"Bạn có 1 bài đăng mới đang chờ duyệt! - JobWeb";
            string url = $"http://localhost:5281/api/Account/notification/{admin.Id}";
            string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {admin.FullName},</h2>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

            e.SendEmail(admin.Email, title, message);

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
            o.IsUpdatedAt = DateTime.Now;

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
