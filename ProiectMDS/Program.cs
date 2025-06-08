using Microsoft.EntityFrameworkCore;
using ProiectMDS.Data;
using Microsoft.AspNetCore.Identity;
using ProiectMDS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using ProiectMDS.Services;


var builder = WebApplication.CreateBuilder(args);

// Adăugăm DbContext înainte de Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurăm serviciile Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Email configurare 
builder.Services.AddTransient<ProiectMDS.Services.IEmailSender, EmailService>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailService>();


builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Adăugăm celelalte servicii
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
});

var app = builder.Build();

// Configurarea pipeline-ului HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();//de verificat
app.UseAuthentication();  // Adăugăm autentificare
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "test",
    pattern: "test/{action=Index}/{id?}",
    defaults: new { controller = "Test" });


app.MapRazorPages();

app.Run();
