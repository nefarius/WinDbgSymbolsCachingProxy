using Microsoft.Extensions.DependencyInjection;

namespace WinDbgSymbolsCachingProxy.UpdateCheck;

public static class LatestMsiVersionCheckServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a process-wide latest-MSI check. The host must already have registered
    ///     <see cref="ILocalAssemblyVersionSource"/>.
    /// </summary>
    public static IServiceCollection AddLatestMsiVersionCheck(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IInstalledMsiVersionSource, WindowsInstalledMsiVersionSource>();
        services.AddHttpClient(LatestMsiVersionChecker.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton(static serviceProvider =>
        {
            IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return new LatestMsiVersionChecker(
                factory.CreateClient(LatestMsiVersionChecker.HttpClientName),
                serviceProvider.GetRequiredService<IInstalledMsiVersionSource>(),
                serviceProvider.GetRequiredService<ILocalAssemblyVersionSource>());
        });
        return services;
    }
}
