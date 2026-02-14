using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RadioCab.Models;
using System;

var builder = WebApplication.CreateBuilder(args);
//Usama Project
// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure form options for larger file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB limit
    options.ValueLengthLimit = 104857600; // 100MB limit
});

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<RadioCabContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//2. // Session Add Karo for Authentication purpose
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Authentication Add Karo
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure request body size limit for file uploads
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    var lengthLimit = 104857600; // 100MB
    if (context.Request.ContentLength > lengthLimit)
    {
        context.Response.StatusCode = 413; // Request Entity Too Large
        await context.Response.WriteAsync("File size too large. Maximum allowed size is 100MB.");
        return;
    }
    await next();
});

app.UseRouting();

app.UseSession();          // REQUIRED

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "userVacancies",
    pattern: "User/Vacancies",
    defaults: new { controller = "User", action = "Vacancies" });

app.MapControllerRoute(
    name: "userVacancyDetails",
    pattern: "User/VacancyDetails/{id}",
    defaults: new { controller = "User", action = "VacancyDetails" });

app.MapControllerRoute(
    name: "userApplyForVacancy",
    pattern: "User/ApplyForVacancy",
    defaults: new { controller = "User", action = "ApplyForVacancy" });
app.Run();
