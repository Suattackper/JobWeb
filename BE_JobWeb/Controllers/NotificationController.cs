using BE_JobWeb.Others;
using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpGet("listnotificationadmin")]
        [Authorize(Roles = "1")]
        public IActionResult ListNotificationAdmin()
        {
            return Ok(db.JobSeekerNotifications.Where(p => p.Type.Contains("admin")).ToList());
        }
        [HttpGet("notificationpostjob/{id}")]
        [Authorize(Roles = "1")]
        public IActionResult NotificationPostJob(Guid id)
        {
            EmailService e = new EmailService();
            string check = "Send mail";
            JobSeekerJobPosting jobPosting = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == id);
            if (jobPosting == null) return BadRequest("Không tìm thấy thấy thông tin bài đăng!");

            JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == jobPosting.EnterpriseId);
            if (enterprise == null) return BadRequest("Không tìm thấy thấy thông tin công ty của bài đăng!");

            if(jobPosting.StatusCode == "SC6")
            {
                //Gửi thông báo đến recuiter
                JobSeekerRecruiterProfile recruiterProfile1 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
                if (recruiterProfile1 == null) return BadRequest("Không tìm thấy thấy thông tin nhà tuyển dụng đăng ký công ty!");
                else
                {
                    JobSeekerNotification notification = new JobSeekerNotification();
                    notification.Id = Guid.NewGuid().ToString();
                    notification.Type = "jobpostrejected";
                    notification.Description = $"Bài đăng {jobPosting.JobTitle} đã bị quản trị viên từ chối!";
                    notification.IdConcern = jobPosting.Id;
                    notification.IdUserReceive = recruiterProfile1.RecruiterId;
                    notification.IsCreatedAt = DateTime.Now;
                    notification.IsSent = true;
                    notification.IsSeen = false;
                    db.JobSeekerNotifications.Add(notification);
                    db.SaveChanges();

                    string title = $"Bài đăng {jobPosting.JobTitle} đã bị quản trị viên từ chối!";
                    string url = $"http://localhost:5281/api/Account/notification/{recruiterProfile1.RecruiterId}";
                    string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiterProfile1.Fullname},</h2>\r\n    <p>Có vẻ như chúng tôi đã từ chối bài đăng của bạn, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                    e.SendEmail(recruiterProfile1.Email, title, message);
                    check += " recruiter";
                }
            }
            if(jobPosting.StatusCode == "SC7")
            {
                //Gửi thông báo đến recuiter
                JobSeekerRecruiterProfile recruiterProfile1 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
                if (recruiterProfile1 == null) return BadRequest("Không tìm thấy thấy thông tin nhà tuyển dụng đăng ký công ty!");
                else
                {
                    JobSeekerNotification notification = new JobSeekerNotification();
                    notification.Id = Guid.NewGuid().ToString();
                    notification.Type = "jobpostspendingapproved";
                    notification.Description = $"Bài đăng {jobPosting.JobTitle} đang được quản trị viên kiểm tra!";
                    notification.IdConcern = jobPosting.Id;
                    notification.IdUserReceive = recruiterProfile1.RecruiterId;
                    notification.IsCreatedAt = DateTime.Now;
                    notification.IsSent = true;
                    notification.IsSeen = false;
                    db.JobSeekerNotifications.Add(notification);
                    db.SaveChanges();

                    string title = $"Bài đăng {jobPosting.JobTitle} đang được quản trị viên kiểm tra!";
                    string url = $"http://localhost:5281/api/Account/notification/{recruiterProfile1.RecruiterId}";
                    string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiterProfile1.Fullname},</h2>\r\n    <p>Cảm ơn bạn đã sử dụng website của chúng tôi, kết quả kiểm tra sẽ sớm có thông báo đên bạn, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                    e.SendEmail(recruiterProfile1.Email, title, message);

                    return Ok("Gửi mail kiểm tra bài đăng thành công");
                }
            }

            //Gửi thông báo đến recuiter
            JobSeekerRecruiterProfile recruiterProfile = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
            if (recruiterProfile == null) return BadRequest("Không tìm thấy thấy thông tin nhà tuyển dụng đăng ký công ty!");
            else
            {
                JobSeekerNotification notification = new JobSeekerNotification();
                notification.Id = Guid.NewGuid().ToString();
                notification.Type = "jobpostapproved";
                notification.Description = $"Bài đăng {jobPosting.JobTitle} đã được quản trị viên duyệt, giờ đây nó có thể được hiển thị tại JobWeb!";
                notification.IdConcern = jobPosting.Id;
                notification.IdUserReceive = recruiterProfile.RecruiterId;
                notification.IsCreatedAt = DateTime.Now;
                notification.IsSent = true;
                notification.IsSeen = false;
                db.JobSeekerNotifications.Add(notification);
                db.SaveChanges();

                string title = $"Bài đăng {jobPosting.JobTitle} đã được quản trị viên duyệt, giờ đây nó có thể được hiển thị tại JobWeb - JobWeb";
                string url = $"http://localhost:5281/api/Account/notification/{recruiterProfile.RecruiterId}";
                string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiterProfile.Fullname},</h2>\r\n    <p>Cảm ơn bạn đã sử dụng website của chúng tôi.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                e.SendEmail(recruiterProfile.Email, title, message);
                check += " recruiter";
            }

            //Thông báo đến candidate theo dõi công ty
            List<JobSeekerEnterpriseFollowed> enterpriseFolloweds = db.JobSeekerEnterpriseFolloweds.Where(p => p.EnterpriseId == enterprise.EnterpriseId).ToList();
            if (enterpriseFolloweds.Count > 0)
            {
                foreach (JobSeekerEnterpriseFollowed i in enterpriseFolloweds)
                {
                    JobSeekerCandidateProfile candidateProfile = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == i.CandidateId);
                    if (candidateProfile != null)
                    {
                        JobSeekerNotification notification = new JobSeekerNotification();
                        notification.Id = Guid.NewGuid().ToString();
                        notification.Type = "newjobpost";
                        notification.Description = $"1 bài đăng mới đã được đăng tuyển từ công ty {enterprise.FullCompanyName}!";
                        notification.IdConcern = jobPosting.Id;
                        notification.IdUserReceive = candidateProfile.CandidateId;
                        notification.IsCreatedAt = DateTime.Now;
                        notification.IsSent = true;
                        notification.IsSeen = false;
                        db.JobSeekerNotifications.Add(notification);
                        db.SaveChanges();

                        string title = $"Vừa có 1 job mới từ công ty bạn đã theo dõi - JobWeb";
                        string url = $"http://localhost:5281/api/Account/notification/{candidateProfile.CandidateId}";
                        string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {candidateProfile.Fullname},</h2>\r\n    <p>Công ty {enterprise.FullCompanyName} vừa đăng 1 bài đăng tuyển mới.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                        e.SendEmail(candidateProfile.Email, title, message);
                        check += ", candidate followed";
                    }
                }
            }

            //Thông báo đến candidate đăng ký tìm kiếm theo từ khóa
            List<JobSeekerNotificationType> noptificationtypes = db.JobSeekerNotificationTypes.Where(p => p.TypeName == "signupnotification" && jobPosting.JobTitle.Contains(p.Description)).ToList();
            if (noptificationtypes.Count > 0)
            {
                foreach (JobSeekerNotificationType i in noptificationtypes)
                {
                    JobSeekerCandidateProfile candidateProfile = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == i.IdUser);
                    if (candidateProfile != null)
                    {
                        JobSeekerNotification notification = new JobSeekerNotification();
                        notification.Id = Guid.NewGuid().ToString();
                        notification.Type = "newjobpost";
                        notification.Description = $"'{i.Description}': 1 việc làm mới phù hợp với bạn!";
                        notification.IdConcern = jobPosting.Id;
                        notification.IdUserReceive = candidateProfile.CandidateId;
                        notification.IsCreatedAt = DateTime.Now;
                        notification.IsSent = true;
                        notification.IsSeen = false;
                        db.JobSeekerNotifications.Add(notification);
                        db.SaveChanges();

                        string title = $"'{i.Description}': 1 việc làm mới phù hợp với bạn - JobWeb";
                        string url = $"http://localhost:5281/api/Account/notification/{candidateProfile.CandidateId}";
                        string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {candidateProfile.Fullname},</h2>\r\n    <p>Dựa trên cài dặt thông báp cài đặt việc làm của bạn cho từ khóa {i.Description}. Chúng tôi đã tìm thấy 1 việc làm mới phù hợp với bạn</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                        e.SendEmail(candidateProfile.Email, title, message);
                        check += ", candidate sign up with keyword";
                    }
                }
            }
            check += "!";
            return Ok(check);
        }
        [HttpGet("notificationcompany/{id}")]
        [Authorize(Roles = "1")]
        public IActionResult NotificationCompany(Guid id)
        {
            EmailService e = new EmailService();
            string check = "Send mail";

            JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == id);
            if (enterprise == null) return BadRequest("Không tìm thấy thấy thông tin công ty!");

            JobSeekerRecruiterProfile recruiterProfile1 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.EnterpriseId == enterprise.EnterpriseId);
            if (recruiterProfile1 == null) return BadRequest("Không tìm thấy thấy thông tin nhà tuyển dụng đăng ký công ty!");

            if (enterprise.IsCensorship == false)
            {
                JobSeekerNotification notification = new JobSeekerNotification();
                notification.Id = Guid.NewGuid().ToString();
                notification.Type = "companyrejected";
                notification.Description = $"Công ty {enterprise.FullCompanyName} đã bị quản trị viên từ chối!";
                notification.IdConcern = enterprise.EnterpriseId;
                notification.IdUserReceive = recruiterProfile1.RecruiterId;
                notification.IsCreatedAt = DateTime.Now;
                notification.IsSent = true;
                notification.IsSeen = false;
                db.JobSeekerNotifications.Add(notification);
                db.SaveChanges();

                string title = $"Công ty {enterprise.FullCompanyName} đang được quản trị viên kiểm tra!";
                string url = $"http://localhost:5281/api/Account/notification/{recruiterProfile1.RecruiterId}";
                string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiterProfile1.Fullname},</h2>\r\n    <p>Cảm ơn bạn đã sử dụng website của chúng tôi, chúng tôi đang xem xét phê duyệt công ty {enterprise.FullCompanyName} bạn đã đăng kí, kết quả sẽ sớm được thông báo tới bạn, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                e.SendEmail(enterprise.CompanyEmail, title, message);
                return Ok("Gửi mail từ chối phê duyệt công ty thành công");
            }
            else if(enterprise.IsCensorship == true)
            {
                JobSeekerNotification notification = new JobSeekerNotification();
                notification.Id = Guid.NewGuid().ToString();
                notification.Type = "companyapproved";
                notification.Description = $"Công ty {enterprise.FullCompanyName} đã được quản trị viên phê duyệt!";
                notification.IdConcern = enterprise.EnterpriseId;
                notification.IdUserReceive = recruiterProfile1.RecruiterId;
                notification.IsCreatedAt = DateTime.Now;
                notification.IsSent = true;
                notification.IsSeen = false;
                db.JobSeekerNotifications.Add(notification);
                db.SaveChanges();

                string title = $"Công ty {enterprise.FullCompanyName} đã được quản trị viên phê duyệt!";
                string url = $"http://localhost:5281/api/Account/notification/{recruiterProfile1.RecruiterId}";
                string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {recruiterProfile1.Fullname},</h2>\r\n    <p>Cảm ơn bạn đã sử dụng website của chúng tôi, chúng tôi đã phê duyệt công ty {enterprise.FullCompanyName} bạn đã đăng kí, bây giờ công ty của bạn có thể hiển thị và đăng bài đăng tuyển ở website của chúng tôi, nếu có vấn đề gì vui lòng liên hệ đến chúng tôi để được giải đáp.</p>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                e.SendEmail(enterprise.CompanyEmail, title, message);
                return Ok("Gửi mail từ chối phê duyệt công ty thành công");
            }
            return BadRequest("Có cấn đề khi dữ liệu đầu vào");
        }
    }
}
