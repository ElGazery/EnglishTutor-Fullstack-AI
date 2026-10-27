using EnglishTutor.Application.Common.Settings;
using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Infrastructure.Persistence;
using EnglishTutor.Infrastructure.Repositories;
using EnglishTutor.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // --- Database ---
            services.AddDbContext<TutorDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // --- Identity ---
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<TutorDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.User.RequireUniqueEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            });

            // --- Settings ---
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));

            // --- Services ---
            services.AddScoped<ITokenService, TokenService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // --- Gemini HTTP client ---
            services.AddHttpClient<IAiTutorService, GeminiAiTutorService>();

            // --- Repository / UoW ---
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
