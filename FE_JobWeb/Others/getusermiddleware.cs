using Azure.Core;
using Data_JobWeb.Entity;
using Newtonsoft.Json;

namespace FE_JobWeb.Others
{
    public class getusermiddleware
    {
        private readonly RequestDelegate _next;

        public getusermiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Gán giá trị cho HttpContext trước khi controller xử lý request
            string userJson = context.Request.Cookies["user"];
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<JobSeekerUserLoginDatum>(userJson);
                context.Items["User"] = user; // Gán vào HttpContext.Items
            }

            // Tiếp tục xử lý request
            await _next(context);
        }
    }
}
