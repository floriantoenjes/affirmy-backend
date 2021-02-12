using System;
using System.Text;
using AffirmyBackend.Areas.Identity.Data;
using AffirmyBackend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

[assembly: HostingStartup(typeof(AffirmyBackend.Areas.Identity.IdentityHostingStartup))]
namespace AffirmyBackend.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        
        public IConfiguration Configuration { get; }

        public IdentityHostingStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<AffirmyBackendContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("AffirmyBackendContextConnection")));

                services.AddDefaultIdentity<AffirmyBackendUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<AffirmyBackendContext>()
                    .AddDefaultTokenProviders();

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = Configuration["JWT:ValidAudience"],
                        ValidIssuer = Configuration["JWT:ValidIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                    };
                });

            });
        }
    }
}