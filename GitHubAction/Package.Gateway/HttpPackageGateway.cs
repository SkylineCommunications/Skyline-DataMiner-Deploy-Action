using System.Net;
using ArtifactDeploymentInfoApi.Generated;
using ArtifactDeploymentInfoApi.Generated.Models;
using DeployArtifactApi.Generated;
using DeployArtifactApi.Generated.Models;
using Microsoft.Rest;
using Package.Domain.Exceptions;
using Package.Domain.Models;
using Package.Domain.Services;
using UploadArtifactApi;

namespace Package.Gateway;

public class HttpPackageGateway : IPackageGateway
{
    private readonly IArtifactUploadApi _artifactUploadApi;
    private readonly IPackagePresenter _packagePresenter;
    private readonly IArtifactDeploymentInfoAPI _artifactDeploymentInfoApi;
    private readonly IDeployArtifactAPI _deployArtifactApi;
    private const string DeploymentInfoKey = "DeploymentInfo";

    public HttpPackageGateway(IPackagePresenter packagePresenter, IArtifactUploadApi artifactUploadApi, IArtifactDeploymentInfoAPI artifactDeploymentInfoApi, IDeployArtifactAPI deployArtifactApi)
    {
        _packagePresenter = packagePresenter;
        _artifactUploadApi = artifactUploadApi;
        _artifactDeploymentInfoApi = artifactDeploymentInfoApi;
        _deployArtifactApi = deployArtifactApi;
    }

    public async Task<UploadedPackage> UploadPackageAsync(CreatedPackage createdPackage, string catalogVersion, string key)
    {
        try
        {
            _packagePresenter.LogInformation("Calling Artifact Upload");
            var res = await _artifactUploadApi.ArtifactUploadV11PrivateArtifactPostAsync(
                createdPackage.Package,
                createdPackage.Name,
                catalogVersion,
                createdPackage.Type,
                key,
                default, _packagePresenter);

            if (res.ArtifactId == null) throw new UploadPackageException("Received an invalid upload response");

            return new UploadedPackage(res.ArtifactId);
        }
        catch (KeyException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new UploadPackageException($"Couldn't upload the package. Error message: {e}");
        }
    }

    public async Task<DeployingPackage> DeployPackageAsync(UploadedPackage uploadedPackage, string key)
    {
        HttpOperationResponse<DeploymentModel> res;
        try
        {
            res = await _deployArtifactApi.DeployArtifactWithApiKeyFunctionWithHttpMessagesAsync(new DeployArtifactAsSystemForm(uploadedPackage.ArtifactId), key);
        }
        catch (Exception e)
        {
            throw new DeployPackageException($"Couldn't deploy the package {e}", e);
        }

        if (res.Response.IsSuccessStatusCode)
        {
            if (Guid.TryParse(res.Body.DeploymentId, out var deploymentId))
            {
                return new DeployingPackage(uploadedPackage.ArtifactId, deploymentId);
            }
            throw new DeployPackageException("Received an invalid deployment ID");
        }

        if (res.Response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            throw new KeyException($"The deploy API returned a response with status code {res.Response.StatusCode}");
        }

        var responseContent = string.Empty;
        if (res.Response.Content != null)
        {
            responseContent = await res.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        throw new DeployPackageException($"The deploy API returned a response with status code {res.Response.StatusCode}, content: {responseContent}");
    }

    public async Task<DeployedPackage> GetDeployedPackageAsync(DeployingPackage deployingPackage, string key)
    {
        HttpOperationResponse<IDictionary<string, DeploymentInfoModel>> res;
        try
        {
            res = await _artifactDeploymentInfoApi.GetPrivateArtifactDeploymentInfoWithHttpMessagesAsync(deployingPackage.DeploymentId, key);
        }
        catch (Exception e)
        {
            throw new GetDeploymentPackageException($"Couldn't get the deployed package {e.ToString()}", e);
        }

        if (res.Response.IsSuccessStatusCode)
        {
            if (res.Body.TryGetValue(DeploymentInfoKey, out var deploymentInfoModel))
            {
                return new DeployedPackage(deploymentInfoModel.ArtifactId, deployingPackage.DeploymentId, deploymentInfoModel.CurrentState);
            }
            throw new GetDeploymentPackageException("Received an invalid deployment info response");
        }
        
        if (res.Response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            throw new KeyException($"The GetDeployedPackage API returned a response with status code {res.Response.StatusCode}");
        }

        var responseContent = string.Empty;
        if (res.Response.Content != null)
        {
            responseContent = await res.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        throw new GetDeploymentPackageException($"The GetDeployedPackage API returned a response with status code {res.Response.StatusCode}, content: {responseContent}");
    }
}
