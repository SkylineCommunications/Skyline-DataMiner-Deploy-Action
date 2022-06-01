using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Package.Application;
using Package.Domain.Services;
using Package.Gateway;
using ArtifactDeploymentInfoApi.Generated;
using DeployArtifactApi.Generated;
using GitHubAction.Console.Extensions;
using GitHubAction.Console.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Package.Builder;
using Package.Domain.Enums;
using Package.Domain.Models;
using UploadArtifactApi;
using Serilog;


namespace GitHubAction.Console;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        try
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
            throw;
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
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                });
                services.BuildServiceProvider();
            })
            .UseSerilog((context, services, loggerConfiguration) =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();

                loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}]{Message:lj}[{SourceContext}]{NewLine}{Exception}"
                    );
            });
}
