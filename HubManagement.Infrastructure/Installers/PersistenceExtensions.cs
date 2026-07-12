using HubManagement.BuildingBlock.Core.DbSettings;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.DataContext;
using HubManagement.Infrastructure.Inteceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HubManagement.Infrastructure.Installers;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<PostGreSqlSetting>().Bind(configuration.GetSection(nameof(PostGreSqlSetting)))
            .ValidateDataAnnotations().Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), 
                "PostGreSqlSetting.ConnectionString is required.")
            .ValidateOnStart();
        
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DomainEventsInterceptor>();
        
        services.AddDbContext<HubDbContext>((sp, options) =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var dbConfig = sp.GetRequiredService<IOptions<PostGreSqlSetting>>().Value;
            options.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(HubDbContext).Assembly.FullName);

                if (dbConfig.commandTimeout.HasValue)
                {
                    npgsqlOptions.CommandTimeout(dbConfig.commandTimeout.Value);
                }

                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            if (env.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
        });
        
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
                
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            }).AddEntityFrameworkStores<HubDbContext>()
            .AddDefaultTokenProviders();
        
        return services;
    }
}