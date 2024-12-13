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
using Data_JobWeb.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace FE_JobWeb.Controllers
{
    public class HomeController : Controller
    {
        private JobSeekerContext db = new JobSeekerContext();
        private readonly ILogger<HomeController> _logger;
        int pageSize = 9;
        private ApplicationUser user;

        public HomeController(ILogger<HomeController> logger, ApplicationUser user)
        {
            _logger = logger;
            this.user = user;
        }
        //Notification candidate, recruiter, admin
        public IActionResult Notification(Guid id, string? error, string? success)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            JobSeekerUserLoginDatum u = db.JobSeekerUserLoginData.FirstOrDefault(x => x.Id == id);
            if (u == null) View("Error");

            List<JobSeekerNotification> lnoti = new List<JobSeekerNotification>();

            if (u.RoleId == 1)
            {
                lnoti = db.JobSeekerNotifications.OrderByDescending(p => p.IsCreatedAt).Where(p => p.Type.Contains("admin")).ToList();
                return View(lnoti);
            }

            lnoti = db.JobSeekerNotifications.OrderByDescending(p => p.IsCreatedAt).Where(p => p.IdUserReceive == id).ToList();

            return View(lnoti);
        }
        public IActionResult NotificationForward(string id, string url)
        {
            #region validate
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(id)) return RedirectToAction("Notification", "Home", new { id = user.User.Id, error = "Có vấn đề khi chuyển tiếp thông báo!" });
            #endregion

            JobSeekerNotification noti = db.JobSeekerNotifications.FirstOrDefault(p => p.Id == id);
            if(noti == null) return RedirectToAction("Notification", "Home", new { id = user.User.Id, error = "Không tìm thấy thông báo!" });

            noti.IsSeen = true;
            db.SaveChanges();

            return Redirect(url);
        }
        //home
        public async Task<IActionResult> Index()
        {
            // Gọi đồng thời tất cả các API cần thiết
            var jobCategoriesTask = GetJobCategory();
            var enterprisesTask = GetListEnterprise();
            var jobPostingsTask = GetListPostJob();
            var citiesTask = GetCity();

            // Chờ tất cả các tác vụ hoàn thành
            await Task.WhenAll(jobCategoriesTask, enterprisesTask, jobPostingsTask, citiesTask);

            // Lấy kết quả từ các tác vụ
            var jobCategories = jobCategoriesTask.Result;
            var enterprises = enterprisesTask.Result;
            var jobPostings = jobPostingsTask.Result;
            var cities = citiesTask.Result;

            // Xử lý dữ liệu và lưu vào ViewBag
            ViewBag.Jobcategory = jobCategories;
            ViewBag.Category = jobCategories.Take(10).ToList();
            ViewBag.Company = enterprises.OrderByDescending(e => e.ViewCount).Take(10).ToList();
            ViewBag.Hochiminh = jobPostings.Count(p => p.Province == "Thành phố Hồ Chí Minh");
            ViewBag.Hanoi = jobPostings.Count(p => p.Province == "Thành phố Hà Nội");
            ViewBag.Danang = jobPostings.Count(p => p.Province == "Thành phố Đà Nẵng");
            ViewBag.City = cities;

            return View();
        }
        public async Task<IActionResult> CompanyHome(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            ViewBag.Jobfield = await GetJobfield();
            ViewBag.City = await GetCity();

            if (TempData.ContainsKey("comanyhomes"))
            {
                string jobPostingsJson = TempData["comanyhomes"] as string;
                List<JobSeekerEnterprise> data = JsonConvert.DeserializeObject<List<JobSeekerEnterprise>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = await GetListEnterprise();

                    data = data.Where(p => p.IsCensorship == true).ToList();

                    PaginatedList<JobSeekerEnterprise> paginatedJobs2 = PaginatedList<JobSeekerEnterprise>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerEnterprise> paginatedJobs1 = PaginatedList<JobSeekerEnterprise>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            List<JobSeekerEnterprise> enterprise = await GetListEnterprise();
            enterprise = enterprise.Where(p => p.IsCensorship == true).ToList();

            PaginatedList<JobSeekerEnterprise> paginatedJobs = PaginatedList<JobSeekerEnterprise>.Create(enterprise, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> CompanyHome(string search, string jobfield, string city)
        {
            #region validate
            if (string.IsNullOrEmpty(jobfield) || string.IsNullOrEmpty(city)) return RedirectToAction("CompanyHome", "Home", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            List<JobSeekerEnterprise> a = await GetListEnterprise();
            a = a.Where(p => p.IsCensorship == true).ToList();
            if (search != null) a = a.Where(p => p.FullCompanyName.Contains(search)).ToList();

            if(jobfield != "all") a = a.Where(p => p.JobFieldId == int.Parse(jobfield)).ToList();

            if (city != "all") a = a.Where(p => p.City == city).ToList();

            if (a.Any())
            {
                TempData["comanyhomes"] = JsonConvert.SerializeObject(a);
                return RedirectToAction("CompanyHome", "Home");
            }
            else return RedirectToAction("CompanyHome", "Home", new { error = "Không tìm thấy các công ty!" });
        }
        public async Task<IActionResult> CompanyDetailHome(Guid id, string? error, string? success)
        {
            if (id == Guid.Empty) return View("Error");

            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/company/getcompanybyid/{id}";

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                JobSeekerEnterprise j = new JobSeekerEnterprise();

                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng 
                j = JsonConvert.DeserializeObject<JobSeekerEnterprise>(responseData);

                Console.WriteLine("get company thanh cong");

                // Chạy các tác vụ đồng thời
                var jobFieldTask = GetJobfield();
                var listCompanyTask = GetListEnterprise();
                var listJobTask = GetListPostJob();

                // Chờ tất cả các tác vụ hoàn thành
                await Task.WhenAll(listCompanyTask, jobFieldTask, listJobTask);

                List<JobSeekerJobPosting> list = await listJobTask;
                list = list.OrderByDescending(p => p.IsUpdatedAt).Where(p => p.EnterpriseId == j.EnterpriseId && p.StatusCode == "SC5").Take(3).ToList();

                // Gán kết quả sau khi tất cả hoàn thành
                ViewBag.Jobfield = await jobFieldTask;
                ViewBag.Company = await listCompanyTask;
                ViewBag.Listjob = list;

                return View(j);
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("CompanyDetailHomeAsync: not found company that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                Console.WriteLine("Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!");
                return View("Error");
            }
        }
        public async Task<IActionResult> CompanyHomeFollow(int type, Guid id)
        {
            if (type == 1)
            {
                #region validate
                if (id == Guid.Empty) return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Không tìm thấy id company!" });
                #endregion

                if (user.User == null) return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Vui lòng đăng nhập để thực hiện chức năng này!" });

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/addcompanfollow";

                JobSeekerEnterpriseFollowed o = new JobSeekerEnterpriseFollowed();
                o.CandidateId = user.User.Id;
                o.EnterpriseId = id;
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
                    Console.WriteLine("add companyfollow thanh cong");
                    return RedirectToAction("CompanyDetailHome", "Home", new { id = id, success = "Theo dõi công ty thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("CompanyDetailHome: add that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                #region validate
                if (id == Guid.Empty) return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Không tìm thấy id company!" });
                #endregion
                if (user.User == null) return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Vui lòng đăng nhập để thực hiện chức năng này!" });

                JobSeekerEnterpriseFollowed check = db.JobSeekerEnterpriseFolloweds.FirstOrDefault(p => p.EnterpriseId == id && p.CandidateId == user.User.Id);
                if (check == null) return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Không tìm thấy công ty đã theo dõi!" });

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = $"http://localhost:5281/api/company/deletecompanyfollow/{check.Id}";

                //get with token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu DELETE tới API
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("delete companyfollow thanh cong");
                    return RedirectToAction("CompanyDetailHome", "Home", new { id = id, success = "Bỏ theo dõi công ty thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { id = id, error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Delete companyfollow: delete that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CompanyDetailHome", "Home", new { id = id, error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> JobHome(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            // Chạy đồng thời các tác vụ
            var jobPostingTask = GetListPostJob();
            var jobCategoryTask = GetJobCategory();
            var jobLevelTask = GetJobLevel();
            var citytask = GetCity();

            await Task.WhenAll(jobPostingTask, citytask, jobCategoryTask, jobLevelTask);

            ViewBag.Jobcategory = await jobCategoryTask;
            ViewBag.Joblevel = await jobLevelTask;
            ViewBag.City = await citytask;

            if (TempData.ContainsKey("jobhomes"))
            {
                string jobPostingsJson = TempData["jobhomes"] as string;
                List<JobSeekerJobPosting> data = JsonConvert.DeserializeObject<List<JobSeekerJobPosting>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = await jobPostingTask;

                    data = data.OrderByDescending(p => p.IsUpdatedAt).Where(p => p.StatusCode == "SC5").ToList();

                    PaginatedList<JobSeekerJobPosting> paginatedJobs2 = PaginatedList<JobSeekerJobPosting>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerJobPosting> paginatedJobs1 = PaginatedList<JobSeekerJobPosting>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            List<JobSeekerJobPosting> job = await jobPostingTask;
            job = job.OrderByDescending(p => p.IsUpdatedAt).Where(p => p.StatusCode == "SC5").ToList();

            PaginatedList<JobSeekerJobPosting> paginatedJobs = PaginatedList<JobSeekerJobPosting>.Create(job, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpGet]
        public async Task<IActionResult> JobHomeSearch(string search, string category, string joblevel, string exp, string city, Guid id)
        {
            #region validate
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(joblevel) || string.IsNullOrEmpty(exp) || string.IsNullOrEmpty(city)) return RedirectToAction("JobHome", "Home", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            List<JobSeekerJobPosting> a = await GetListPostJob();
            a = a.OrderByDescending(p => p.IsUpdatedAt).Where(p => p.StatusCode == "SC5").ToList();
            if (search != null) a = a.Where(p => p.JobTitle.Contains(search)).ToList();

            if (category != "all") a = a.Where(p => p.JobCategoryId == int.Parse(category)).ToList();

            if (joblevel != "all") a = a.Where(p => p.JobLevelCode == int.Parse(joblevel)).ToList();

            if (exp != "all") a = a.Where(p => p.ExpRequirement == exp).ToList();

            if (city != "all") a = a.Where(p => p.Province == city).ToList();

            if (a.Any())
            {
                TempData["jobhomes"] = JsonConvert.SerializeObject(a);
                return RedirectToAction("JobHome", "Home");
            }
            else return RedirectToAction("JobHome", "Home", new { error = "Không tìm thấy các bài đăng!" });
        }
        //home - theo doi jobposting
        [HttpGet]
        public async Task<IActionResult> JobHomeSaveJob(int type, Guid id)
        {
            if(type == 1)
            {
                #region validate
                if (id == Guid.Empty) return RedirectToAction("JobHome", "Home", new { error = "Không tìm thấy id post!" });
                #endregion

                if (user.User == null) return RedirectToAction("JobHome", "Home", new { error = "Vui lòng đăng nhập để thực hiện chức năng này!" });

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/addsavejobpost";

                JobSeekerSavedJobPosting o = new JobSeekerSavedJobPosting();
                o.CandidateId = user.User.Id;
                o.JobPostingId = id;
                o.SavedAt = DateTime.Now;

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
                    Console.WriteLine("add savejob thanh cong");
                    return RedirectToAction("JobHome", "Home", new { success = "Theo dõi bài đăng thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("JobHome", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("JobHome: add that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("JobHome", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                #region validate
                if (id == Guid.Empty) return RedirectToAction("JobHome", "Home", new { error = "Không tìm thấy id post!" });
                #endregion
                if (user.User == null) return RedirectToAction("JobHome", "Home", new { error = "Vui lòng đăng nhập để thực hiện chức năng này!" });

                JobSeekerSavedJobPosting check = db.JobSeekerSavedJobPostings.FirstOrDefault(p => p.JobPostingId == id && p.CandidateId == user.User.Id);
                if (check == null) return RedirectToAction("JobHome", "Home", new { error = "Không tìm thấy bài đăng đã lưu!" });

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = $"http://localhost:5281/api/company/deletesavejobpost/{check.Id}";

                //get with token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu DELETE tới API
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("delete savejob thanh cong");
                    return RedirectToAction("JobHome", "Home", new { success = "Bỏ lưu bài đăng thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("JobHome", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Delete savejob: delete that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("JobHome", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public async Task<IActionResult> JobDetailHome(Guid id, string? error, string? success)
        {
            if (id == Guid.Empty) return View("Error");

            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/company/getpostjobbyid/{id}";

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                JobSeekerJobPosting j = new JobSeekerJobPosting();

                string responseData = await response.Content.ReadAsStringAsync();
                // Deserialization từ JSON sang danh sách đối tượng 
                j = JsonConvert.DeserializeObject<JobSeekerJobPosting>(responseData);

                Console.WriteLine("get jobpost thanh cong");

                // Chạy các tác vụ đồng thời
                var jobCategoryTask = GetJobCategory();
                var jobLevelTask = GetJobLevel();
                var listJobTask = GetListPostJob();

                // Chờ tất cả các tác vụ hoàn thành
                await Task.WhenAll(jobCategoryTask, jobLevelTask, listJobTask);

                List<JobSeekerJobPosting> list = await listJobTask;
                list = list.Where(p => p.JobLevelCode == j.JobLevelCode && p.StatusCode == "SC5").Take(5).ToList();

                // Gán kết quả sau khi tất cả hoàn thành
                ViewBag.Jobcategory = await jobCategoryTask;
                ViewBag.Joblevel = await jobLevelTask;
                ViewBag.Listjob = list;

                return View(j);
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("JobDetailHome: sen mail that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                Console.WriteLine("Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!");
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> JobDetailHome(Guid id, string name, string cvurl, string coverletter)
        {
            #region validate
            if (user.User == null) return RedirectToAction("JobDetailHome", "Home", new { id = id, error = "Vui lòng đăng nhập để thực hiện chức năng này!" });
            if (string.IsNullOrEmpty(cvurl)) return RedirectToAction("JobDetailHome", "Home", new { id = id, error = "Vui lòng thêm cv của bạn từ profile để thực hiện chức năng này!" });

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(cvurl) || id == Guid.Empty) return RedirectToAction("JobDetailHome", "Home", new { id = id, error = "Vui lòng nhập đầy đủ!" });
            #endregion

            JobSeekerJobPostingApply check = db.JobSeekerJobPostingApplies.FirstOrDefault(p => p.CandidateId == user.User.Id && p.JobPostingId == id);
            if (check != null) return RedirectToAction("JobDetailHome", "Home", new { id = id, error = "Bạn đã ứng tuyển bài đăng này rồi!" });

            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "http://localhost:5281/api/company/addjobapply";

            JobSeekerJobPostingApply o = new JobSeekerJobPostingApply();
            o.CandidateId = user.User.Id;
            o.JobPostingId = id;
            o.ApplyTime = DateTime.Now;
            o.StatusCode = "SC7";

            if(coverletter != null) o.CoverLetter = coverletter;

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
                Console.WriteLine("add applyjob thanh cong");
                return RedirectToAction("JobDetailHome", "Home", new { id = id, success = "Úng tuyển thành công!" });
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
            {
                Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
            {
                Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                return RedirectToAction("JobDetailHome", "Home", new { id = id, error = "Bạn không có quyền thực hiện thao tác này!" });
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("JobDetailHome: add that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                return RedirectToAction("JobDetailHome", "Home", new { id = id, error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
            }

        }

        //candidate
        public IActionResult IndexCandidate()
        {
            JobSeekerUserLoginDatum u = db.JobSeekerUserLoginData.FirstOrDefault(x => x.Id == user.User.Id);
            if (u == null) View("Error");

            ViewBag.Totalapply = db.JobSeekerJobPostingApplies.Where(p => p.CandidateId == user.User.Id).Count();

            ViewBag.Totalsave = db.JobSeekerSavedJobPostings.Where(p => p.CandidateId == user.User.Id).Count();

            ViewBag.Totalnotification = db.JobSeekerNotifications.OrderByDescending(p => p.IsCreatedAt).Where(p => p.IdUserReceive == user.User.Id && p.IsSeen == false).Count();

            ViewBag.Notification = db.JobSeekerNotifications.OrderByDescending(p => p.IsCreatedAt).Where(p => p.IdUserReceive == user.User.Id && p.IsSeen == false).Take(5).ToList();

            ViewBag.Jobapply = db.JobSeekerJobPostingApplies.OrderByDescending(p => p.ApplyTime).Where(p => p.CandidateId == user.User.Id).Take(5).ToList();

            ViewBag.Jobsave = db.JobSeekerSavedJobPostings.OrderByDescending(p => p.SavedAt).Where(p => p.CandidateId == user.User.Id).Take(5).ToList();

            return View();
        }
        public IActionResult JobApplyCandidate(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            if (TempData.ContainsKey("jobapplys"))
            {
                string jobPostingsJson = TempData["jobapplys"] as string;
                List<JobSeekerJobPostingApply> data = JsonConvert.DeserializeObject<List<JobSeekerJobPostingApply>>(jobPostingsJson);

                if (!data.Any())
                {
                    data = db.JobSeekerJobPostingApplies.Where(p => p.CandidateId == user.User.Id).ToList();

                    PaginatedList<JobSeekerJobPostingApply> paginatedJobs2 = PaginatedList<JobSeekerJobPostingApply>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                PaginatedList<JobSeekerJobPostingApply> paginatedJobs1 = PaginatedList<JobSeekerJobPostingApply>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            List<JobSeekerJobPostingApply> jobapply = db.JobSeekerJobPostingApplies.OrderByDescending(p => p.ApplyTime).Where(p => p.CandidateId == user.User.Id).ToList();

            PaginatedList<JobSeekerJobPostingApply> paginatedJobs = PaginatedList<JobSeekerJobPostingApply>.Create(jobapply, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public IActionResult JobApplyCandidate(string status)
        {
            #region validate
            if (string.IsNullOrEmpty(status)) return RedirectToAction("JobApplyCandidate", "Home", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            List<JobSeekerJobPostingApply> a = db.JobSeekerJobPostingApplies.Where(p => p.CandidateId == user.User.Id).ToList();

            if (status != "") a = a.Where(p => p.StatusCode == status).ToList();

            if (a.Any())
            {
                TempData["jobapplys"] = JsonConvert.SerializeObject(a);
                return RedirectToAction("JobApplyCandidate", "Home");
            }
            else return RedirectToAction("JobApplyCandidate", "Home", new { error = "Không tìm thấy các bài đăng đã ứng tuyển!" });
        }
        public IActionResult JobSaveCandidate(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            List<JobSeekerSavedJobPosting> jobapply = db.JobSeekerSavedJobPostings.OrderByDescending(p => p.SavedAt).Where(p => p.CandidateId == user.User.Id).ToList();

            PaginatedList<JobSeekerSavedJobPosting> paginatedJobs = PaginatedList<JobSeekerSavedJobPosting>.Create(jobapply, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpGet]
        public IActionResult JobSaveCandidateDelete(int id)
        {
            #region validate
            if (id == null) return RedirectToAction("JobSaveCandidate", "Home", new { error = "Vui lòng nhập đầy đủ!" });
            #endregion

            JobSeekerSavedJobPosting a = db.JobSeekerSavedJobPostings.FirstOrDefault(p => p.Id == id);
            if (a == null) return RedirectToAction("JobSaveCandidate", "Home", new { error = "Không tìm thấy bài post đã lưu!" });

            db.JobSeekerSavedJobPostings.Remove(a);
            db.SaveChanges();

            return RedirectToAction("JobSaveCandidate", "Home", new { success = "Hủy lưu thành công!" });
        }

        //recruiter
        public async Task<IActionResult> IndexRecruiter()
        {
            JobSeekerUserLoginDatum u = db.JobSeekerUserLoginData.FirstOrDefault(x => x.Id == user.User.Id);
            if (u == null) View("Error");

            JobSeekerRecruiterProfile re = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.User.Id);
            if (re == null) return View("Error");

            List<JobSeekerJobPosting> ljob = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == re.EnterpriseId).ToList();
            List<JobSeekerJobPostingApply> lapply = new List<JobSeekerJobPostingApply>();

            ViewBag.Totaljob = ljob.Count();

            ViewBag.Totalpendingjob = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == re.EnterpriseId && p.StatusCode == "SC&").Count();

            int d = 0;
            if(ljob.Count > 0)
            {
                foreach(JobSeekerJobPosting i in ljob)
                {
                    JobSeekerJobPostingApply j = db.JobSeekerJobPostingApplies.FirstOrDefault(p => p.JobPostingId == i.Id);
                    if (j != null)
                    {
                        d++;
                        lapply.Add(j);
                    }
                }
            }
            ViewBag.Totalcandidate = d;

            ViewBag.Totalnotification = db.JobSeekerNotifications.OrderByDescending(p => p.IsCreatedAt).Where(p => p.IdUserReceive == user.User.Id && p.IsSeen == false).Count();

            ViewBag.Notification = db.JobSeekerNotifications.OrderByDescending(p => p.IsCreatedAt).Where(p => p.IdUserReceive == user.User.Id && p.IsSeen == false).Take(5).ToList();

            ViewBag.JobApply = db.JobSeekerJobPostingApplies.ToList();
            ViewBag.Jobcategory = db.JobSeekerJobCategories.ToList();
            ViewBag.Joblevel = db.JobSeekerJobLevels.ToList();

            ViewBag.Job = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == re.EnterpriseId && p.StatusCode == "SC5").Take(5).ToList();

            lapply = lapply.OrderByDescending(p => p.ApplyTime).Take(5).ToList();

            ViewBag.Candidate = lapply;

            return View();
        }
        public IActionResult CandidateApplyRecruiter(string? error, string? success, int page = 1)
        {
            if (error != null) ViewBag.ErrorMessage = error;
            if (success != null) ViewBag.SuccessMessage = success;

            if (TempData.ContainsKey("candidateapplys"))
            {
                string jobPostingsJson = TempData["candidateapplys"] as string;
                List<JobSeekerJobPostingApply> data = JsonConvert.DeserializeObject<List<JobSeekerJobPostingApply>>(jobPostingsJson);

                if (!data.Any())
                {
                    JobSeekerRecruiterProfile rec1 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.User.Id);
                    if (rec1 == null) rec1 = new JobSeekerRecruiterProfile();

                    JobSeekerEnterprise enter1 = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == rec1.EnterpriseId);
                    if (enter1 == null) enter1 = new JobSeekerEnterprise();

                    List<JobSeekerJobPosting> jobpost1 = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == enter1.EnterpriseId).ToList();

                    ViewBag.Jobpost = jobpost1;

                    if (jobpost1.Count > 0)
                    {
                        foreach (JobSeekerJobPosting i in jobpost1)
                        {
                            List<JobSeekerJobPostingApply> j = db.JobSeekerJobPostingApplies.Where(p => p.JobPostingId == i.Id).ToList();
                            if (!j.Any()) continue;
                            foreach (JobSeekerJobPostingApply ii in j)
                            {
                                data.Add(ii);
                            }
                        }
                    }

                    PaginatedList<JobSeekerJobPostingApply> paginatedJobs2 = PaginatedList<JobSeekerJobPostingApply>.Create(data, page, pageSize);

                    return View(paginatedJobs2);
                }

                JobSeekerRecruiterProfile rec11 = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.User.Id);
                if (rec11 == null) rec11 = new JobSeekerRecruiterProfile();

                JobSeekerEnterprise enter11 = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == rec11.EnterpriseId);
                if (enter11 == null) enter11 = new JobSeekerEnterprise();

                List<JobSeekerJobPosting> jobpost11 = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == enter11.EnterpriseId).ToList();

                ViewBag.Jobpost = jobpost11;

                PaginatedList<JobSeekerJobPostingApply> paginatedJobs1 = PaginatedList<JobSeekerJobPostingApply>.Create(data, page, pageSize);

                return View(paginatedJobs1);
            }

            List<JobSeekerJobPostingApply> jobapply = new List<JobSeekerJobPostingApply>();

            JobSeekerRecruiterProfile rec = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.User.Id);
            if (rec == null) rec = new JobSeekerRecruiterProfile();

            JobSeekerEnterprise enter = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == rec.EnterpriseId);
            if (enter == null) enter = new JobSeekerEnterprise();

            List<JobSeekerJobPosting> jobpost = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == enter.EnterpriseId).ToList();
            ViewBag.Jobpost = jobpost;

            if (jobpost.Count > 0)
            {
                foreach (JobSeekerJobPosting i in jobpost)
                {
                    List<JobSeekerJobPostingApply> j = db.JobSeekerJobPostingApplies.Where(p => p.JobPostingId == i.Id).ToList();
                    if (!j.Any()) continue;
                    foreach (JobSeekerJobPostingApply ii in j)
                    {
                        jobapply.Add(ii);
                    }
                }
            }
            jobapply = jobapply.OrderByDescending(p => p.ApplyTime).ToList();

            PaginatedList<JobSeekerJobPostingApply> paginatedJobs = PaginatedList<JobSeekerJobPostingApply>.Create(jobapply, page, pageSize);

            return View(paginatedJobs);
        }
        [HttpPost]
        public async Task<IActionResult> CandidateApplyRecruiter(int type, string search, string job, string status, int idedit, string statusedit, int iddelete)
        {
            if (type == 1)
            {
                #region validate
                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(job)) return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                JobSeekerRecruiterProfile rec = db.JobSeekerRecruiterProfiles.FirstOrDefault(p => p.RecruiterId == user.User.Id);
                if (rec == null) rec = new JobSeekerRecruiterProfile();

                JobSeekerEnterprise enter = db.JobSeekerEnterprises.FirstOrDefault(p => p.EnterpriseId == rec.EnterpriseId);
                if (enter == null) enter = new JobSeekerEnterprise();

                List<JobSeekerJobPosting> a = db.JobSeekerJobPostings.Where(p => p.EnterpriseId == enter.EnterpriseId).ToList();

                if (search != null) a = a.Where(p => p.JobTitle.Contains(search)).ToList();

                if (job != "all") a = a.Where(p => p.Id.ToString() == job).ToList();


                List<JobSeekerJobPostingApply> jobapply = new List<JobSeekerJobPostingApply>();

                if (a.Count > 0)
                {
                    foreach (JobSeekerJobPosting i in a)
                    {
                        List<JobSeekerJobPostingApply> j = db.JobSeekerJobPostingApplies.Where(p => p.JobPostingId == i.Id).ToList();
                        if (!j.Any()) continue;
                        foreach (JobSeekerJobPostingApply ii in j)
                        {
                            jobapply.Add(ii);
                        }
                    }
                }

                if (status != "all") jobapply = jobapply.Where(p => p.StatusCode == status).ToList();

                if (jobapply.Any())
                {
                    TempData["candidateapplys"] = JsonConvert.SerializeObject(jobapply, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    return RedirectToAction("CandidateApplyRecruiter", "Home");
                }
                else return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Không tìm thấy!" });
            }
            else if(type == 2)
            {
                #region validate
                if (string.IsNullOrEmpty(statusedit) || idedit == 0) return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = "http://localhost:5281/api/company/updatejobapply";

                JobSeekerJobPostingApply o = db.JobSeekerJobPostingApplies.FirstOrDefault(p => p.Id == idedit);
                if (o == null) return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Có vấn đề khi đổi trạng thái!" });
                
                if(o.StatusCode == statusedit) return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "không có gì thay đổi!" });

                o.StatusCode = statusedit;

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
                    Console.WriteLine("cat nhat trang thai ung vien ung tuyen thanh cong");
                    return RedirectToAction("CandidateApplyRecruiter", "Home", new { success = "Cật nhật trạng thái ứng viên thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("CandidateApplyRecruiter: cat nhat that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
            else
            {
                #region validate
                if (iddelete == 0) return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Vui lòng nhập đầy đủ!" });
                #endregion

                HttpClient client = new HttpClient();
                //Call api
                var apiUrl = $"http://localhost:5281/api/company/deletejobapply/{iddelete}";

                //get with token
                string token = Request.Cookies["jwtToken"];
                Console.WriteLine("jwt: " + token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Gửi yêu cầu DELETE tới API
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("delete ung vien thanh cong");
                    return RedirectToAction("CandidateApplyRecruiter", "Home", new { success = "Xóa ứng viên thành công!" });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) // 401
                {
                    Console.WriteLine("Không được phép: Người dùng chưa xác thực.");
                    return RedirectToAction("Login", "Account", new { error = "Đăng nhập hết hạn!" });
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                {
                    Console.WriteLine("Không được phép: Người dùng không có quyền truy cập.");
                    return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Ghi log lỗi chi tiết từ API
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Delete applyjob: delete that bai: " + response.StatusCode);
                    Console.WriteLine("Chi tiet loi API: " + errorDetails);
                    return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!" });
                }
            }
        }
        public IActionResult CandidateDetailRecruiter(Guid id, int? idjob)
        {
            JobSeekerCandidateProfile can = db.JobSeekerCandidateProfiles.FirstOrDefault(p => p.CandidateId == id);
            if(can == null) return RedirectToAction("CandidateApplyRecruiter", "Home", new { error = "Không tìm thấy thông tin ứng viên!" });

            List<JobSeekerEducationDetail> ledu = db.JobSeekerEducationDetails.Where(p => p.CandidateId == id).ToList();
            List<JobSeekerWorkingExperience> lexp = db.JobSeekerWorkingExperiences.Where(p => p.CandidateId == id).ToList();
            List<JobSeekerCertificate> lcer = db.JobSeekerCertificates.Where(p => p.CandidateId == id).ToList();

            if(idjob != null) NotificationCandidate(idjob);

            ViewBag.Education = ledu;
            ViewBag.Experience = lexp;
            ViewBag.Certificate = lcer;

            return View(can);
        }
        //admin
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

            ViewBag.NotificationAdmin = notificationAd.OrderByDescending(p => p.IsCreatedAt).Where(p => p.Type.Contains("admin")).Take(5).ToList();
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

                if (censor != "all") a = a.Where(p => p.IsCensorship == bool.Parse(censor)).ToList();

                if (a.Any())
                {
                    TempData["Enterprises"] = JsonConvert.SerializeObject(a);
                    return RedirectToAction("ManageCompanyAdmin", "Home");
                }
                else return RedirectToAction("ManageCompanyAdmin", "Home", new { error = "Không tìm thấy các công ty!" });
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

                if (status != "all") a = a.Where(p => p.StatusCode == status).ToList();

                if (a.Any())
                {
                    TempData["JobPostings"] = JsonConvert.SerializeObject(a);
                    return RedirectToAction("ManageJobPostingAdmin", "Home");
                }
                else return RedirectToAction("ManageJobPostingAdmin", "Home", new { error = "Không tìm thấy các bài đăng!" });
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

            if (status != "all") a = a.Where(p => p.StatusCode == status).ToList();

            if (type != "all") a = a.Where(p => p.RoleId == int.Parse(type)).ToList();

            if (a.Any())
            {
                TempData["Accounts"] = JsonConvert.SerializeObject(a);
                return RedirectToAction("ManageAccountAdmin", "Home");
            }
            else return RedirectToAction("ManageAccountAdmin", "Home", new { error = "Không tìm thấy các tài khoản!" });
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
        //Gửi thông báo đến candidate
        public async Task NotificationCandidate(int? id)
        {
            if (id == null) return;
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = $"http://localhost:5281/api/notification/notificationcandidate/{id}";

            //get with token
            string token = Request.Cookies["jwtToken"];
            Console.WriteLine("jwt: " + token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("NotificationCandidate thanh cong");
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
                Console.WriteLine("NotificationCandidate: sen mail that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                Console.WriteLine("Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!");
                return;
            }
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
                return new List<JobSeekerJobField>();
            }
        }
        //Lấy danh sách lĩnh vực
        public async Task<List<City>> GetCity()
        {
            HttpClient client = new HttpClient();
            //Call api
            var apiUrl = "https://raw.githubusercontent.com/kenzouno1/DiaGioiHanhChinhVN/master/data.json";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            List<City> j = new List<City>();

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                //Deserialization từ JSON sang danh sách đối tượng jobfield
                j = JsonConvert.DeserializeObject<List<City>>(responseData);

                Console.WriteLine("get city thanh cong");
                return j;
            }
            else
            {
                // Ghi log lỗi chi tiết từ API
                string errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine(" lay city that bai: " + response.StatusCode);
                Console.WriteLine("Chi tiet loi API: " + errorDetails);
                ViewBag.ErrorMessage = "Có 1 vấn đề nào đó xả ra, vui lòng kết nối tới chúng tôi để được giúp đỡ!";
                return new List<City>();
            }
        }
    }
}
