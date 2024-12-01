using BE_JobWeb.Others;
using BE_JobWeb.PasswordHasher;
using Data_JobWeb.Dtos;
using Data_JobWeb.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BE_JobWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private JobSeekerContext db = new JobSeekerContext();
        [HttpPost("login")]
        public IActionResult Login(JobSeekerUserLoginDatum e)
        {
            //Thêm tài khoản
            //Mã hóa mật khẩu
            Hasher hasher = new Hasher();
            JobSeekerUserLoginDatum c = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == e.Email);
            if(c == null) return BadRequest("Login fail!");
            if(c.EmailVerified == false) return BadRequest("Login fail!");
            bool checkpass = hasher.VerifyPassword(c.Password, e.Password);
            if (checkpass)
            {
                c.IsActive = true;
                db.SaveChanges();
                //Tạo token
                //AuthenticationRole a = db.AuthenticationRoles.FirstOrDefault(p => p.RoleId == c.RoleId);
                Token t = new Token();
                string token = t.GenerateToken(c.FullName, c.RoleId.ToString(), c.Id.ToString());

                //lưu token vào web
                CookieOptions options = new CookieOptions
                {
                    HttpOnly = true, // Cookie chỉ có thể truy cập từ server
                    Secure = true, // Cookie chỉ được truyền qua HTTPS
                    Expires = DateTime.Now.AddHours(1) // Thời gian hết hạn
                };
                Response.Cookies.Append("jwtToken", token, options);

                return Ok(new { message = "Login success!", token = token , data = c});
            }
            else return Unauthorized("Login error");
        }
        [HttpGet("logout/{id}")]
        public IActionResult Logout(Guid id)
        {
            JobSeekerUserLoginDatum c = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Id == id);
            if(c == null) return BadRequest("Logout fail!");

            c.IsActive = false;
            c.LastActiveTime = DateTime.Now;
            db.SaveChanges();
            
            return Ok("Logout success!");
        }
        [HttpGet("getlistuser")]
        public IActionResult GetListUser()
        {
            return Ok(db.JobSeekerUserLoginData.OrderByDescending(p => p.IsCreatedAt).ToList());
        }
        [HttpPost("addcandidate")]
        public IActionResult AddCandidate(JobSeekerUserLoginDatum p)
        {
            //Thêm tài khoản
            //Mã hóa mật khẩu
            Hasher hasher = new Hasher();
            p.Password = hasher.HashPassword(p.Password);
            db.JobSeekerUserLoginData.Add(p);
            db.SaveChanges();
            //Tạo nhà cung cấp
            JobSeekerUserLoginDataExternal e = new JobSeekerUserLoginDataExternal();
            e.Id = p.Id;
            e.ProviderName = "system";
            db.JobSeekerUserLoginDataExternals.Add(e);
            db.SaveChanges();
            //Tạo hồ sơ 
            JobSeekerCandidateProfile c = new JobSeekerCandidateProfile();
            c.CandidateId = p.Id;
            c.Fullname = p.FullName;
            c.Email = p.Email;
            c.RoleId = p.RoleId;
            db.JobSeekerCandidateProfiles.Add(c);
            db.SaveChanges();

            return Ok("Add Candidate success!");
        }
        [HttpPost("addrecruiter")]
        public IActionResult AddRecruiter(RegisterRecruiter p)
        {
            //Thêm tài khoản
            //Mã hóa mật khẩu
            Hasher hasher = new Hasher();
            p.UserLoginData.Password = hasher.HashPassword(p.UserLoginData.Password);
            db.JobSeekerUserLoginData.Add(p.UserLoginData);
            db.SaveChanges();
            //Thêm công ty
            db.JobSeekerEnterprises.Add(p.EnterPrise);
            db.SaveChanges();
            //Tạo hồ sơ 
            JobSeekerRecruiterProfile c = new JobSeekerRecruiterProfile();
            c.RecruiterId = p.UserLoginData.Id;
            c.Fullname = p.UserLoginData.FullName;
            c.Email = p.UserLoginData.Email;
            c.RoleId = p.UserLoginData.RoleId;
            c.Gender = p.Gender;
            c.EnterpriseId = p.EnterPrise.EnterpriseId;
            c.PhoneNumb = p.Phone;
            db.JobSeekerRecruiterProfiles.Add(c);
            db.SaveChanges();

            return Ok("Add Recruiter success!");
        }
        [HttpGet("verifyemail/{email}")]
        public IActionResult VerifyEmail(string email)
        {
            JobSeekerUserLoginDatum x = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == email);
            if (x != null)
            {
                if (x.EmailVerified == true)
                {
                    return Ok("Email đã được xác nhận");
                }
                else
                {
                    x.EmailVerified = true;
                    x.StatusCode = "SC9";
                    db.SaveChanges();

                    if(x.RoleId == 2)
                    {
                        JobSeekerRecruiterProfile recruiterProfile = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == x.Id);
                        if (recruiterProfile == null) return Redirect("http://localhost:5080/Account/Login");

                        JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == recruiterProfile.EnterpriseId);
                        if (enterprise == null) return Redirect("http://localhost:5080/Account/Login");

                        JobSeekerUserLoginDatum admin = db.JobSeekerUserLoginData.FirstOrDefault(p => p.RoleId == 1);
                        if (admin == null) return Redirect("http://localhost:5080/Account/Login");

                        JobSeekerNotification notification = new JobSeekerNotification();
                        notification.Id = Guid.NewGuid().ToString();
                        notification.Type = "admin_newenterprise";
                        notification.Description = $"Bạn có 1 công ty mới đang chờ duyệt!";
                        notification.IsSeen = false;
                        notification.IsCreatedAt = DateTime.Now;
                        notification.IdConcern = enterprise.EnterpriseId;
                        db.JobSeekerNotifications.Add(notification);
                        db.SaveChanges();

                        EmailService e = new EmailService();
                        string title = $"Bạn có 1 công ty mới đang chờ duyệt! - JobWeb";
                        string url = $"http://localhost:5281/api/Account/notification/{admin.Id}";
                        string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>{title}</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {admin.FullName},</h2>\r\n    <p>Kiểm tra các thông báo của bạn tại đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Thông báo</a>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                        e.SendEmail("khidot705@gmail.com", title, message);
                    }

                    //return Ok("Xác nhận email thành công");
                    return Redirect("http://localhost:5080/Account/Login");
                }
            }
            else return BadRequest(email + " không tồn tại trong hệ thống");
        }
        [HttpGet("verifyresetpassword/{email}")]
        public IActionResult VerifyResetPassword(string email)
        {
            JobSeekerUserLoginDatum x = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == email);
            if (x != null)
            {
                Hasher h = new Hasher();
                string passnew = h.GeneratePassword();
                string encrypepassnew = h.HashPassword(passnew);

                EmailService e = new EmailService();
                string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>Đặt lại mật khẩu - JobWeb</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {x.FullName},</h2>\r\n    <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>\r\n    <p>Mật khẩu mới của bạn là: {passnew}</p>\r\n    <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng <a href=\"mailto:baitoan88@gmail.com\">liên hệ với bộ phận hỗ trợ</a>.</p>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>";

                e.SendEmail(email, "Đặt lại mật khẩu - JobWeb", message);
                x.Password = encrypepassnew;
                db.SaveChanges();

                //return Ok("Pass mới đã được gửi đến " + email);
                return Redirect("http://localhost:5080/Account/Login");
            }
            else return BadRequest(email + " không tồn tại trong hệ thống");
        }
        [HttpPost("sendemail")]
        public IActionResult SendEmail(SendMail sendmail)
        {
            JobSeekerUserLoginDatum x = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == sendmail.Email);
            if (x != null)
            {
                if(sendmail.Type == "verifyresetpassword")
                {
                    string url = $"http://localhost:5281/api/Account/verifyresetpassword/{sendmail.Email}";

                    EmailService e = new EmailService();
                    string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>Xác nhận đặt lại mật khẩu - JobWeb</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {x.FullName},</h2>\r\n    <p>Cảm ơn bạn đã sử dụng website JobWeb của chúng tôi. Để hoàn tất quá trình đặt lại mật khẩu, vui lòng xác nhận đặt lại mật khẩu của bạn bằng cách nhấn vào liên kết dưới đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Xác minh Email</a>\r\n    <p>Nếu bạn không thực hiện yêu cầu đặt lại mật khẩu tại JobWeb, vui lòng bỏ qua email này.</p>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>\r\n";

                    e.SendEmail(sendmail.Email, "Xác nhận đặt lại mật khẩu - JobWeb", message);

                    return Ok("Url verify reset password đã được gửi đến " + sendmail.Email);
                }
                else //verifyemail
                {
                    string url = $"http://localhost:5281/api/Account/verifyemail/{sendmail.Email}";

                    EmailService e = new EmailService();
                    string message = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>Xác minh email - JobWeb</title>\r\n</head>\r\n<body>\r\n    <h2>Chào {x.FullName},</h2>\r\n    <p>Cảm ơn bạn đã đăng ký tài khoản tại JobWeb. Để hoàn tất quá trình đăng ký, vui lòng xác minh địa chỉ email của bạn bằng cách nhấn vào liên kết dưới đây:</p>\r\n    <a href=\"{url}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block;\">Xác minh Email</a>\r\n    <p>Nếu bạn không đăng ký tài khoản tại JobWeb, vui lòng bỏ qua email này.</p>\r\n    <p>Cảm ơn bạn,</p>\r\n    <p>Đội ngũ JobWeb</p>\r\n</body>\r\n</html>\r\n";

                    e.SendEmail(sendmail.Email, "Xác minh email - JobWeb", message);

                    return Ok("Url verify email đã được gửi đến " + sendmail.Email);
                }
            }
            else return BadRequest(sendmail.Email + " không tồn tại trong hệ thống");
        }
        [HttpPost("changepassword")]
        public IActionResult ChangePassword(JobSeekerUserLoginDatum e)
        {
            Hasher hasher = new Hasher();
            JobSeekerUserLoginDatum r = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Id == e.Id);
            if (r != null)
            {
                r.Password = hasher.HashPassword(e.Password);
                db.SaveChanges();
                return Ok("Thay đổi mật khẩu thành công");
            }
            else return BadRequest($"Không tồn tại tài khoản có id {e.Id} trong hệ thống");
        }
        [HttpPost("updateprofile")]
        [Authorize(Roles = "3")]
        public IActionResult UpdateProfile(JobSeekerUserLoginDatum e)
        {
            JobSeekerUserLoginDatum r = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Id == e.Id);
            if (r != null)
            {
                r.AvartarUrl = e.AvartarUrl;
                r.FullName = e.FullName;
                r.Email = e.Email;
                if(r.RoleId == 3)
                {
                    JobSeekerCandidateProfile c = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == r.Id);
                    if (c != null)
                    {
                        c.Fullname = r.FullName;
                        c.Email = r.Email;
                        c.AvartarUrl = r.AvartarUrl;
                        db.SaveChanges();
                        return Ok("Update profile thành công");
                    }
                    else return BadRequest($"Không tồn tại profile của tài khoản có id {e.Id} trong hệ thống");
                }
                else if(r.RoleId == 2)
                {

                    JobSeekerRecruiterProfile c = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == r.Id);
                    if (c != null)
                    {
                        c.Fullname = r.FullName;
                        c.Email = r.Email;
                        c.AvatarLink = r.AvartarUrl;
                        db.SaveChanges();
                        return Ok("Update profile thành công");
                    }
                    else return BadRequest($"Không tồn tại profile của tài khoản có id {e.Id} trong hệ thống");
                }
                else
                {
                    db.SaveChanges();
                    return Ok("Update profile thành công");
                }
            }
            else return BadRequest($"Không tồn tại tài khoản có id {e.Id} trong hệ thống");
        }
        [HttpPost("updatecandidateprofile")]
        //[Authorize(Roles = "3,1")] nhiu quyen
        [Authorize(Roles = "1,3")]
        public IActionResult UpdateCandidateProfile(JobSeekerCandidateProfile updatedProfile)
        {
            //Cật nhật công ty
            JobSeekerCandidateProfile candidateProfile = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == updatedProfile.CandidateId);
            if (candidateProfile == null) return BadRequest("Không tìm thấy hồ sơ candidate!");

            JobSeekerUserLoginDatum userlogin = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Id == updatedProfile.CandidateId);
            if (candidateProfile == null) return BadRequest("Không tìm thấy hồ sơ login candidate!");

            candidateProfile.Fullname = updatedProfile.Fullname;
            candidateProfile.Dob = updatedProfile.Dob;
            candidateProfile.Gender = updatedProfile.Gender;
            candidateProfile.Email = updatedProfile.Email;
            candidateProfile.PhoneNumb = updatedProfile.PhoneNumb;
            candidateProfile.AvartarUrl = updatedProfile.AvartarUrl;
            candidateProfile.CvUrl = updatedProfile.CvUrl;
            candidateProfile.FacbookLink = updatedProfile.FacbookLink;
            candidateProfile.LinkedinLink = updatedProfile.LinkedinLink;
            candidateProfile.GithubUrl = updatedProfile.GithubUrl;
            candidateProfile.TwitterUrl = updatedProfile.TwitterUrl;
            candidateProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            candidateProfile.Province = updatedProfile.Province;
            candidateProfile.District = updatedProfile.District;
            candidateProfile.Ward = updatedProfile.Ward;
            candidateProfile.IsAllowedPublic = updatedProfile.IsAllowedPublic;
            candidateProfile.IsUpdatedAt = DateTime.Now; // Ghi nhận thời gian cập nhật
            candidateProfile.AddressDetail = updatedProfile.AddressDetail;

            userlogin.AvartarUrl = updatedProfile.AvartarUrl;

            db.SaveChanges();

            return Ok("Update company success!");
        }
    }
}
