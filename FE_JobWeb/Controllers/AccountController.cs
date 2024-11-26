using BE_JobWeb.PasswordHasher;
using Data_JobWeb.Dtos;
using Data_JobWeb.Entity;
using FE_JobWeb.Others;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FE_JobWeb.Controllers
{
    public class AccountController : Controller
    {
        private JobSeekerContext db = new JobSeekerContext();
        private ApplicationUser user;
        public AccountController(ApplicationUser user)
        {
            this.user = user;
        }
        public IActionResult Login(string? error)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            #region validate
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ!";
                return View();
            }
            if (!IsValidEmail(email))
            {
                ViewBag.ErrorMessage = "Email không hợp lệ!";
                return View();
            }
            #endregion
            JobSeekerUserLoginDatum p = new JobSeekerUserLoginDatum();
            p.Email = email;
            p.Password = password;
            
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/account/login";
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json");
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();

                // Parse JSON to get "data" field
                var jsonObject = JObject.Parse(responseData);
                var data = jsonObject["data"].ToString();
                var token = jsonObject["token"].ToString();

                if (data != null)
                {
                    // Lưu thông tin tài khoản singleton
                    user.User = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(data);

                    //lưu token vào web
                    CookieOptions options = new CookieOptions
                    {
                        HttpOnly = true, // Cookie chỉ có thể truy cập từ server
                        Secure = true, // Cookie chỉ được truyền qua HTTPS
                        Expires = DateTime.UtcNow.AddHours(1) // Thời gian hết hạn
                    };
                    Response.Cookies.Append("jwtToken", token, options);

                    Console.WriteLine("login thanh cong");
                    if(user.User.RoleId == 3) return RedirectToAction("Index", "Home");
                    else if(user.User.RoleId == 2) return RedirectToAction("IndexRecruiter", "Home");
                    else return RedirectToAction("IndexAdmin", "Home");
                }
                else
                {
                    Console.WriteLine("Login khong thanh cong - du lieu phan hoi khong hop le");
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Register Candidate: Gui that bai voi ma loi: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Email hoặc mật khẩu không chính xác!";
                return View();
            }
        }
        [HttpGet]
        public IActionResult Logout()
        {
            user.User = null;
            // Xóa token 
            Response.Cookies.Delete("jwtToken");

            return RedirectToAction("Index", "Home");
        }
        //Đăng kí úng viên
        public IActionResult RegisterCandidate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterCandidate(string fullname, string password, string email, string confirmpassword)
        {
            #region validate
            if (string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmpassword))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ!";
                return View();
            }
            if (!IsValidEmail(email))
            {
                ViewBag.ErrorMessage = "Email không hợp lệ!";
                return View();
            }
            if (!IsValidPassword(password))
            {
                ViewBag.ErrorMessage = "Mật khẩu phải có it nhất 12 ký tự, trong đó ít nhất 1 ký tự in hoa, thường, số, ký tự đặc biệt!";
                return View();
            }
            if (password != confirmpassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận khác mật khẩu!";
                return View();
            }
            JobSeekerUserLoginDatum o = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == email);
            if (o != null)
            {
                ViewBag.ErrorMessage = "Email đã tồn tại!";
                return View();
            }
            #endregion
            HttpClient client = new HttpClient();
            //Khởi tạo
            JobSeekerUserLoginDatum p = new JobSeekerUserLoginDatum();
            p.FullName = fullname;
            p.Password = password;
            p.Email = email;
            p.RoleId = 3;
            p.Id = Guid.NewGuid();
            p.EmailVerified = false;
            p.StatusCode = "SC8"; // email chưa xác thực
            p.IsCreatedAt = DateTime.UtcNow;
            p.AvartarUrl = "https://firebasestorage.googleapis.com/v0/b/movieapp-2f052.appspot.com/o/JobWeb%2FImageUsers%2Fprofile_1.png?alt=media";

            //Call api
            var apiUrl = "http://localhost:5281/api/account/addcandidate";
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json");
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("add candidate thanh cong");
                return View("Login");
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Register Candidate: Gui that bai voi ma loi: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return View();
            }
        }
        //Đăng kí tuyển dụng
        public async Task<IActionResult> RegisterRecruiter()
        {
            //get jobfield
            ViewBag.Jobfield = await GetJobfield();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterRecruiterAsync(string fullname, string password, string email, string confirmpassword, string gender, string phone, string companyname, string companyemail, string companyphone, string taxcode, string companysize, string websiteurl, string city, string district, string ward, string jobfield, string addressdetail, DateTime foundeddate)
        {
            #region validate
            if (string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmpassword) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(companyname) || string.IsNullOrEmpty(companyemail) || string.IsNullOrEmpty(companyphone) || string.IsNullOrEmpty(taxcode) || string.IsNullOrEmpty(companysize) || string.IsNullOrEmpty(websiteurl) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district) || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(jobfield) || string.IsNullOrEmpty(addressdetail) || foundeddate == null)
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            if (!IsValidEmail(email))
            {
                ViewBag.ErrorMessage = "Email không hợp lệ!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            JobSeekerUserLoginDatum l = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == email);
            if (l != null)
            {
                ViewBag.ErrorMessage = "Email đã tồn tại!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            if (!IsValidPhoneNumber(phone))
            {
                ViewBag.ErrorMessage = "Số điện thoại không hợp lệ!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            JobSeekerRecruiterProfile r = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.PhoneNumb == phone);
            if (r != null)
            {
                ViewBag.ErrorMessage = "Số điện thoại đã tồn tại!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            if (!IsValidPassword(password))
            {
                ViewBag.ErrorMessage = "Mật khẩu phải có it nhất 12 ký tự, trong đó ít nhất 1 ký tự in hoa, thường, số, ký tự đặc biệt!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            if (password != confirmpassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận khác mật khẩu!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            if (!IsValidEmail(companyemail))
            {
                ViewBag.ErrorMessage = "Email công ty không hợp lệ!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            JobSeekerEnterprise ll = db.JobSeekerEnterprises.FirstOrDefault(p => p.CompanyEmail == companyemail);
            if (ll != null)
            {
                ViewBag.ErrorMessage = "Email công ty đã tồn tại!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            if (!IsValidPhoneNumber(companyphone))
            {
                ViewBag.ErrorMessage = "Số điện thoại công ty không hợp lệ!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            JobSeekerEnterprise r1 = db.JobSeekerEnterprises.FirstOrDefault(p => p.CompanyPhoneContact == companyphone);
            if (r1 != null)
            {
                ViewBag.ErrorMessage = "Số điện thoại đã tồn tại!";
                ViewBag.Jobfield = await GetJobfield();
                return View();
            }
            #endregion

            //Khởi tạo
            JobSeekerUserLoginDatum p = new JobSeekerUserLoginDatum();
            p.Id = Guid.NewGuid();
            p.FullName = fullname;
            p.Password = password;
            p.Email = email;
            p.RoleId = 2;
            p.EmailVerified = false;
            p.StatusCode = "SC8"; // email chưa xác thực
            p.IsCreatedAt = DateTime.UtcNow;
            p.AvartarUrl = "https://firebasestorage.googleapis.com/v0/b/movieapp-2f052.appspot.com/o/JobWeb%2FImageUsers%2Fprofile_1.png?alt=media";

            JobSeekerEnterprise e = new JobSeekerEnterprise();
            e.EnterpriseId = Guid.NewGuid();
            e.FullCompanyName = companyname;
            e.CompanyEmail = companyemail;
            e.CompanyPhoneContact = companyphone;
            e.TaxCode = taxcode;
            e.EnterpriseSize = companysize;
            e.WebsiteUrl = websiteurl;
            e.City = city;
            e.District = district;
            e.Ward = ward;
            e.JobFieldId = int.Parse(jobfield);
            e.Address = addressdetail;
            e.FoundedDate = DateOnly.FromDateTime(foundeddate);
            e.LogoUrl = "https://firebasestorage.googleapis.com/v0/b/movieapp-2f052.appspot.com/o/JobWeb%2FImageUsers%2Fprofile_1.png?alt=media";
            e.CoverImgUrl = "https://firebasestorage.googleapis.com/v0/b/movieapp-2f052.appspot.com/o/JobWeb%2FImageUsers%2Fprofile_1.png?alt=media";

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/account/addrecruiter";
            // Tạo một object dto
            RegisterRecruiter o = new RegisterRecruiter();
            o.UserLoginData = p;
            o.EnterPrise = e;
            o.Gender = gender;
            o.Phone = phone;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
            Console.WriteLine("json\n" + JsonConvert.SerializeObject(o));
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("add recruiter thanh cong");
                return View("Login");
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Register Recruiter: add recruiter that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return View();
            }
        }
        public IActionResult VerifyEmail()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            #region validate
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ!";
                return View();
            }
            if (!IsValidEmail(email))
            {
                ViewBag.ErrorMessage = "Email không hợp lệ!";
                return View();
            }
            JobSeekerUserLoginDatum l = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == email);
            if (l == null)
            {
                ViewBag.ErrorMessage = "Email không tồn tại trong hệ thống!";
                return View();
            }
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/account/sendemail";
            string type = "verifyresetpassword";
            SendMail o = new SendMail();
            o.Email = email;
            o.Type = type;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
            Console.WriteLine("json\n" + JsonConvert.SerializeObject(o));
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Chúng tôi đã gửi cho bạn một mail xác minh, vui lòng check mail và làm theo hướng dẫn để hoàn tất quy trình lấy lại mật khẩu!";
                Console.WriteLine("gui mail xac nhan forgotpass thanh cong");
                return View("Login");
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Forgot Passord: gui mail that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return View();
            }
        }
        public IActionResult MyProfile()
        {
            return View();
        }
        //ddanglm ts ddaay, cần update ảnh bằng firebase và hiển thị lên, code sơ sơ chauw demo, về xem kĩ lạ
        [HttpPost]
        public async Task<IActionResult> MyProfile(string fullname, string email, IFormFile image)
        {
            #region validate
            if (string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ!";
                return View();
            }
            JobSeekerUserLoginDatum u = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Email == email);
            if (email != user.User.Email)
            {
                if(u != null)
                {
                    ViewBag.ErrorMessage = "Email đã được sử dụng!";
                    return View();
                }
            }
            #endregion
            JobSeekerUserLoginDatum c = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Id == user.User.Id);

            if (image != null)
            {
                FirebaseService firebase = new FirebaseService();
                //// Xóa ảnh cũ từ firebase storage
                //if (user.User.AvartarUrl != null || user.User.AvartarUrl != "https://firebasestorage.googleapis.com/v0/b/movieapp-2f052.appspot.com/o/JobWeb%2FImageUsers%2Fprofile_1.png?alt=media")
                //{
                //    if (await firebase.DeleteImageFromFirebaseAsync(firebase.ExtractFileNameFromUrl("https://firebasestorage.googleapis.com/v0/b/movieapp-2f052.appspot.com/o/JobWeb%2FImageUsers%2Fcv.jpg?alt=media"))) Console.WriteLine("xoa anh cu tu firebase thanh cong");
                //    else Console.WriteLine("xoa anh cu tu firebase that bai");
                //}

                // Đẩy ảnh lên firebase storage
                using var stream = image.OpenReadStream();
                var fileName = Path.GetFileName(image.FileName);
                var imageUrl = await firebase.UploadImageToFirebaseAsync(stream, fileName, user.User.Id.ToString(), "user");


                if (imageUrl == "error")
                {
                    ViewBag.ErrorMessage = $"Chỉ hỗ trợ định dạng.jpg, .jpeg và .png.!";
                    Console.WriteLine("sai dinh dang anh");
                    return View();
                }
                Console.WriteLine("upload anh vao firebase thanh cong");

                c.AvartarUrl = imageUrl;
            }

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/account/updateprofile";
            c.FullName = fullname;
            c.Email = email;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c), Encoding.UTF8, "application/json");
            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content); 

            if (response.IsSuccessStatusCode)
            {
                user.User = c;
                ViewBag.ErrorMessage = $"Cật nhật thông tin tài khoản thành công!";
                Console.WriteLine("cat nhat thong tin tai khoan thanh cong thanh cong");
                return View();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                ViewBag.ErrorMessage = $"Bạn không có quyền thực hiện thao tác này!";
                return View();
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Update Profile: cat nhat that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldpassword, string newpassword, string confirmnewpassword)
        {
            #region validate
            if (string.IsNullOrEmpty(oldpassword) || string.IsNullOrEmpty(newpassword) || string.IsNullOrEmpty(oldpassword))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ!";
                return View("MyProfile");
            }
            JobSeekerUserLoginDatum u = db.JobSeekerUserLoginData.FirstOrDefault(p => p.Id == user.User.Id);
            Hasher h = new Hasher();
            if (u != null && !h.VerifyPassword(u.Password, oldpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu cũ sai!";
                return View("MyProfile");
            }
            if (oldpassword == newpassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu mới phải khác mật khẩu cũ!";
                return View("MyProfile");
            }
            if (!IsValidPassword(newpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu phải có it nhất 12 ký tự, trong đó ít nhất 1 ký tự in hoa, thường, số, ký tự đặc biệt!";
                return View("MyProfile");
            }
            if (newpassword != confirmnewpassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu mới khác mật khẩu xác nhận mới!";
                return View("MyProfile");
            }
            #endregion
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/account/changepassword";
            JobSeekerUserLoginDatum c = new JobSeekerUserLoginDatum();
            c.Id = user.User.Id;
            c.Password = newpassword;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c), Encoding.UTF8, "application/json");
            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c));
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = $"Mật khẩu mới là {newpassword}!";
                Console.WriteLine("change password thanh cong");
                return View("MyProfile");
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Change Password: change password that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return View("MyProfile");
            }
        }
        [HttpPost]
        public IActionResult NotificationSettingCandidate()
        {
            return View();
        }
        public IActionResult CandidateProfile(string? error)
        {
            JobSeekerCandidateProfile j = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == user.User.Id);
            if (j == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy profile candidate!" });

            List<JobSeekerWorkingExperience> k = db.JobSeekerWorkingExperiences.Where(p => p.CandidateId == user.User.Id).ToList();
            ViewBag.Experience = k;

            List<JobSeekerEducationDetail> l = db.JobSeekerEducationDetails.Where(p => p.CandidateId == user.User.Id).ToList();
            ViewBag.Education = l;

            List<JobSeekerCertificate> m = db.JobSeekerCertificates.Where(p => p.CandidateId == user.User.Id).ToList();
            ViewBag.Certificate = m;

            if (error != null) ViewBag.ErrorMessage = error;
            return View(j);
        }
        [HttpPost]
        public async Task<IActionResult> CandidateProfile(int type, IFormFile image, string fullname, string email, string gender, DateTime dob, string phone, string city, string district, string ward, string address, IFormFile profile, string facebookurl, string linkedinurl, string portfoliourl, string twitterurl, string githuburl)
        {
            if (type == 1)
            {
                #region validate
                if (string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district) || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(address) || dob == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });

                if (!IsValidEmail(email)) return RedirectToAction("CandidateProfile", "Account", new { error = "Email không hợp lệ!" });

                if (!IsValidPhoneNumber(phone)) return RedirectToAction("CandidateProfile", "Account", new { error = "Số điện thoại không hợp lệ!" });

                JobSeekerCandidateProfile r = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == user.User.Id);
                if (r != null)
                {
                    if (email != r.Email)
                    {
                        JobSeekerCandidateProfile f = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.Email == email);
                        if (f != null) return RedirectToAction("CandidateProfile", "Account", new { error = $"Email {email} đã được sử dụng!" });
                    }
                    if (phone != r.PhoneNumb)
                    {
                        JobSeekerCandidateProfile f = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.PhoneNumb == phone);
                        if (f != null) return RedirectToAction("CandidateProfile", "Account", new { error = $"Số điện thoại {phone} đã được sử dụng!" });
                    }
                }
                else return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy hồ sơ candiate!" });

                if (DateOnly.FromDateTime(dob) > DateOnly.FromDateTime(DateTime.UtcNow)) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày sinh không hợp lệ!" });
                #endregion
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/account/updatecandidateprofile";

                JobSeekerCandidateProfile o = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == user.User.Id);
                if (o != null)
                {
                    o.Fullname = fullname;
                    o.Email = email;
                    o.PhoneNumb = phone;
                    o.Gender = gender;
                    o.Dob = DateOnly.FromDateTime(dob);
                    o.Province = city;
                    o.District = district;
                    o.Ward = ward;
                    o.AddressDetail = address;
                    if (image != null)
                    {
                        FirebaseService firebase = new FirebaseService();
                        // Đẩy ảnh lên firebase storage
                        using var stream = image.OpenReadStream();
                        var fileName = Path.GetFileName(image.FileName);
                        var imageUrl = await firebase.UploadImageToFirebaseAsync(stream, fileName, user.User.Id.ToString(), "user");

                        if (imageUrl == "error")
                        {
                            Console.WriteLine("sai dinh dang anh");
                            RedirectToAction("CandidateProfile", "Account", new { error = $"Chỉ hỗ trợ định dạng.jpg, .jpeg và .png.!" });
                        }
                        Console.WriteLine("upload anh vao firebase thanh cong");

                        o.AvartarUrl = imageUrl;
                    }
                }
                else return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy hồ sơ candidate!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
                //get token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu POST tới API
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    user.User.AvartarUrl = o.AvartarUrl;
                    Console.WriteLine("cat nhat thong tin ho so thanh cong");
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin hồ sơ thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Candidate Profile 1: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else if (type == 2)
            {
                if (profile == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng tải cv lên hệ thống!" });
                if (profile == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng tải cv lên hệ thống!" });

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/account/updatecandidateprofile";

                JobSeekerCandidateProfile o = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == user.User.Id);
                if (o != null)
                {
                    FirebaseService firebase = new FirebaseService();
                    // Đẩy ảnh lên firebase storage
                    using var stream = profile.OpenReadStream();
                    var fileName = Path.GetFileName(profile.FileName);
                    var imageUrl = await firebase.UploadImageToFirebaseAsync(stream, fileName, user.User.Id.ToString() + "cv", "resume");

                    if (imageUrl == "error")
                    {
                        Console.WriteLine("sai dinh dang file");
                        return RedirectToAction("CandidateProfile", "Account", new { error = "Chỉ hỗ trợ định dạng .pdf!" });
                    }
                    Console.WriteLine("upload file vao firebase thanh cong");

                    o.CvUrl = imageUrl;
                }
                else return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy hồ sơ candidate!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
                //get token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu POST tới API
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    user.User.AvartarUrl = o.AvartarUrl;
                    Console.WriteLine("cat nhat thong tin ho so thanh cong");
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin hồ sơ thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Candidate Profile 1: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/account/updatecandidateprofile";

                JobSeekerCandidateProfile o = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == user.User.Id);
                if (o != null)
                {
                    if(facebookurl != null) o.FacbookLink = facebookurl;
                    if(linkedinurl != null) o.LinkedinLink = linkedinurl;
                    if(portfoliourl != null) o.PortfolioUrl = portfoliourl;
                    if(twitterurl != null) o.TwitterUrl = twitterurl;
                    if(githuburl != null) o.GithubUrl = githuburl;
                }
                else return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy hồ sơ candidate!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
                //get token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu POST tới API
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("cat nhat thong tin ho so thanh cong");
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin hồ sơ thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Candidate Profile 2: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddCandidateProfileExperience(string jobtitle, string companyname, DateTime startday, DateTime endday, string description)
        {
            #region validate
            if (string.IsNullOrEmpty(jobtitle) || string.IsNullOrEmpty(companyname) || string.IsNullOrEmpty(description) || startday == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });

            if (startday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày bắt đầu không hợp lệ!" });
            if (endday != null && endday != DateTime.MinValue)
            {
                if (endday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc không hợp lệ!" });
                if (endday < startday) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc phải lớn hơn ngày bắt đầu!" });
            }
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/experience/addexperience";
            JobSeekerWorkingExperience c = new JobSeekerWorkingExperience();
            c.WorkingExpId = Guid.NewGuid();
            c.JobTitle = jobtitle;
            c.CompanyName = companyname;
            c.Description = description;
            c.StartDate = DateOnly.FromDateTime(startday);
            c.EndDate = endday != null && endday != DateTime.MinValue ? DateOnly.FromDateTime(endday) : null;
            c.CandidateId = user.User.Id;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin kinh nghiem tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin kinh nghiệm tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("AddCandidateProfileExperience: add that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCandidateProfileExperience(Guid id, string jobtitle, string companyname, DateTime startday, DateTime endday, string description)
        {
            #region validate
            if (string.IsNullOrEmpty(jobtitle) || string.IsNullOrEmpty(companyname) || string.IsNullOrEmpty(description) || startday == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });

            if (startday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày bắt đầu không hợp lệ!" });
            if (endday != null && endday != DateTime.MinValue)
            {
                if (endday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc không hợp lệ!" });
                if (endday < startday) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc phải lớn hơn ngày bắt đầu!" });
            }
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/experience/updateexperience";
            JobSeekerWorkingExperience c = db.JobSeekerWorkingExperiences.FirstOrDefault(p => p.WorkingExpId == id);
            if (c == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy kinh nghiệm này!" });
            c.JobTitle = jobtitle;
            c.CompanyName = companyname;
            c.Description = description;
            c.StartDate = DateOnly.FromDateTime(startday);
            c.EndDate = endday != null && endday != DateTime.MinValue ? DateOnly.FromDateTime(endday) : null;
            c.CandidateId = user.User.Id;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin kinh nghiem tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin kinh nghiệm tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("UpdateCandidateProfileExperience: add that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        public async Task<IActionResult> DeleteExperience(Guid id)
        {
            if (id == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/experience/deleteexperience/{id}";
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu DELETE tới API
            HttpResponseMessage response = await client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin kinh nghiem tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin kinh nghiệm tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("AddCandidateProfileExperience: delete that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddCandidateProfileEducation(string name, string major, string degree, DateTime startday, DateTime endday, string description)
        {
            #region validate
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(major) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(degree) || startday == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });

            if (startday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày bắt đầu không hợp lệ!" });
            if (endday != null && endday != DateTime.MinValue)
            {
                if (endday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc không hợp lệ!" });
                if (endday < startday) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc phải lớn hơn ngày bắt đầu!" });
            }
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/education/addeducation";
            JobSeekerEducationDetail c = new JobSeekerEducationDetail();
            c.EducationId = Guid.NewGuid();
            c.SchoolName = name;
            c.Major = major;
            c.Degree = degree;
            c.Description = description;
            c.StartDate = DateOnly.FromDateTime(startday);
            c.EndDate = endday != null && endday != DateTime.MinValue ? DateOnly.FromDateTime(endday) : null;
            c.CandidateId = user.User.Id;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin hoc van tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin học vấn tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("AddCandidateProfileEducation: add that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCandidateProfileEducation(Guid id, string name, string major, string degree, DateTime startday, DateTime endday, string description)
        {
            #region validate
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(major) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(degree) || startday == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });

            if (startday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày bắt đầu không hợp lệ!" });
            if (endday != null && endday != DateTime.MinValue)
            {
                if (endday > DateTime.UtcNow) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc không hợp lệ!" });
                if (endday < startday) return RedirectToAction("CandidateProfile", "Account", new { error = "Ngày kết thúc phải lớn hơn ngày bắt đầu!" });
            }
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/education/updateeducation";
            JobSeekerEducationDetail c = db.JobSeekerEducationDetails.FirstOrDefault(p => p.EducationId == id);
            if (c == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy học vấn này!" });
            c.SchoolName = name;
            c.Major = major;
            c.Degree = degree;
            c.Description = description;
            c.StartDate = DateOnly.FromDateTime(startday);
            c.EndDate = endday != null && endday != DateTime.MinValue ? DateOnly.FromDateTime(endday) : null;
            c.CandidateId = user.User.Id;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin hoc van tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin học vấn tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("UpdateCandidateProfileEducation: update that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        public async Task<IActionResult> DeleteEducation(Guid id)
        {
            if (id == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/education/deleteeducation/{id}";
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu DELETE tới API
            HttpResponseMessage response = await client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin hoc van tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin học vấn tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("AddCandidateProfileeducation: delete that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddCandidateProfileCertificate(string name, string organization, string link, string description)
        {
            #region validate
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(link)) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/certificate/addcertificate";
            JobSeekerCertificate c = new JobSeekerCertificate();
            c.CertificateId = Guid.NewGuid();
            c.CertificateName = name;
            c.Organization = organization;
            c.CertificateLink = link;
            c.Description = description;
            c.CandidateId = user.User.Id;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin chung chi tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin chứng chỉ tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("AddCandidateProfileCertificate: add that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCandidateProfileCertificate(Guid id, string name, string organization, string link, string description)
        {
            #region validate
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(link)) return RedirectToAction("CandidateProfile", "Account", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/certificate/updatecertificate";
            JobSeekerCertificate c = db.JobSeekerCertificates.FirstOrDefault(p => p.CertificateId == id);
            if (c == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Không tìm thấy chứng chỉ này!" });
            c.CertificateName = name;
            c.Organization = organization;
            c.CertificateLink = link;
            c.Description = description;
            c.CandidateId = user.User.Id;
            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST tới API
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin chung chi tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin chứng chỉ tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("UpdateCandidateProfileCertificate: update that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        public async Task<IActionResult> DeleteCertificate(Guid id)
        {
            if (id == null) return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/certificate/deletecertificate/{id}";
            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu DELETE tới API
            HttpResponseMessage response = await client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("cat nhat thong tin chung chi tai khoan thanh cong thanh cong");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Cật nhật thông tin chứng chỉ tài khoản thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("CandidateProfile", "Account", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("AddCandidateProfilecertificate: delete that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("CandidateProfile", "Account", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        //Lấy danh sách lĩnh vực
        public async Task<List<JobSeekerJobField>> GetJobfield()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/job/getjobfield";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerJobField> j = new List<JobSeekerJobField>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerJobField>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("get jobfield thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Register Candidate: lay jobfield that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return null;
            }
        }
        //check pass
        public static bool IsValidPassword(string password)
        {
            var hasMinimumLength = password.Length >= 12;
            var hasSpecialChar = Regex.IsMatch(password, @"[!@#\$%\^&\*\(\)_\+\-=\[\]{};':""\\|,.<>\/?]");
            var hasNumber = Regex.IsMatch(password, @"\d");
            var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
            var hasLowerCase = Regex.IsMatch(password, @"[a-z]");

            return hasMinimumLength && hasSpecialChar && hasNumber && hasUpperCase && hasLowerCase;
        }
        //checkmail
        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        //checkphone
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^0\d{9,10}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(phoneNumber);
        }
    }
}
