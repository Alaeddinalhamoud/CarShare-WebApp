using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Jobs;
using CarShareV1.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Stripe;
using System;
using System.Collections.Specialized;

namespace CarShareV1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _quartzScheduler = ConfigureQuartz();
        }
        private IScheduler _quartzScheduler;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<UserApplication, IdentityRole>(config => {
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultUI(UIFramework.Bootstrap4)
                  .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //Facebook
            services.AddAuthentication()
            //    .AddFacebook(facebookOptions =>
            //{
            //    facebookOptions.AppId = "616448745474826";
            //    facebookOptions.AppSecret = "4b94750cb10d928035476ffa54f98d06";
            //})
              .AddGoogle(googleOptions => {
                  googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                  googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];

              })
              .AddFacebook(facebookOptions=>
              {
                  facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                  facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
              });

           
            services.AddSingleton<IdentityRole>();
            // requires
            //the email token life time
            services.Configure<DataProtectionTokenProviderOptions>(o =>
               o.TokenLifespan = TimeSpan.FromHours(3));
            services.AddTransient<IEmailSender, EmailSender>();          
            services.AddTransient<ISMSProvider, SMSProvider>();
            services.AddTransient<GooglereCaptchaService>();
            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("SendGrid"));
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
            services.Configure<ReCAPTCHASettings>(Configuration.GetSection("GooglereCAPTCHA"));
            // Add scheduled tasks & scheduler
            services.AddTransient<IJobFactory, ServiceJobFactory>((provider) => new ServiceJobFactory(services.BuildServiceProvider()));

            services.AddTransient<JobDeleteUnpaid>();
            services.AddSingleton(provider => _quartzScheduler);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            StripeConfiguration.SetApiKey(Configuration.GetSection("Stripe")["SecretKey"]);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            _quartzScheduler.JobFactory = new ServiceJobFactory(app.ApplicationServices);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        public IScheduler ConfigureQuartz()
        {
            NameValueCollection props = new NameValueCollection()
            {
                {"quarrtz.serializer.type","binary" },
            };
            StdSchedulerFactory facotry = new StdSchedulerFactory(props);
            var scheduler = facotry.GetScheduler().Result;
            scheduler.Start().Wait();
            return scheduler;
        }

        private void OnShutdown()
        {
            if (!_quartzScheduler.IsShutdown) _quartzScheduler.Shutdown();
        }
    }
}
