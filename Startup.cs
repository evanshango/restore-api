using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Middlewares;
using API.RequestHelpers;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API;

public class Startup {
    private readonly IConfiguration _config;

    public Startup(IConfiguration configuration) {
        _config = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
        services.AddDbContext<StoreContext>(opt => {
            opt.UseSqlite(_config.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentityCore<User>(opt => { opt.User.RequireUniqueEmail = true; }).AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<StoreContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt => {
                opt.Events = new JwtBearerEvents {
                    OnChallenge = context => {
                        context.Response.OnStarting(async () => {
                            // Write to the response in any way you wish
                            await context.Response.WriteAsJsonAsync(new ProblemDetails {
                                Title = "Unauthorized request",
                                Status = 401,
                                Detail = "Please check your credentials and try again"
                            });
                        });
                        return Task.CompletedTask;
                    }
                };
                opt.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSettings:TokenKey"])),
                    ValidIssuer = _config["JWTSettings:Issuer"]
                };
            });
        services.AddAuthorization();


        services.AddScoped<TokenService>();

        services.AddControllers();

        services.AddSwaggerGen(c => {
            c.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}");
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Skinet API", Version = "v1" });
            c.OperationFilter<AuthorizeOperationFilter>();
            var securityScheme = new OpenApiSecurityScheme {
                Description = "JWT Auth Bearer Scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference {
                    Id = "bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        });
        services.AddCors();
        services.AddRouting(opt => {
            opt.LowercaseUrls = true;
            opt.LowercaseQueryStrings = true;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        app.UseMiddleware<ExceptionMiddleware>();
        if (env.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skinet API v1"));
        }

        // app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000"));

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}