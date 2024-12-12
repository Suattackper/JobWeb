using Data_JobWeb.Entity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//add db
builder.Services.AddDbContext<JobSeekerContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("JobWebDb")));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
//builde EF database first 

//package manager console lệnh
//Scaffold-DbContext "Data Source=VVV\ANHTAN;Initial Catalog=Quanly;Integrated Security=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entity

//thêm -Force để cật nhật full
//Scaffold-DbContext "Data Source=VVV\ANHTAN;Initial Catalog=Quanly;Integrated Security=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entity -Force

//terminal lệnh
//ccài đặt CLI
//dotnet tool install --global dotnet-ef

//dotnet ef dbcontext scaffold "Data Source=VVV\ANHTAN;Initial Catalog=Quanly;Integrated Security=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer -o Entity
//thêm -Force để cật nhật full
//dotnet ef dbcontext scaffold "Data Source=VVV\ANHTAN;Initial Catalog=Quanly;Integrated Security=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer -o Entity --force