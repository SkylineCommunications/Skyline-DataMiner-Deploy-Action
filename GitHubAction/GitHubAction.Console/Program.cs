using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Package.Application;
using Package.Domain.Services;
using Package.Gateway;
using ArtifactDeploymentInfoApi.Generated;
using DeployArtifactApi.Generated;
using GitHubAction.Console.Extensions;
using GitHubAction.Console.Options;
using GitHubAction.Domain.Presenters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Package.Builder;
using Package.Domain.Enums;
using Package.Domain.Models;
using UploadArtifactApi;
using Serilog;
using Serilog.Filters;


namespace GitHubAction.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();
        try
        {
            var host = CreateHostBuilder(args).Build();
            var gitHubAction = host.Services.GetRequiredService<GitHubAction>();
            await gitHubAction.RunAsync(args, new CancellationToken());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddScoped<GitHubAction>();
                services.AddHttpClient();
                services.AddScoped<IPackagePresenter, ConsolePackagePresenter>();
                services.AddSingleton<ApiOptions>(sp =>
                {
                    var environment = Environment.GetEnvironmentVariable("Skyline-deploy-action-namespace");

                    string apiBaseUrl;
                    if (environment != null)
                    {
                        apiBaseUrl = $"https://api-{environment}.dataminer.services/{environment}";
                        Log.Information("Found the \"Skyline-deploy-action-namespace\" environment variable");
                        Log.Information("Setting the base url for the api to: {0}", apiBaseUrl);
                    }
                    else
                    {
                        apiBaseUrl = "https://api.dataminer.services/";
                    }

                    //return
                    return new ApiOptions()
                    {
                        ApiBaseUrl = apiBaseUrl
                    };
                });
                services.AddApis();
                services.AddScoped<IPackageService, PackageService>();
                services.AddScoped<IPackageGateway, HttpPackageGateway>();
                services.AddScoped<IPackageBuilder, PackageBuilder>();
                services.AddScoped<IGithubPresenter, GithubPresenter>();
                services.BuildServiceProvider();
            })
            .UseSerilog((context, services, loggerConfiguration) =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();

                loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Logger(lc => lc
                        .Filter.ByExcluding(Matching.WithProperty<string>("type", type => type == "githubCommand"))
                        .WriteTo.Console(
                            outputTemplate:
                                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}]{Message:lj}[{SourceContext}]{NewLine}{Exception}"))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(Matching.WithProperty<string>("type", type => type == "githubCommand"))
                        .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}"));
            });
}
