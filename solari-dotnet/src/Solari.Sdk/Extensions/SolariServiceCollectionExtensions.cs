using Microsoft.Extensions.DependencyInjection;

namespace Solari.Sdk.Extensions;

/// <summary>
/// ASP.NET Core / generic-host DI registration for <see cref="SolariClient"/>.
/// </summary>
public static class SolariServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SolariClient"/> backed by an <c>IHttpClientFactory</c>-managed
    /// <see cref="HttpClient"/>, so connections are pooled correctly instead of each
    /// request opening a new socket.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddSolari(options =>
    /// {
    ///     options.ApiKey = builder.Configuration["Solari:ApiKey"]!;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSolari(this IServiceCollection services, Action<SolariOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SolariOptions();
        configure(options);

        services.AddHttpClient<SolariClient>(client =>
        {
            client.BaseAddress = options.BaseAddress;
            client.Timeout = options.RequestTimeout;
        }).AddTypedClient((httpClient, _) => new SolariClient(httpClient, options));

        return services;
    }
}
