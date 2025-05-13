using Data_JobWeb.Dtos;
using Data_JobWeb.Entity;
using FE_JobWeb.Others;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace FE_JobWeb.Controllers
{
    public class CompanyController : Controller
    {
        private JobSeekerContext db = new JobSeekerContext();
        //private ApplicationUser user;
        //int page = 1;
        int pageSize = 10;
        //public CompanyController(ApplicationUser user)
        //{
        //    this.user = user;
        //}
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CompanyProfile(string? error, string? success)
        {
            string json = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            JobSeekerUserLoginDatum user = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(json);

            JobSeekerRecruiterProfile r = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
            if (r == null) return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy profile recruiter!" });

            JobSeekerEnterprise e = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r.EnterpriseId);
            if (e == null) return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy profile company!" });

            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            ViewBag.Jobfield = await GetJobfield();

            return View(e);
        }
        [HttpPost]
        public async Task<IActionResult> CompanyProfile(int type, string companyname, string jobfield, string companysize, string taxcode, DateTime foundeddate, string companyemail, string companyphone, string companydecription, IFormFile imagelogo, IFormFile imagebackground, string facebookurl, string linkedinurl, string websiteurl, string city, string district, string ward, string address)
        {
            string json = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            JobSeekerUserLoginDatum user = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(json);

            if (type == 1)
            {
                #region validate
                if (string.IsNullOrEmpty(companyname) || string.IsNullOrEmpty(jobfield) || string.IsNullOrEmpty(companysize) || string.IsNullOrEmpty(taxcode) || string.IsNullOrEmpty(companyemail) || string.IsNullOrEmpty(companyphone) || string.IsNullOrEmpty(companydecription) || foundeddate == null) return RedirectToAction("CompanyProfile", "Company", new { error = "Vui lòng nhập đầy đủ!" });

                if (!IsValidEmail(companyemail)) return RedirectToAction("CompanyProfile", "Company", new { error = "Email không hợp lệ!" });

                if (!IsValidPhoneNumber(companyphone)) return RedirectToAction("CompanyProfile", "Company", new { error = "Số điện thoại không hợp lệ!" });

                JobSeekerRecruiterProfile r = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
                if (r != null)
                {
                    JobSeekerEnterprise e = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r.EnterpriseId);
                    if (e != null)
                    {
                        if(companyemail != e.CompanyEmail)
                        {
                            JobSeekerEnterprise f = db.JobSeekerEnterprises.FirstOrDefault(p => p.CompanyEmail == companyemail);
                            if (f != null) return RedirectToAction("CompanyProfile", "Company", new { error = $"Email {companyemail} đã được sử dụng!" });
                        }
                        if(companyphone != e.CompanyPhoneContact)
                        {
                            JobSeekerEnterprise f = db.JobSeekerEnterprises.FirstOrDefault(p => p.CompanyPhoneContact == companyphone);
                            if (f != null) return RedirectToAction("CompanyProfile", "Company", new { error = $"Số điện thoại {companyphone} đã được sử dụng!" });
                        }
                    }
                    else return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy công ty!" });
                }
                else return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy hồ sơ recruiter!" });

                if (DateOnly.FromDateTime(foundeddate) > DateOnly.FromDateTime(DateTime.Now)) return RedirectToAction("CompanyProfile", "Company", new { error = "Ngày thành lập không hợp lệ!" });
                #endregion
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatecompany";

                JobSeekerEnterprise o = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r.EnterpriseId);
                if (o != null)
                {
                    o.FullCompanyName = companyname;
                    o.JobFieldId = int.Parse(jobfield);
                    o.EnterpriseSize = companysize;
                    o.TaxCode = taxcode;
                    o.FoundedDate = DateOnly.FromDateTime(foundeddate);
                    o.CompanyEmail = companyemail;
                    o.CompanyPhoneContact = companyphone;
                    o.Descriptions = companydecription;
                }
                else return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy công ty!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
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
                    Console.WriteLine("cat nhat thong tin cong ty thanh cong");
                    return RedirectToAction("CompanyProfile", "Company", new { success = "Cật nhật thông tin công ty thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Company Profile 1: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else if(type == 2)
            {
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatecompany";

                JobSeekerRecruiterProfile r = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
                if (r == null) return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy hồ sơ recruiter!" });

                JobSeekerEnterprise o = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r.EnterpriseId);
                if (o != null)
                {
                    FirebaseService firebase = new FirebaseService();
                    // Đẩy ảnh lên firebase storage
                    if (imagelogo != null)
                    {
                        using var stream = imagelogo.OpenReadStream();
                        var fileName = Path.GetFileName(imagelogo.FileName);
                        var imageUrl = await firebase.UploadImageToFirebaseAsync(stream, fileName, o.EnterpriseId.ToString() + "logo", "company");

                        if (imageUrl == "error")
                        {
                            Console.WriteLine("sai dinh dang anh");
                            return RedirectToAction("CompanyProfile", "Company", new { error = $"Logo chỉ hỗ trợ định dạng.jpg, .jpeg và .png.!" });
                        }
                        Console.WriteLine("upload anh logo vao firebase thanh cong");
                        o.LogoUrl = imageUrl;
                    }
                    if (imagebackground != null)
                    {
                        using var stream = imagebackground.OpenReadStream();
                        var fileName = Path.GetFileName(imagebackground.FileName);
                        var imageUrl = await firebase.UploadImageToFirebaseAsync(stream, fileName, o.EnterpriseId.ToString() + "background", "company");

                        if (imageUrl == "error")
                        {
                            Console.WriteLine("sai dinh dang anh");
                            return RedirectToAction("CompanyProfile", "Company", new { error = $"Background chỉ hỗ trợ định dạng.jpg, .jpeg và .png.!" });
                        }
                        Console.WriteLine("upload anh background vao firebase thanh cong");
                        o.CoverImgUrl = imageUrl;
                    }
                }
                else return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy công ty!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
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
                    Console.WriteLine("cat nhat thong tin cong ty thanh cong");
                    return RedirectToAction("CompanyProfile", "Company", new { success = "Cật nhật thông tin công ty thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Company Profile 2: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else if(type == 3)
            {
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatecompany";

                JobSeekerRecruiterProfile r = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
                if (r == null) return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy hồ sơ recruiter!" });

                JobSeekerEnterprise o = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r.EnterpriseId);
                if (o != null)
                {
                    if (facebookurl != null) o.FacebookUrl = facebookurl;
                    if (linkedinurl != null) o.LinkedinUrl = linkedinurl;
                    if (websiteurl != null) o.WebsiteUrl = websiteurl;
                }
                else return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy công ty!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
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
                    Console.WriteLine("cat nhat thong tin cong ty thanh cong");
                    return RedirectToAction("CompanyProfile", "Company", new { success = "Cật nhật thông tin công ty thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Company Profile 3: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                #region validate
                if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district) || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(address)) return RedirectToAction("CompanyProfile", "Company", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatecompany";

                JobSeekerRecruiterProfile r = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
                if (r == null) return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy hồ sơ recruiter!" });

                JobSeekerEnterprise o = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r.EnterpriseId);
                if (o != null)
                {
                    o.City = city;
                    o.District = district;
                    o.Ward = ward;
                    o.Address = address;
                }
                else return RedirectToAction("CompanyProfile", "Company", new { error = "Không tìm thấy công ty!" });

                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
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
                    Console.WriteLine("cat nhat thong tin cong ty thanh cong");
                    return RedirectToAction("CompanyProfile", "Company", new { success = "Cật nhật thông tin công ty thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Company Profile 4: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CompanyProfile", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> PostJob(string? error, string? success)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            ViewBag.Jobcategory = await GetJobCategory();
            ViewBag.Joblevel = await GetJobLevel();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PostJob(string title, string quantity, string category, string worktype, string exp, string level, string salarymin, string salarymax, string degree, string gender, string city, string district, string ward, DateTime expiredtime, string address, string description, string require, string benefit, string keyword)
        {
            string json = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            JobSeekerUserLoginDatum user = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(json);

            JobSeekerRecruiterProfile r1 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
            if (r1 == null) return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy hồ sơ nhà tuyển dụng!" });

            JobSeekerEnterprise e1 = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r1.EnterpriseId);
            if (e1 == null) return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy công ty!" });

            if (e1.IsCensorship == null || e1.IsCensorship == false) return RedirectToAction("ListPostJob", "Company", new { error = "Công ty chưa được kiểm duyệt!" });

            #region validate
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(worktype) || string.IsNullOrEmpty(exp) || string.IsNullOrEmpty(salarymin) | string.IsNullOrEmpty(salarymax) || string.IsNullOrEmpty(degree) || string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district) || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(require) || string.IsNullOrEmpty(benefit)|| string.IsNullOrEmpty(keyword) || expiredtime == null) 
                return RedirectToAction("PostJob","Company", new {error = "Vui lòng nhập đầy đủ!"});

            if (DateOnly.FromDateTime(expiredtime) < DateOnly.FromDateTime(DateTime.Now)) return RedirectToAction("PostJob", "Company", new { error = "Ngày hết hạn không hợp lệ!" });

            if (int.Parse(salarymin) > int.Parse(salarymax)) return RedirectToAction("PostJob", "Company", new { error = "Mức lương tối đa phải lớn hơn tối thiểu!" });
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/company/addpostjob";

            JobSeekerRecruiterProfile recruiter = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
            if(recruiter == null) return RedirectToAction("PostJob", "Company", new { error = "Không tìm thấy thông tin nhà tuyển dụng!" });

            JobSeekerEnterprise enterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == recruiter.EnterpriseId);
            if (enterprise == null) return RedirectToAction("PostJob", "Company", new { error = "Không tìm thấy thông tin công ty!" });

            JobSeekerJobPosting o = new JobSeekerJobPosting();
            o.Id = Guid.NewGuid();
            o.JobTitle = title;
            o.Quantity = int.Parse(quantity);
            o.JobCategoryId = int.Parse(category);
            o.WorkingType = worktype;
            o.ExpRequirement = exp;
            o.JobLevelCode = int.Parse(level);
            o.SalaryMin = int.Parse(salarymin);
            o.SalaryMax = int.Parse(salarymax);
            o.AcademicLevel = degree;
            o.GenderRequire = gender;
            o.Province = city;
            o.District = district;
            o.Ward = ward;
            o.ExpiredTime = expiredtime;
            o.Address = address;
            o.JobDesc = description;
            o.JobRequirement = require;
            o.BenefitEnjoyed = benefit;
            o.KeyWord = keyword;
            o.EnterpriseId = enterprise.EnterpriseId;
            o.StatusCode = "SC7";
            o.IsCreatedAt = DateTime.Now;
            o.IsUpdatedAt = DateTime.Now;
            o.ViewCount = 0;


            // Convert đối tượng thành JSON
            var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), Encoding.UTF8, "application/json");

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
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
                Console.WriteLine("add job post thanh cong");
                return RedirectToAction("PostJob", "Company", new { success = "Tạo bài đăng tuyển thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("PostJob", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Post Job: add that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("PostJob", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        public async Task<IActionResult> ListPostJob(string? error, string? success, int page = 1)
        {
            string json = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Login", "Account", new {error = "Đăng nhập hết hạn!" });
            }
            JobSeekerUserLoginDatum user = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(json);

            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            JobSeekerRecruiterProfile re = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
            if (re == null) return View("Error");

            ViewBag.JobApply = await GetListJobApply();
            ViewBag.Jobcategory = await GetJobCategory();
            ViewBag.Joblevel = await GetJobLevel();

            if (TempData.ContainsKey("JobPostings"))
            {
                string jobPostingsJson = TempData["JobPostings"] as string;
                List<JobSeekerJobPosting> data = JsonConvert.DeserializeObject<List<JobSeekerJobPosting>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = await GetListPostJob();

                    data = data.Where(p => p.EnterpriseId == re.EnterpriseId).ToList();

                    PaginatedList<JobSeekerJobPosting> paginatedJobs2 = PaginatedList<JobSeekerJobPosting>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerJobPosting> paginatedJobs1 = PaginatedList<JobSeekerJobPosting>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }
            List<JobSeekerJobPosting> a = await GetListPostJob();

            a = a.Where(p => p.EnterpriseId == re.EnterpriseId).ToList();

            PaginatedList<JobSeekerJobPosting> paginatedJobs = PaginatedList<JobSeekerJobPosting>.Create(a, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> ListPostJob(string type, string search, string status, Guid id, string title, string quantity, string category, string worktype, string exp, string level, string salarymin, string salarymax, string degree, string gender, string city, string district, string ward, DateTime expiredtime, string address, string description, string require, string benefit, string keyword)
        {
            string json = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            JobSeekerUserLoginDatum user = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(json);

            JobSeekerRecruiterProfile r1 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.Id);
            if (r1 == null) return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm hồ sơ nhà tuyển dụng!" });

            JobSeekerEnterprise e1 = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == r1.EnterpriseId);
            if (e1 == null) return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy công ty!" });

            if (e1.IsCensorship == null || e1.IsCensorship == false) return RedirectToAction("ListPostJob", "Company", new { error = "Công ty chưa được kiểm duyệt!" });

            if (type == "1")
            {
                #region validate
                if (string.IsNullOrEmpty(status)) return RedirectToAction("ListPostJob", "Company", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                List<JobSeekerJobPosting> a = await GetListPostJob();

                a = a.Where(p => p.EnterpriseId == r1.EnterpriseId).ToList();

                if (search != null) a = a.Where(p => p.JobTitle.Contains(search)).ToList();

                switch (status)
                {
                    case "all":
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ListPostJob", "Company");
                        else return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy các bài đăng!" });
                    case "SC5":
                        a = a.Where(p => p.StatusCode == status).ToList();
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ListPostJob", "Company");
                        else return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy các bài đăng!" });
                    case "SC6":
                        a = a.Where(p => p.StatusCode == status).ToList();
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ListPostJob", "Company");
                        else return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy các bài đăng!" });
                    case "SC7":
                        a = a.Where(p => p.StatusCode == status).ToList();
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ListPostJob", "Company");
                        else return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy các bài đăng!" });
                    default:
                            return RedirectToAction("ListPostJob", "Company", new { error = "Có vấn đề gì đó ở filter status!" });
                }
            }
            else
            {
                #region validate
                if (id == null || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(worktype) || string.IsNullOrEmpty(exp) || string.IsNullOrEmpty(salarymin) | string.IsNullOrEmpty(salarymax) || string.IsNullOrEmpty(degree) || string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district) || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(require) || string.IsNullOrEmpty(benefit) || string.IsNullOrEmpty(keyword) || expiredtime == null)
                    return RedirectToAction("ListPostJob", "Company", new { error = "Vui lòng nhập đầy đủ!" });

                if (DateOnly.FromDateTime(expiredtime) < DateOnly.FromDateTime(DateTime.Now)) return RedirectToAction("ListPostJob", "Company", new { error = "Ngày hết hạn không hợp lệ!" });

                if (int.Parse(salarymin) > int.Parse(salarymax)) return RedirectToAction("ListPostJob", "Company", new { error = "Mức lương tối đa phải lớn hơn tối thiểu!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatepostjob";
                Console.WriteLine(id);
                JobSeekerJobPosting o = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == id);
                if (o == null) return RedirectToAction("ListPostJob", "Company", new { error = "Không tìm thấy thông tin bài đăng!" });
                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

                o.JobTitle = title;
                o.Quantity = int.Parse(quantity);
                o.JobCategoryId = int.Parse(category);
                o.WorkingType = worktype;
                o.ExpRequirement = exp;
                o.JobLevelCode = int.Parse(level);
                o.SalaryMin = int.Parse(salarymin);
                o.SalaryMax = int.Parse(salarymax);
                o.AcademicLevel = degree;
                o.GenderRequire = gender;
                o.Province = city;
                o.District = district;
                o.KeyWord = keyword;
                o.Ward = ward;
                o.ExpiredTime = expiredtime;
                o.Address = address;
                o.JobDesc = description;
                o.JobRequirement = require;
                o.BenefitEnjoyed = benefit;
                o.IsUpdatedAt = DateTime.Now;


                // Convert đối tượng thành JSON
                var content = new StringContent(JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.UTF8, "application/json");

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
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
                    Console.WriteLine("update job post thanh cong");
                    return RedirectToAction("ListPostJob", "Company", new { success = "Cật nhật bài đăng tuyển thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ListPostJob", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("List Post Job: update that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ListPostJob", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeletePostJob(Guid id)
        {
            #region validate
            if (id == null) return RedirectToAction("ListPostJob", "Company", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/company/deletepostjob/{id}";

            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu DELETE tới API
            HttpResponseMessage response = await client.DeleteAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("delete job post thanh cong");
                return RedirectToAction("ListPostJob", "Company", new { success = "Xóa bài đăng tuyển thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("ListPostJob", "Company", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Delete Post Job: delete that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("ListPostJob", "Company", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        //Lấy danh sách listjobpostapplybyidpostjob
        public async Task<List<JobSeekerJobPostingApply>> GetListJobApply()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/company/getlistjobapply";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerJobPostingApply> j = new List<JobSeekerJobPostingApply>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerJobPostingApply>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("get GetListJobApply thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetListJobApply: lay jobapply that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return null;
            }
        }
        //Lấy danh sách listjobpost
        public async Task<List<JobSeekerJobPosting>> GetListPostJob()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/company/getlistpostjob";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerJobPosting> j = new List<JobSeekerJobPosting>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerJobPosting>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("get listJobPosting thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetJobLevel: lay joblevel that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return null;
            }
        }
        //Lấy danh sách leveljob
        public async Task<List<JobSeekerJobLevel>> GetJobLevel()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/job/getjoblevel";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerJobLevel> j = new List<JobSeekerJobLevel>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerJobLevel>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("get jobcategory thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetJobLevel: lay joblevel that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return null;
            }
        }
        //Lấy danh sách danh mục
        public async Task<List<JobSeekerJobCategory>> GetJobCategory()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/job/getjobcategory";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerJobCategory> j = new List<JobSeekerJobCategory>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerJobCategory>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("get jobcategory thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetJobCategory: lay jobcategory that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return null;
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
            string pattern = @"^0\d{9}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(phoneNumber);
        }
    }
}
