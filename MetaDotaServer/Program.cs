using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MetaDotaServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MetaDotaServer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);


            #region ע��JWT����

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            ).AddJwtBearer(options =>
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
                    ValidateIssuer = true, //�Ƿ���֤Issuer
                    ValidIssuer = builder.Configuration["Jwt:Issuer"], //������Issuer
                    ValidateAudience = true, //�Ƿ���֤Audience
                    ValidAudience = builder.Configuration["Jwt:Audience"], //������Audience
                    ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])), //SecurityKey
                    ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                    ClockSkew = TimeSpan.FromSeconds(0), //����ʱ���ݴ�ֵ�������������ʱ�䲻ͬ�����⣨�룩
                    RequireExpirationTime = true,
                };
            }
            );
            #endregion

            builder.Services.AddSingleton<MetaDotaServer.Tool.DbContextFactory>();

 
            // Add services to the container.
            builder.Services.AddControllers();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

  
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();
            
            app.Run();
        }
    }
}
