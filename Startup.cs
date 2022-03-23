using API.Data;
using API.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        services.AddControllers();
        services.AddSwaggerGen(c => {
            c.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}");
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Skinet API", Version = "v1" });
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

        app.UseCors(opt => opt.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins("http://localhost:3000")
        );

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}