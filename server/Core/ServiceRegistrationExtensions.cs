using Microsoft.SymbolStore;

using WinDbgSymbolsCachingProxy.Core.Auth;
using WinDbgSymbolsCachingProxy.Jobs;
using WinDbgSymbolsCachingProxy.Logging;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Dependency injection registrations for the application's own services.
/// </summary>
internal static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddSymbolProxyServices(this IServiceCollection services, LogBufferService logBuffer)
    {
        services.AddSingleton<IBadgeFactory, BadgeFactory>();
        services.AddSingleton<IBadgeService, BadgeService>();
        services.AddSingleton<ISvgService, SvgService>();
        services.AddTransient<RecheckNotFoundJob>();
        services.AddSingleton<SymbolAliasLookupService>();
        services.AddSingleton<RecheckNotFoundService>();
        services.AddTransient<SymStoreService>();
        services.AddTransient<ITracer, Tracer>();
        services.AddSingleton<SymbolParsingService>();
        services.AddSingleton<ICachedSymbolOverviewProvider, CachedSymbolOverviewProvider>();
        services.AddSingleton<IStatusOpenGraphImageRenderer, StatusOpenGraphImageRenderer>();
        services.AddSingleton(logBuffer);

        // Auth-related services
        services.AddSingleton<RoleService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<ApiKeyService>();

        return services;
    }
}