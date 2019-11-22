using EFCore.Scaffolding.Extension;
using Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.SelfHost.Common;
using Serilog;
using Serilog.Events;

namespace Quartz.SelfHost
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

            services.AddControllers();
            services.AddSingleton(SchedulerCenter.Instance);
            services.AddEntityFrameworkSqlServer().AddDbContext<QuartzDbContext>(options =>
            {
                options.UseSqlServer(Connection.ConnectionString, b => b.UseRowNumberForPaging());
            });

            AuthenticationConfiguration.AddService(services, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SchedulerCenter.Instance.StartScheduleAsync().Wait();
        }

    }
}
