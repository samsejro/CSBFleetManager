//using CSBFleetManager.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSBFleetManager.Persistence;
using CSBFleetManager.Services;
using CSBFleetManager.Services.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using CSBFleetManager.Models;
using CSBFleetManager.Entity;

namespace CSBFleetManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDatabaseDeveloperPageExceptionFilter();//RECENTLY COMMENTED

            //services.AddDefaultIdentity<IdentityUser>(
            //  options => options.SignIn.RequireConfirmedAccount = true)
            //  .AddRoles<IdentityRole>()
            //  .AddEntityFrameworkStores<ApplicationDbContext>();

            //services.AddControllersWithViews();
            //services.AddRazorPages();

            //first and last 3rd row commentend out
            services.AddIdentity<ApplicationUser, IdentityRole>()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultUI()
              .AddClaimsPrincipalFactory<MyUserClaimsPrincipalFactory>()
              .AddDefaultTokenProviders();

            services.AddControllersWithViews();
            services.AddRazorPages();
               //.AddDefaultTokenProviders();


           


            //services.AddTransient<IEmailSender, EmailSender>();

            services.Configure<IdentityOptions>(options =>
            {
                //Default Password Settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                //Default Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";


            });
            services.AddHttpContextAccessor();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IMDAService, MDAService>();
            services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IGetDetailsOnLASRRAIdService, GetDetailsOnLASRRAIdService>();
            services.AddScoped<IGetDetailsOnOracleNumberService, GetDetailsOnOracleNumberService>();
            services.AddScoped<IRegistrationStatistics, RegistrationStatistics>();

            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager,
                                    IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy(); //recently commented

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            //DataSeedingInitializer.UserAndRoleSeedAsync(userManager, roleManager).Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            //app.Use(async (context, next) =>
            //{
            //    await next();
            //    if (context.Response.StatusCode == 404)
            //    {
            //        context.Request.Path = "/Home";
            //        await next();
            //    }
            //});
        }

    }
}
