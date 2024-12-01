using Data_JobWeb.Entity;
using FE_JobWeb.Models;
using Google.Apis.Storage.v1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using FE_JobWeb.Others;
using Microsoft.AspNetCore.Http;

namespace FE_JobWeb.Controllers
{
    public class HomeController : Controller
    {
        private JobSeekerContext db = new JobSeekerContext();
        private readonly ILogger<HomeController> _logger;
        int pageSize = 10;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexCandidate()
        {
            return View();
        }
        public IActionResult IndexRecruiter()
        {
            return View();
        }
        public async Task<IActionResult> IndexAdmin(string? error, string? success)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            // Chạy đồng thời các tác vụ
            var jobPostingTask = GetListPostJob();
            var userTask = GetListUser();
            var enterpriseTask = GetListEnterprise();
            var jobApplyTask = GetListJobApply();
            var jobCategoryTask = GetJobCategory();
            var jobLevelTask = GetJobLevel();
            var notificationAdmin = GetListNotificationAdmin();
            await Task.WhenAll(jobPostingTask, userTask, enterpriseTask, jobApplyTask, jobCategoryTask, jobLevelTask, notificationAdmin);

            List<JobSeekerUserLoginDatum> user = await userTask;
            List<JobSeekerEnterprise> enterprise = await enterpriseTask;
            List<JobSeekerJobPosting> jobPosting = await jobPostingTask;
            List<JobSeekerJobPostingApply> jobApply = await jobApplyTask;
            List<JobSeekerJobCategory> jobCategory = await jobCategoryTask;
            List<JobSeekerJobLevel> jobLevel = await jobLevelTask;
            List<JobSeekerNotification> notificationAd = await notificationAdmin;

            ViewBag.Totaljobcategory = jobCategory.Count();
            ViewBag.Totaljob = jobPosting.Count();
            ViewBag.Totaljobnotapprove = jobPosting.Count(p => p.StatusCode == "SC7");
            ViewBag.Totalcandidate = user.Count(p => p.RoleId == 3);
            ViewBag.Totalrecruiter = user.Count(p => p.RoleId == 2);
            ViewBag.Totalenterprise = enterprise.Count();

            ViewBag.JobApply = jobApply;
            ViewBag.Jobcategory = jobCategory;
            ViewBag.Joblevel = jobLevel;
            ViewBag.NotificationAdmin = notificationAd;

            ViewBag.Account = user.OrderByDescending(p => p.IsCreatedAt).Where(p => p.RoleId == 3 || p.RoleId == 2).Take(5).ToList();
            ViewBag.Jobpost = jobPosting.OrderByDescending(p => p.IsCreatedAt).Take(5).ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> IndexAdmin(Guid id, string statuscode)
        {
            #region validate
            if (string.IsNullOrEmpty(statuscode) || id == null) return RedirectToAction("IndexAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/company/updatepostjob";
            JobSeekerJobPosting o = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == id);
            if (o == null) return RedirectToAction("IndexAdmin", "Home", new { error = "Không tìm thấy thông tin bài đăng!" });

            JobSeekerEnterprise jobSeekerEnterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == o.EnterpriseId);
            if (jobSeekerEnterprise == null) return RedirectToAction("IndexAdmin", "Home", new { error = "Không tìm thấy thông tin công ty của bài đăng!" });
            if (jobSeekerEnterprise.IsCensorship == false) return RedirectToAction("IndexAdmin", "Home", new { error = "Công ty đăng bài chưa được phê duyệt!" });

            if(o.StatusCode == statuscode) return RedirectToAction("IndexAdmin", "Home", new { error = "Không có gì thay đổi!" });

            Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            o.StatusCode = statuscode;
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

                NotificationPostJob(o.Id);

                return RedirectToAction("ManageJobPostingAdmin", "Home", new { success = "Cật nhật trạng thái bài đăng tuyển thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("IndexAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("List Post Job: update that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("IndexAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }
        }
        public async Task<IActionResult> ManageCompanyAdmin(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            ViewBag.Jobfield = await GetJobfield();

            if (TempData.ContainsKey("Enterprises"))
            {
                string jobPostingsJson = TempData["Enterprises"] as string;
                List<JobSeekerEnterprise> data = JsonConvert.DeserializeObject<List<JobSeekerEnterprise>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = await GetListEnterprise();

                    PaginatedList<JobSeekerEnterprise> paginatedJobs2 = PaginatedList<JobSeekerEnterprise>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerEnterprise> paginatedJobs1 = PaginatedList<JobSeekerEnterprise>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            List<JobSeekerEnterprise> enterprise = await GetListEnterprise();
            enterprise = enterprise.OrderByDescending(p => p.IsCreatedAt).ToList();

            PaginatedList<JobSeekerEnterprise> paginatedJobs = PaginatedList<JobSeekerEnterprise>.Create(enterprise, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> ManageCompanyAdmin(string type, string search, string censor, Guid id, string censor1)
        {
            if (type == "1")
            {
                #region validate
                if (string.IsNullOrEmpty(censor)) return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                List<JobSeekerEnterprise> a = await GetListEnterprise();
                if (search != null) a = a.Where(p => p.FullCompanyName.Contains(search) || p.CompanyEmail.Contains(search) || p.CompanyPhoneContact.Contains(search)).ToList();

                switch (censor)
                {
                    case "all":
                        TempData["Enterprises"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageCompanyAdmin", "Home");
                        else return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không tìm thấy các công ty!" });
                    case "false":
                        a = a.Where(p => p.IsCensorship == false).ToList();
                        TempData["Enterprises"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageCompanyAdmin", "Home");
                        else return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không tìm thấy các công ty!" });
                    case "true":
                        a = a.Where(p => p.IsCensorship == true).ToList();
                        TempData["Enterprises"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageCompanyAdmin", "Home");
                        else return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không tìm thấy các công ty!" });
                    default:
                        return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Có vấn đề gì đó ở filter status!" });
                }
            }
            else
            {
                #region validate
                if (string.IsNullOrEmpty(censor1) || id == null) return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion
                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatecompany";

                JobSeekerEnterprise o = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == id);
                if (o != null)
                {
                    if (censor1 == "true" && o.IsCensorship == true) return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không có gì thay đổi!" });
                    if (censor1 == "false" && o.IsCensorship == false) return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không có gì thay đổi!" });
                    if (censor1 == "true") o.IsCensorship = true;
                    if (censor1 == "false") o.IsCensorship = false;
                }
                else return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không tìm thấy công ty!" });

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
                    NotificationCompany(o.EnterpriseId);
                    return RedirectToAction("ManageCompanyAdmin", "Home", new { success = "Cật nhật trạng thái phê duyệt thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("ManageCompanyAdmin: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> ManageJobPostingAdmin(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            // Chạy đồng thời các tác vụ
            var jobPostingTask = GetListPostJob();
            var jobCategoryTask = GetJobCategory();
            var jobLevelTask = GetJobLevel();
            await Task.WhenAll(jobPostingTask, jobCategoryTask, jobLevelTask);

            List<JobSeekerJobCategory> jobCategory = await jobCategoryTask;
            List<JobSeekerJobLevel> jobLevel = await jobLevelTask;
            List<JobSeekerJobPosting> jobPosting = await jobPostingTask;

            ViewBag.Jobcategory = jobCategory;
            ViewBag.Joblevel = jobLevel;

            jobPosting = jobPosting.OrderByDescending(p => p.IsCreatedAt).ToList();

            if (TempData.ContainsKey("JobPostings"))
            {
                string jobPostingsJson = TempData["JobPostings"] as string;
                List<JobSeekerJobPosting> data = JsonConvert.DeserializeObject<List<JobSeekerJobPosting>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = jobPosting;

                    PaginatedList<JobSeekerJobPosting> paginatedJobs2 = PaginatedList<JobSeekerJobPosting>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerJobPosting> paginatedJobs1 = PaginatedList<JobSeekerJobPosting>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            PaginatedList<JobSeekerJobPosting> paginatedJobs = PaginatedList<JobSeekerJobPosting>.Create(jobPosting, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> ManageJobPostingAdmin(string type, string search, string status, Guid id, string statuscode)
        {
            if (type == "1")
            {
                #region validate
                if (string.IsNullOrEmpty(status)) return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                List<JobSeekerJobPosting> a = await GetListPostJob();
                if (search != null) a = a.Where(p => p.JobTitle.Contains(search)).ToList();

                switch (status)
                {
                    case "all":
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageJobPostingAdmin", "Home");
                        else return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy các bài đăng!" });
                    case "SC5":
                        a = a.Where(p => p.StatusCode == status).ToList();
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageJobPostingAdmin", "Home");
                        else return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy các bài đăng!" });
                    case "SC6":
                        a = a.Where(p => p.StatusCode == status).ToList();
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageJobPostingAdmin", "Home");
                        else return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy các bài đăng!" });
                    case "SC7":
                        a = a.Where(p => p.StatusCode == status).ToList();
                        TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                        if (a.Any()) return RedirectToAction("ManageJobPostingAdmin", "Home");
                        else return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy các bài đăng!" });
                    default:
                        return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Có vấn đề gì đó ở filter status!" });
                }
            }
            else
            {
                #region validate
                if (string.IsNullOrEmpty(statuscode) || id == null) return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatepostjob";
                JobSeekerJobPosting o = db.JobSeekerJobPostings.FirstOrDefault(p => p.Id == id);
                if (o == null) return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy thông tin bài đăng!" });

                JobSeekerEnterprise jobSeekerEnterprise = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == o.EnterpriseId);
                if (jobSeekerEnterprise == null) return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy thông tin công ty của bài đăng!" });
                if (jobSeekerEnterprise.IsCensorship == false) return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Công ty đăng bài chưa được phê duyệt!" });

                if (o.StatusCode == statuscode) return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không có gì thay đổi!" });

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

                o.StatusCode = statuscode;
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

                    NotificationPostJob(o.Id);

                    return RedirectToAction("ManageJobPostingAdmin", "Home", new { success = "Cật nhật trạng thái bài đăng tuyển thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("ManageJobPostingAdmin: update that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> ManageCategoryAdmin(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            List<JobSeekerJobCategory> u = await GetJobCategory();

            u = u.OrderByDescending(p => p.IsCreatedAt).ToList();

            if (TempData.ContainsKey("JobCategorys"))
            {
                string jobPostingsJson = TempData["JobCategorys"] as string;
                List<JobSeekerJobCategory> data = JsonConvert.DeserializeObject<List<JobSeekerJobCategory>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = u;

                    PaginatedList<JobSeekerJobCategory> paginatedJobs2 = PaginatedList<JobSeekerJobCategory>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerJobCategory> paginatedJobs1 = PaginatedList<JobSeekerJobCategory>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            PaginatedList<JobSeekerJobCategory> paginatedJobs = PaginatedList<JobSeekerJobCategory>.Create(u, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> ManageCategoryAdmin(string type, string nameadd, string search, string idedit, string nameedit, string iddelete)
        {
            if (type == "1")
            {
                #region validate
                if (string.IsNullOrEmpty(search)) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                List<JobSeekerJobCategory> a = await GetJobCategory();
                if (search != null) a = a.Where(p => p.JobCategoryName.Contains(search)).ToList();

                if (a.Any())
                {
                    TempData["JobCategorys"] = JsonConvert.SerializeObject(a);
                    return RedirectToAction("ManageCategoryAdmin", "Home");
                }
                else return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Không tìm thấy các nghề nghiệp!" });
            }
            else if(type == "2")
            {
                #region validate
                if (string.IsNullOrEmpty(nameadd)) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/job/addjobcategory";

                JobSeekerJobCategory o = new JobSeekerJobCategory();
                o.JobCategoryName = nameadd;
                o.IsCreatedAt = DateTime.Now;
                o.IsUpdatedAt = DateTime.Now;

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

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
                    Console.WriteLine("add category thanh cong");
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { success = "Thêm ngành nghề thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("ManageCategoryAdmin: add that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else if(type == "3")
            {
                #region validate
                if (string.IsNullOrEmpty(nameedit) || string.IsNullOrEmpty(idedit)) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/job/updatejobcategory";

                JobSeekerJobCategory o = db.JobSeekerJobCategories.FirstOrDefault(p => p.Id == int.Parse(idedit));
                if (o == null) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Không tìm thấy thông tin ngành nghề!" });
                if (o.JobCategoryName == nameedit) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Không có gì thay đổi!" });

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

                o.JobCategoryName = nameedit;
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
                    Console.WriteLine("update category thanh cong");
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { success = "Cật nhật ngành nghề thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("ManageCategoryAdmin: update that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                #region validate
                if (iddelete == null) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                //List<JobSeekerJobPosting> check = db.JobSeekerJobPostings.Where(p => p.JobCategoryId == int.Parse(iddelete)).ToList();
                //if(check.Any()) return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Mục này đang được sử dụng, vui lòng xử lý dữ liệu liên quan trước khi xóa!" });

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = $"http://localhost:5281/api/job/deletejobcategory/{iddelete}";

                //get with token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu DELETE tới API
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("delete category thanh cong");
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { success = "Xóa ngành nghề thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Delete Category: delete that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageCategoryAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> ManageJobFieldAdmin(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            List<JobSeekerJobField> u = await GetJobfield();

            if (TempData.ContainsKey("JobFields"))
            {
                string jobPostingsJson = TempData["JobFields"] as string;
                List<JobSeekerJobField> data = JsonConvert.DeserializeObject<List<JobSeekerJobField>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = u;

                    PaginatedList<JobSeekerJobField> paginatedJobs2 = PaginatedList<JobSeekerJobField>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerJobField> paginatedJobs1 = PaginatedList<JobSeekerJobField>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            PaginatedList<JobSeekerJobField> paginatedJobs = PaginatedList<JobSeekerJobField>.Create(u, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> ManageJobFieldAdmin(string type, string nameadd, string search, string idedit, string nameedit, string iddelete)
        {
            if (type == "1")
            {
                #region validate
                if (string.IsNullOrEmpty(search)) return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                List<JobSeekerJobField> a = await GetJobfield();
                if (search != null) a = a.Where(p => p.JobFieldName.Contains(search)).ToList();

                if (a.Any())
                {
                    TempData["JobFields"] = JsonConvert.SerializeObject(a);
                    return RedirectToAction("ManageJobFieldAdmin", "Home");
                }
                else return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Không tìm thấy các lĩnh vực!" });
            }
            else if (type == "2")
            {
                #region validate
                if (string.IsNullOrEmpty(nameadd)) return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/job/addjobfield";

                JobSeekerJobField o = new JobSeekerJobField();
                o.JobFieldName = nameadd;

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

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
                    Console.WriteLine("add jobfield thanh cong");
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { success = "Thêm lĩnh vực thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("ManageJobFieldAdmin: add that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else if (type == "3")
            {
                #region validate
                if (string.IsNullOrEmpty(nameedit) || string.IsNullOrEmpty(idedit)) return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/job/updatejobfield";

                JobSeekerJobField o = db.JobSeekerJobFields.FirstOrDefault(p => p.JobFieldId == int.Parse(idedit));
                if (o == null) return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Không tìm thấy thông tin lĩnh vực!" });
                if (o.JobFieldName == nameedit) return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Không có gì thay đổi!" });

                Console.WriteLine("json\n" + JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

                o.JobFieldName = nameedit;

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
                    Console.WriteLine("update jobfield thanh cong");
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { success = "Cật nhật lĩnh vực thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("ManageJobFieldAdmin: update that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                #region validate
                if (iddelete == null) return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = $"http://localhost:5281/api/job/deletejobfield/{iddelete}";

                //get with token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu DELETE tới API
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("delete jobfield thanh cong");
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { success = "Xóa lĩnh vực thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Delete Category: delete that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("ManageJobFieldAdmin", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> ManageAccountAdmin(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            // Chạy đồng thời các tác vụ
            List<JobSeekerUserLoginDatum> user = await GetListUser();

            user = user.OrderByDescending(p => p.IsCreatedAt).Where(p => p.RoleId == 3 || p.RoleId == 2).ToList();

            if (TempData.ContainsKey("Accounts"))
            {
                string jobPostingsJson = TempData["Accounts"] as string;
                List<JobSeekerUserLoginDatum> data = JsonConvert.DeserializeObject<List<JobSeekerUserLoginDatum>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = user;

                    PaginatedList<JobSeekerUserLoginDatum> paginatedJobs2 = PaginatedList<JobSeekerUserLoginDatum>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerUserLoginDatum> paginatedJobs1 = PaginatedList<JobSeekerUserLoginDatum>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            PaginatedList<JobSeekerUserLoginDatum> paginatedJobs = PaginatedList<JobSeekerUserLoginDatum>.Create(user, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> ManageAccountAdmin(string search, string status, string type)
        {
            #region validate
            if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(type)) return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            List<JobSeekerUserLoginDatum> a = await GetListUser();
            a = a.OrderByDescending(p => p.IsCreatedAt).Where(p => p.RoleId == 3 || p.RoleId == 2).ToList();
            if (search != null) a = a.Where(p => p.FullName.Contains(search) || p.Email.Contains(search)).ToList();

            switch (status)
            {
                case "all":
                    break;
                case "SC8":
                    a = a.Where(p => p.StatusCode == status).ToList();
                    if (!a.Any()) return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Không tìm thấy các tài khoản!" });
                    break;
                case "SC9":
                    a = a.Where(p => p.StatusCode == status).ToList();
                    if (!a.Any()) return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Không tìm thấy các tài khoản!" });
                    break;
                default:
                    return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Có vấn đề gì đó ở filter status!" });
            }
            switch (type)
            {
                case "all":
                    TempData["Accounts"] = JsonConvert.SerializeObject(a);
                    if (a.Any()) return RedirectToAction("ManageAccountAdmin", "Home");
                    else return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Không tìm thấy các tài khoản!" });
                case "2":
                    a = a.Where(p => p.RoleId == 2).ToList();
                    TempData["Accounts"] = JsonConvert.SerializeObject(a);
                    if (a.Any()) return RedirectToAction("ManageAccountAdmin", "Home");
                    else return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Không tìm thấy các tài khoản!" });
                case "3":
                    a = a.Where(p => p.RoleId == 3).ToList();
                    TempData["Accounts"] = JsonConvert.SerializeObject(a);
                    if (a.Any()) return RedirectToAction("ManageAccountAdmin", "Home");
                    else return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Không tìm thấy các tài khoản!" });
                default:
                    return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Có vấn đề gì đó ở filter type!" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        //Gửi thông báo đến recruiter về company
        public async Task NotificationCompany(Guid idcompany)
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/notification/notificationcompany/{idcompany}";

            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("NotificationCompany thanh cong");
                Console.WriteLine("Phản hồi từ API: " + await response.Content.ReadAsStringAsync());
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("NotificationCompany: sen mail that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                Console.WriteLine("Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!");
                return;
            }
        }
        //Gửi thông báo đến account có liên quan đến job/enterprise
        public async Task NotificationPostJob(Guid idjostposting)
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/notification/notificationpostjob/{idjostposting}";

            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("NotificationPostJob thanh cong");
                Console.WriteLine("Phản hồi từ API: " + await response.Content.ReadAsStringAsync());
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("NotificationPostJob: sen mail that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                Console.WriteLine("Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!");
                return;
            }
        }
        //Lấy danh sách thông báo của admin
        public async Task<List<JobSeekerNotification>> GetListNotificationAdmin()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/notification/listnotificationadmin";

            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerNotification> j = new List<JobSeekerNotification>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerNotification>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("get GetListNotificationAdmin thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetListNotificationAdmin: lay listnotificationadmin that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return new List<JobSeekerNotification>();
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
                return new List<JobSeekerJobPostingApply>();
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
                return new List<JobSeekerJobPosting>();
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
                return new List<JobSeekerJobLevel>();
            }
        }
        //Lấy danh sách company
        public async Task<List<JobSeekerEnterprise>> GetListEnterprise()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/company/getlistenterprise";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerEnterprise> j = new List<JobSeekerEnterprise>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerEnterprise>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("GetListEnterprise thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetListEnterprise: lay GetListEnterprise that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return new List<JobSeekerEnterprise>();
            }
        }
        //Lấy danh sách user
        public async Task<List<JobSeekerUserLoginDatum>> GetListUser()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/account/getlistuser";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<JobSeekerUserLoginDatum> j = new List<JobSeekerUserLoginDatum>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<JobSeekerUserLoginDatum>>(responseData);

                //ViewBag.Jobfield = j;
                Console.WriteLine("GetListUser thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GetListUser: lay GetListUser that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return new List<JobSeekerUserLoginDatum>();
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
                return new List<JobSeekerJobCategory>();
            }
        }//Lấy danh sách lĩnh vực
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
                return new List<JobSeekerJobField>();
            }
        }
    }
}
