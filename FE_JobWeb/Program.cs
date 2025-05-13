using FE_JobWeb.Others;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký singleton
//builder.Services.AddSingleton<ApplicationUser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Sử dụng Session
app.UseSession();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Tạo FirebaseApp
// Thiết lập biến môi trường GOOGLE_APPLICATION_CREDENTIALS
var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "movieapp-2f052-firebase-adminsdk-uqcg4-93bfec4098.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

// Tạo FirebaseApp với AppOptions
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.GetApplicationDefault()
});
//FirebaseApp.Create(new AppOptions()
//{
//    Credential = GoogleCredential.FromFile("wwwroot/movieapp-2f052-firebase-adminsdk-uqcg4-93bfec4098.json")
//});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
