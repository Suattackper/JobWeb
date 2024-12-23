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
        [HttpGet("getcompanybyid/{id}")]
        public IActionResult GetCompanyById(Guid id)
        {
            JobSeekerEnterprise o = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == id);
            if (o == null) return BadRequest("Not found company");

            o.ViewCount = o.ViewCount + 1;
            db.SaveChanges();

            return Ok(o);
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
        [HttpGet("getpostjobbyid/{id}")]
        public IActionResult GetJobPostById(Guid id)
        {
            JobSeekerJobPosting job = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == id);
            if (job == null) return BadRequest("Not Found JobPost");

            job.ViewCount = job.ViewCount + 1;
            db.SaveChanges();

            return Ok(job);
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
            notification.Title = $"http://localhost:5080/Home/ManageJobPostingAdmin";
            db.JobSeekerNotifications.Add(notification);
            db.SaveChanges();

            EmailService e = new EmailService();
            string title = $"Bạn có 1 bài đăng mới đang chờ duyệt! - JobWeb";
            string url = $"http://localhost:5080/Home/Notification/{admin.Id}";
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
            o.KeyWord = p.KeyWord;
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
        [HttpPost("addjobapply")]
        [Authorize(Roles = "1,3")]
        public IActionResult AddApplyJobPost(JobSeekerJobPostingApply t)
        {
            db.JobSeekerJobPostingApplies.Add(t);
            db.SaveChanges();

            //thong bao den recruiter
            JobSeekerJobPosting jobPosting = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == t.JobPostingId);
            if (jobPosting == null) return BadRequest("Khong tim thay bai dang");
            JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == jobPosting.EnterpriseId);
            if (enterprise == null) return BadRequest("Khong tim thay cong ty");
            JobSeekerRecruiterProfile recruiter = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
            if (recruiter == null) return BadRequest("Khong tim thay nha tuyen dung");
            JobSeekerCandidateProfile candidate = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == t.CandidateId);
            if (candidate == null) return BadRequest("Khong tim thay ung vien");

            EmailService e = new EmailService();
            JobSeekerNotification notification = new JobSeekerNotification();
            notification.Id = Guid.NewGuid().ToString();
            notification.Type = "recruiter_jobpostnewapply";
            notification.Description = $"1 ứng viên mới vừa apply từ bài đăng {jobPosting.JobTitle} của bạn!";
            notification.IdConcern = t.CandidateId;
            notification.IdUserReceive = recruiter.RecruiterId;
            notification.IsCreatedAt = DateTime.Now;
            notification.IsSent = true;
            notification.IsSeen = false;
            notification.Title = $"http://localhost:5080/Home/CandidateDetailRecruiter/{t.CandidateId}?idjob={t.Id}";
            db.JobSeekerNotifications.Add(notification);
            db.SaveChanges();

            string title = $"1 ứng viên mới vừa apply từ bài đăng {jobPosting.JobTitle} của bạn!";
            string url = $"http://localhost:5080/Home/Notification/{recruiter.RecruiterId}";
            string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiter.Fullname},</h2>\r\n    <p>Có vẻ như có 1 ứng viên vừa apply từ bài đăng của bạn, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";
            e.SendEmail(recruiter.Email, title, message);

            //thong bao den candidate
            JobSeekerNotification notification1 = new JobSeekerNotification();
            notification1.Id = Guid.NewGuid().ToString();
            notification1.Type = "candidate_jobpostnewapply";
            notification1.Description = $"Bạn đã ứng tuyển bài đăng {jobPosting.JobTitle} thành công!";
            notification1.IdConcern = jobPosting.Id;
            notification1.IdUserReceive = candidate.CandidateId;
            notification1.IsCreatedAt = DateTime.Now;
            notification1.IsSent = true;
            notification1.IsSeen = false;
            notification1.Title = $"http://localhost:5080/Home/JobDetailHome/{jobPosting.Id}";
            db.JobSeekerNotifications.Add(notification1);
            db.SaveChanges();

            string title1 = $"Bạn đã ứng tuyển bài đăng {jobPosting.JobTitle} thành công!";
            string url1 = $"http://localhost:5080/Home/Notification/{candidate.CandidateId}";
            string message1 = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title1}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {candidate.Fullname},</h2>\r\n    <p>Bạn vừa ứng tuyển thành công, nhà tuyển dụng sẽ sớm thông báo kết quả ứng tuyển đến bạn, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url1}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

            e.SendEmail(candidate.Email, title1, message1);

            return Ok("Add applyjobpost success!");
        }
        [HttpPost("updatejobapply")]
        [Authorize(Roles = "1,2")]
        public IActionResult UpdateApplyJobPost(JobSeekerJobPostingApply t)
        {
            JobSeekerJobPostingApply apply = db.JobSeekerJobPostingApplies.FirstOrDefault(p => p.Id == t.Id);
            if (apply == null) return NotFound("Not Found Apply JobPost!");

            JobSeekerJobPosting jobPosting = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == t.JobPostingId);
            if (jobPosting == null) return BadRequest("Khong tim thay bai dang");

            JobSeekerCandidateProfile candidate = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == apply.CandidateId);
            if (candidate == null) return BadRequest("Khong tim thay ung vien");

            if (apply.StatusCode != t.StatusCode && t.StatusCode == "SC5")
            {
                EmailService e = new EmailService();

                //thong bao den candidate
                JobSeekerNotification notification1 = new JobSeekerNotification();
                notification1.Id = Guid.NewGuid().ToString();
                notification1.Type = "candidate_jobapplyapproved";
                notification1.Description = $"Bạn đã được đánh giá phù hợp từ bài đăng {jobPosting.JobTitle}!";
                notification1.IdConcern = jobPosting.Id;
                notification1.IdUserReceive = candidate.CandidateId;
                notification1.IsCreatedAt = DateTime.Now;
                notification1.IsSent = true;
                notification1.IsSeen = false;
                notification1.Title = $"http://localhost:5080/Home/JobDetailHome/{jobPosting.Id}";
                db.JobSeekerNotifications.Add(notification1);
                db.SaveChanges();

                string title1 = $"Bạn đã được đánh giá phù hợp từ bài đăng {jobPosting.JobTitle}!";
                string url1 = $"http://localhost:5080/Home/Notification/{candidate.CandidateId}";
                string message1 = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title1}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {candidate.Fullname},</h2>\r\n    <p>Một bài đăng mà bạn đã ứng tuyển đã được đánh giá là phù hợp, nhà tuyển dụng sẽ sớm liên hệ đến bạn để trao đổi thêm về công việc, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url1}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                e.SendEmail(candidate.Email, title1, message1);
            }

            apply.CoverLetter = t.CoverLetter;
            apply.ApplyTime = t.ApplyTime;
            apply.JobPostingId = t.JobPostingId;
            apply.CandidateId = t.CandidateId;
            apply.StatusCode = t.StatusCode;

            db.SaveChanges();

            return Ok("update applyjobpost success!");
        }
        [HttpDelete("deletejobapply/{id}")]
        [Authorize(Roles = "1,2")]
        public IActionResult DeleteApplyJobPost(int id)
        {
            JobSeekerJobPostingApply apply = db.JobSeekerJobPostingApplies.FirstOrDefault(p => p.Id == id);
            if (apply == null) return NotFound("Not Found Apply JobPost!");

            db.JobSeekerJobPostingApplies.Remove(apply);

            db.SaveChanges();

            return Ok("delete applyjobpost success!");
        }
        [HttpPost("addsavejobpost")]
        [Authorize(Roles = "1,3")]
        public IActionResult AddSaveJobPost(JobSeekerSavedJobPosting t)
        {
            db.JobSeekerSavedJobPostings.Add(t);
            db.SaveChanges();

            //thong bao den recruiter
            JobSeekerJobPosting jobPosting = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == t.JobPostingId);
            if (jobPosting == null) return BadRequest("Khong tim thay bai dang");
            JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == jobPosting.EnterpriseId);
            if (enterprise == null) return BadRequest("Khong tim thay cong ty");
            JobSeekerRecruiterProfile recruiter = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
            if (recruiter == null) return BadRequest("Khong tim thay nha tuyen dung");

            EmailService e = new EmailService();
            JobSeekerNotification notification = new JobSeekerNotification();
            notification.Id = Guid.NewGuid().ToString();
            notification.Type = "recruiter_jobpostnewsave";
            notification.Description = $"1 ứng viên mới vừa lưu bài đăng {jobPosting.JobTitle} của bạn!";
            notification.IdConcern = t.CandidateId;
            notification.IdUserReceive = recruiter.RecruiterId;
            notification.IsCreatedAt = DateTime.Now;
            notification.IsSent = true;
            notification.IsSeen = false;
            notification.Title = $"http://localhost:5080/Home/CandidateDetailRecruiter/{t.CandidateId}?idjob={jobPosting.Id}";
            db.JobSeekerNotifications.Add(notification);
            db.SaveChanges();

            string title = $"1 ứng viên mới vừa lưu bài đăng {jobPosting.JobTitle} của bạn!";
            string url = $"http://localhost:5080/Home/Notification/{recruiter.RecruiterId}";
            string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiter.Fullname},</h2>\r\n    <p>Có vẻ như có 1 ứng viên vừa lưu bài đăng của bạn, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

            e.SendEmail(recruiter.Email, title, message);
            return Ok("Add savejobpost success!");
        }
        [HttpDelete("deletesavejobpost/{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult DeleteSaveJobPost(int id)
        {
            JobSeekerSavedJobPosting e = db.JobSeekerSavedJobPostings.FirstOrDefault(p => p.Id == id);
            if (e == null) return BadRequest("Không tìm thấy thấy thông tin save job!");

            db.JobSeekerSavedJobPostings.Remove(e);
            db.SaveChanges();

            return Ok("Delete savejob success!");
        }
        [HttpPost("addcompanfollow")]
        [Authorize(Roles = "1,3")]
        public IActionResult AddCompanyFollow(JobSeekerEnterpriseFollowed t)
        {
            db.JobSeekerEnterpriseFolloweds.Add(t);
            db.SaveChanges();

            //thong bao den recruiter
            JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == t.EnterpriseId);
            if (enterprise == null) return BadRequest("Khong tim thay cong ty");
            JobSeekerRecruiterProfile recruiter = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
            if (recruiter == null) return BadRequest("Khong tim thay nha tuyen dung");

            EmailService e = new EmailService();
            JobSeekerNotification notification = new JobSeekerNotification();
            notification.Id = Guid.NewGuid().ToString();
            notification.Type = "recruiter_companynewfollow";
            notification.Description = $"1 ứng viên mới vừa theo dõi công ty {enterprise.FullCompanyName} mà bạn đã đăng ký!";
            notification.IdConcern = t.CandidateId;
            notification.IdUserReceive = recruiter.RecruiterId;
            notification.IsCreatedAt = DateTime.Now;
            notification.IsSent = true;
            notification.IsSeen = false;
            notification.Title = $"http://localhost:5080/Home/CandidateDetailRecruiter/{t.CandidateId}";
            db.JobSeekerNotifications.Add(notification);
            db.SaveChanges();

            string title = $"1 ứng viên mới vừa theo dõi công ty {enterprise.FullCompanyName} mà bạn đã đăng ký!";
            string url = $"http://localhost:5080/Home/Notification/{recruiter.RecruiterId}";
            string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiter.Fullname},</h2>\r\n    <p>Có vẻ như có 1 ứng viên vừa theo dõi công ty bạn đã đăng ký, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

            e.SendEmail(recruiter.Email, title, message);
            return Ok("Add companyfollow success!");
        }
        [HttpDelete("deletecompanyfollow/{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult DeleteCompanyFollow(int id)
        {
            JobSeekerEnterpriseFollowed e = db.JobSeekerEnterpriseFolloweds.FirstOrDefault(p => p.Id == id);
            if (e == null) return BadRequest("Không tìm thấy thấy thông tin company!");

            db.JobSeekerEnterpriseFolloweds.Remove(e);
            db.SaveChanges();

            return Ok("Delete companyfollow success!");
        }
    }
}
