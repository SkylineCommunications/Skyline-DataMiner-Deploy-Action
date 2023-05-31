using ArtifactDeploymentInfoApi.Generated;
using DeployArtifactApi.Generated;
using GitHubAction.Console.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using UploadArtifactApi;

namespace GitHubAction.Console.Extensions;

public static class ApiExtensions
{
    public static void AddApis(this IServiceCollection services)
    {
        services.AddScoped<IArtifactDeploymentInfoAPI>(s =>
        {
            var options = s.GetService<ApiOptions>();
            return new ArtifactDeploymentInfoAPI(
                new Uri($"{options.ApiBaseUrl}/api"),
                new BasicAuthenticationCredentials());
        });

        services.AddScoped<IDeployArtifactAPI>(s =>
        {
            var options = s.GetService<ApiOptions>();
            return new DeployArtifactAPI(
                new Uri($"{options.ApiBaseUrl}/api"),
                new BasicAuthenticationCredentials());
        });

        services.AddScoped<IArtifactUploadApi>(s =>
        {
            var httpClient = s.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(HttpArtifactUploadApi));
            var options = s.GetService<ApiOptions>();
            var presenter = s.GetService<ConsolePackagePresenter>();
            httpClient.BaseAddress = new Uri($"{options.ApiBaseUrl}/");
            return new HttpArtifactUploadApi(httpClient);
        });
    }
}
