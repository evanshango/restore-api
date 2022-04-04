using System;
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
        // services.AddDbContext<StoreContext>(opt => {
        //     // opt.UseNpgsql(_config.GetConnectionString("DefaultConnection"));
        // });

        services.AddDbContext<StoreContext>(opt => {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string connStr;
            if (env == "Development") {
                // Use connection string from file
                connStr = _config.GetConnectionString("DefaultConnection");
            }
            else {
                // Use connection string provided at runtime by heroku
                var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                // Parse connection URL to connection string for NpgSql
                connUrl = connUrl?.Replace("postgres://", string.Empty);
                var pgUserPass = connUrl?.Split("@")[0];
                var pgHostPortDb = connUrl?.Split("@")[1];
                var pgHostPort = pgHostPortDb?.Split("/")[0];
                var pgDb = pgHostPortDb?.Split("/")[1];
                var pgUser = pgUserPass?.Split(":")[0];
                var pgPass = pgUserPass?.Split(":")[1];
                var pgHost = pgHostPort?.Split(":")[0];
                var pgPort = pgHostPort?.Split(":")[1];

                connStr =
                    $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true";
            }
            opt.UseNpgsql(connStr);
        });

        services.AddIdentityCore<User>(opt => { opt.User.RequireUniqueEmail = true; })
            .AddRoles<Role>()
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
        services.AddScoped<PaymentService>();

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

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000"));

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapFallbackToController("Index", "Fallback");
        });
    }
}