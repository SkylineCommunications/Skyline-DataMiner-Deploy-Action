using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Package.Application;
using Package.Domain.Services;
using Package.Gateway;
using ArtifactDeploymentInfoApi.Generated;
using DeployArtifactApi.Generated;
using GitHubAction.Console;
using GitHubAction.ConsolePackagePresenter;
using Microsoft.Extensions.Logging;
using Package.Builder;
using Package.Domain.Enums;
using Package.Domain.Models;
using UploadArtifactApi;
using Serilog;

GitHubAction.GitHubAction gitHubAction;
ILogger<Program> logger;
try
{
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    // Get Environment
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


    // Setup DI
    var serviceProvider = new ServiceCollection()
        .AddScoped<GitHubAction.GitHubAction>()
        .AddHttpClient()
        .AddScoped<IPackagePresenter, ConsolePackagePresenter>()
        .AddScoped<IArtifactUploadApi>(s =>
        {
            var httpClient = s.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(HttpArtifactUploadApi));
            httpClient.BaseAddress = new Uri($"{apiBaseUrl}/");
            return new HttpArtifactUploadApi(httpClient);
        })
        .AddScoped<IArtifactDeploymentInfoAPI>(s =>
            new ArtifactDeploymentInfoAPI(
                new Uri($"{apiBaseUrl}/api"),
                new BasicAuthenticationCredentials()))
        .AddScoped<IDeployArtifactAPI>(s =>
            new DeployArtifactAPI(
                new Uri($"{apiBaseUrl}/api"),
                new BasicAuthenticationCredentials()))
        .AddScoped<IPackageService, PackageService>()
        .AddScoped<IPackageGateway, HttpPackageGateway>()
        .AddScoped<IPackageBuilder, PackageBuilder>()
        .AddLogging(builder => {
            builder.ClearProviders();
            builder.AddConsole();
        })
        .BuildServiceProvider();

    gitHubAction = serviceProvider.GetRequiredService<GitHubAction.GitHubAction>() ?? throw new ArgumentException(nameof(GitHubAction.GitHubAction));
    logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>() ?? throw new ArgumentException(nameof(ILogger<Program>));
}
catch (Exception e)
{
    Console.WriteLine("Something went wrong while starting. " + e);
    Environment.Exit(500); // Internal Server Error
    return;
}

try
{
    // Parse & validate inputs
    var inputs = ParseInputs.ParseAndValidateInputs(args, logger);
    if (inputs == null)
    {
        logger.LogError("There was a problem with the provided arguments...");
        Environment.Exit(400); // Bad Request
        return;
    }

    // Do the work
    var packageName =inputs[Input.PackageName];
    var version = inputs[Input.Version];
    var solutionFile = new FileInfo(inputs[Input.SolutionPath]);
    var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
    

    var localPackageConfig = new LocalPackageConfig(
        solutionFile,
        workingDirectory,
        packageName,
        version,
        SolutionType.DmScript);

    await gitHubAction.RunAsync(
        inputs[Input.ApiKey], 
        localPackageConfig,
        TimeSpan.FromSeconds(3), 
        TimeSpan.FromMinutes(2),
        TimeSpan.Parse(inputs[Input.Timeout]));
}
catch (Exception e)
{
    logger.LogError("Something went wrong. {exception}", e.ToString());
    Environment.Exit(500); // Internal Server Error
    return;
}
