using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Household;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

/// <summary>Registers application services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the application layer.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITodayProvider, TodayProvider>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IItemMappingService, ItemMappingService>();
        services.AddScoped<IBillScanService, BillScanService>();
        services.AddScoped<IBillFilingService, BillFilingService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<PlanningService>();
        services.AddScoped<PriceHistoryService>();
        services.AddScoped<StatisticsService>();
        services.AddScoped<WhatsAppBillService>();
        services.AddScoped<WhatsAppConfigService>();
        services.AddScoped<WhatsAppSettingsService>();
        services.AddSingleton<WhatsAppActivity>();
        services.AddScoped<PantryService>();
        services.AddScoped<AuthService>();
        services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        return services;
    }
}
