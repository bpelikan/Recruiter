using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Recruiter.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Recruiter.Services.Implementation;

namespace Recruiter
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>( config => {
                    config.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAutoMapper();

            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            services
                .AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();
            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en-US"),
                        //new CultureInfo("pl-PL"),
                    };

                    opts.DefaultRequestCulture = new RequestCulture("en-US");
                    opts.SupportedCultures = supportedCultures;
                    opts.SupportedUICultures = supportedCultures;
                });

            // Add application services.
            services.AddTransient<IEmailSender, FakeEmailSender>();
            //services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<ICvStorageService, CvStorageServiceWithArchive>();
            services.AddSingleton<IQueueMessageSender, QueueMessageSender>();
            services.AddScoped<IJobPositionRepository, JobPositionRepository>();
            services.AddScoped<IApplicationStageService, ApplicationStageService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IMyApplicationService, MyApplicationService>();
            services.AddScoped<IApplicationsViewHistoriesService, ApplicationsViewHistoriesService>();
            services.AddScoped<IJobPositionService, JobPositionService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                                IHostingEnvironment env, 
                                ILoggerFactory loggerFactory, 
                                UserManager<ApplicationUser> userManager, 
                                RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Warning);

            app.UseStaticFiles();

            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseAuthentication();

            DbIdentityInitializer.SeedData(userManager, roleManager, Configuration);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
