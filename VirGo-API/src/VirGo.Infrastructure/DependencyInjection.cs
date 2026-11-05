using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VirGo.Application.Abstractions;
using VirGo.Application.Configuration;
using VirGo.Infrastructure.Persistence.MongoDb;
using VirGo.Infrastructure.Auth;
using VirGo.Application.Auth;
using VirGo.Application.Marketplace;

namespace VirGo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<PersistenceOptions>()
            .Bind(configuration.GetSection("Persistence"))
            .Validate(
                options => string.Equals(options.Provider, "InMemory", StringComparison.OrdinalIgnoreCase)
                    || (string.Equals(options.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(options.ConnectionString)),
                "Persistence uses InMemory or MongoDb with a connection string.")
            .ValidateOnStart();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<QuoteHandler>();
        services.AddScoped<BookingHandler>();

        if (!string.Equals(configuration["Persistence:Provider"], "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<ISessionRepository, InMemorySessionRepository>();
            services.AddSingleton<IVenueRepository, InMemoryVenueRepository>();
            services.AddSingleton<IQuoteRepository, InMemoryQuoteRepository>();
            services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
        }
        else
        {
            services.AddSingleton<IUserRepository, MongoUserRepository>();
            services.AddSingleton<ISessionRepository, MongoSessionRepository>();
            services.AddSingleton<IVenueRepository, MongoVenueRepository>();
            services.AddSingleton<IQuoteRepository, MongoQuoteRepository>();
            services.AddSingleton<IBookingRepository, MongoBookingRepository>();
        }

        if (string.Equals(configuration["Persistence:Provider"], "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PersistenceOptions>>().Value;
                var settings = MongoClientSettings.FromConnectionString(options.ConnectionString!);
                settings.ApplicationName = "VirGo-Api";
                settings.MaxConnectionPoolSize = 10;
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
                return new MongoClient(settings);
            });

            services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PersistenceOptions>>().Value;
                return serviceProvider.GetRequiredService<IMongoClient>().GetDatabase(options.DatabaseName);
            });
            services.AddSingleton<IMongoRepositoryFactory, MongoRepositoryFactory>();
        }

        return services;
    }

    private sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
