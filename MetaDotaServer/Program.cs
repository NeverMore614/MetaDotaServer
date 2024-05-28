using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MetaDotaServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MetaDotaServer.Tool;

namespace MetaDotaServer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<MetaDotaServer.Data.TokenContext>(options =>
    
    options.UseSqlServer(builder.Configuration.GetConnectionString("TokenContext") ?? throw new InvalidOperationException("Connection string 'ClassContext' not found.")));
    
            builder.Services.AddDbContext<UserContext>(options =>
    
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Connection string 'ClassContext' not found.")));

            #region ע��JWT����

            //客户端验证
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer("Bearer", options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(10),
                    RequireExpirationTime = true,
                };
            }
            ).AddJwtBearer("BearerReplayBuilder", options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt2:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt2:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt2:SecretKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(10),
                    RequireExpirationTime = true,
                };
            }
            ).AddJwtBearer("BearerLogin", options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt3:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt3:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt3:SecretKey"])),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                };
            }
            );

            #endregion

            builder.Services.AddSingleton<MetaDotaServer.Tool.MDSDbContextFactory>();
            builder.Services.AddSingleton<MDSEmailSender>();
 
            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy
                    (name: "myCors",
                        builde =>
                        {
                            builde.WithOrigins("http://localhost:5069");
                        }
                    );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("myCors");


            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();
            
            app.Run();
        }
    }
}
