using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureDevOpsMonitor.BackgroundServices;
using McMaster.AspNetCore.LetsEncrypt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace AzureDevOpsMonitor
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AzureDevOpsConfig>(Configuration.GetSection("AzureDevops"));
            services.Configure<KestrelConfig>(Configuration.GetSection("Kestrel"));
            services.Configure<JWTConfig>(Configuration.GetSection("JWT"));

            services.AddHttpContextAccessor();
            services.AddSingleton<NotificationProcessChannel>();
            services.AddHostedService<NotificationProcessService>();

            var letsEncryptServiceBuilder = services.AddLetsEncrypt();

            if (Configuration.GetSection("LetsEncrypt:PersistToDisk").Value.Equals(Boolean.TrueString))
            {
                letsEncryptServiceBuilder.PersistDataToDirectory(new DirectoryInfo(Configuration.GetSection("LetsEncrypt:PersistDirectory").Value), Configuration.GetSection("LetsEncrypt:PersistPassword").Value);
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var serverSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:ServerSecret"]));
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = serverSecret,
                        ValidIssuer = Configuration["JWT:Issuer"],
                        ValidAudience = Configuration["JWT:Audience"]
                    };
                });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
