using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static RFPPortalWebsite.Models.Constants.Enums;
using static RFPPortalWebsite.Program;

namespace RFPPortalWebsite
{
    public class Startup
    {
        public static System.Timers.Timer rfpStatusTimer;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            LoadConfig(configuration);
            InitializeService();
        }

        public static void LoadConfig(IConfiguration configuration)
        {
            var config = configuration.GetSection("PlatformSettings");
            config.Bind(_settings);
        }

        public static void InitializeService()
        {
            monitizer = new Monitizer();

            ApplicationStartResult mysqlMigrationcontrol = mysql.Migrate(new rfpdb_context().Database);
            if (!mysqlMigrationcontrol.Success)
            {
                monitizer.startSuccesful = -1;
                monitizer.AddException(mysqlMigrationcontrol.Exception, LogTypes.ApplicationError, true);
            }

            ApplicationStartResult mysqlcontrol = mysql.Connect(_settings.DbConnectionString);
            if (!mysqlcontrol.Success)
            {
                monitizer.startSuccesful = -1;
                monitizer.AddException(mysqlcontrol.Exception, LogTypes.ApplicationError, true);
            }

            if (monitizer.startSuccesful != -1)
            {
                monitizer.startSuccesful = 1;
                monitizer.AddApplicationLog(LogTypes.ApplicationLog, monitizer.appName + " application started successfully.");
            }

            rfpStatusTimer = new System.Timers.Timer(10000);
            rfpStatusTimer.Elapsed += CheckRfpStatus;
            rfpStatusTimer.AutoReset = true;
            rfpStatusTimer.Enabled = true;
        }

        private static void CheckRfpStatus(Object source, ElapsedEventArgs e)
        {
            using (rfpdb_context db = new rfpdb_context())
            {
                //Check if Rfp internal bidding ended and public bidding started
                var dt = DateTime.Now.AddDays(-Program._settings.InternalBiddingDays);
                var publicRfps = db.Rfps.Where(x => x.Status == Models.Constants.Enums.RfpStatusTypes.Internal.ToString() && x.CreateDate < dt && x.WinnerRfpBidID == null).ToList();

                foreach (var rfp in publicRfps)
                {
                    rfp.Status = Models.Constants.Enums.RfpStatusTypes.Public.ToString();
                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                }

                //Check if rfp public bidding ended without any winner
                var dt2 = DateTime.Now.AddDays(-(Program._settings.PublicBiddingDays + Program._settings.InternalBiddingDays));
                var expiredRfps = db.Rfps.Where(x=> x.Status == Models.Constants.Enums.RfpStatusTypes.Public.ToString() && x.CreateDate < dt2 && x.WinnerRfpBidID == null).ToList();

                foreach (var rfp in expiredRfps)
                {
                    rfp.Status = Models.Constants.Enums.RfpStatusTypes.Expired.ToString();
                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(6);
                //options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddControllersWithViews();
            services.AddMvc().AddControllersAsServices();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
