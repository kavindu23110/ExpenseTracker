using Anthropic;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.Persistence;
using ExpenseTracker.Infrastructure.Reading;
using ExpenseTracker.Infrastructure.Seed;
using ExpenseTracker.Infrastructure.WhatsApp;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure;

/// <summary>Registers persistence, seeding and the bill reader.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the infrastructure layer.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(config.GetConnectionString("Default"), sql => sql.EnableRetryOnFailure(6, TimeSpan.FromSeconds(30), null)));
        // Containers restart with empty disks, so sign-in keys live in the database.
        services.AddDataProtection().SetApplicationName("ExpenseTracker").PersistKeysToDbContext<AppDbContext>();
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped(typeof(IStore<>), typeof(EfStore<>));
        services.AddScoped<IItemMappingRepository, ItemMappingRepository>();
        services.AddScoped<IPriceWatchRepository, PriceWatchRepository>();
        services.AddScoped<DatabaseInitialiser>();
        services.Configure<WhatsAppOptions>(config.GetSection(WhatsAppOptions.Section));
        services.AddSingleton<ISecretProtector, SecretProtector>();
        services.AddScoped<IWhatsAppClient, WhatsAppClient>();
        services.Configure<ClaudeOptions>(config.GetSection(ClaudeOptions.Section));

        var claude = config.GetSection(ClaudeOptions.Section).Get<ClaudeOptions>() ?? new ClaudeOptions();
        var apiKey = claude.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) apiKey = config["ANTHROPIC_API_KEY"];
        var cliPath = claude.UseCli ? ClaudeCliBillReaderAdapter.FindCli(claude.CliPath) : null;
        var useApi = !claude.UseCli && !string.IsNullOrWhiteSpace(apiKey);
        services.AddSingleton<IBillReaderStatus>(new BillReaderStatus(useApi || cliPath is not null));
        if (cliPath is not null)
        {
            services.AddSingleton<IBillReader>(sp => new ClaudeCliBillReaderAdapter(
                cliPath,
                sp.GetRequiredService<IOptions<ClaudeOptions>>(),
                sp.GetRequiredService<ILogger<ClaudeCliBillReaderAdapter>>()));
        }
        else if (useApi)
        {
            services.AddSingleton(new AnthropicClient { ApiKey = apiKey! });
            services.AddSingleton<IBillReader, ClaudeBillReaderAdapter>();
        }
        else
        {
            services.AddSingleton<IBillReader, UnconfiguredBillReader>();
        }

        return services;
    }
}
