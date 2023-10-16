using System.Net;

using Catalog.Domain;

using Newtonsoft.Json;

using Package.Domain.Exceptions;
using Package.Domain.Services;

namespace UploadArtifactApi;

public class HttpArtifactUploadApi : IArtifactUploadApi, IDisposable
{
    private readonly HttpClient _httpClient;
    private const string UploadPath = "api/key-artifact-upload/v1-0/private/artifact";

    public HttpArtifactUploadApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PrivateArtifactModel> ArtifactUploadV11PrivateArtifactPostAsync(
        byte[] package,
        string name,
        string key,
        CatalogData catalog,
        CancellationToken cancellationToken, IPackagePresenter presenter)
    {
        using var formData = new MultipartFormDataContent();
        formData.Headers.Add("Ocp-Apim-Subscription-Key", key);
        formData.Add(new StringContent(name), "name");
        formData.Add(new StringContent(catalog.Version), "version");
        formData.Add(new StringContent(catalog.ContentType), "contentType");
        formData.Add(new StringContent(catalog.Branch), "branch");
        formData.Add(new StringContent(catalog.Identifier), "identifier");
        formData.Add(new StringContent(catalog.IsPreRelease ? "true" : "false"), "isprerelease");
        formData.Add(new StringContent(catalog.CommitterMail), "developer");
        formData.Add(new StringContent(catalog.ReleaseUri), "releasepath");

        MemoryStream ms = new MemoryStream();
        ms.Write(package, 0, package.Length);

        // Reset position so it can be read out again.
        ms.Position = 0;
        formData.Add(new StreamContent(ms), "file", name);

        string logInfo = $"--name {name} --version {catalog.Version} --contentType {catalog.ContentType} --branch {catalog.Branch} --identifier {catalog.Identifier} --isprerelease {catalog.IsPreRelease} --developer {catalog.CommitterMail} --releasepath {catalog.ReleaseUri} --file {name}";
        presenter.LogInformation("HTTP Post with info: " + logInfo);

        var response = await _httpClient.PostAsync(UploadPath, formData, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<PrivateArtifactModel>(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            throw new KeyException($"The upload api returned a {response.StatusCode} response. Body: {response.Content}");
        }

        throw new UploadPackageException($"The upload api returned a {response.StatusCode} response. Body: {response.Content}");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
