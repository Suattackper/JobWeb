using Data_JobWeb.Entity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//add db
builder.Services.AddDbContext<JobSeekerContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("JobWebDb")));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
