using Package.Domain.Exceptions;
using System.Net;
using Newtonsoft.Json;
using Package.Domain.Services;
using System.Text;

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
        string version,
        string contentType,
        string key,
        CancellationToken cancellationToken, IPackagePresenter presenter)
    {
        string dmappFilePath = Path.Combine(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE"), name);
        File.WriteAllBytes(dmappFilePath, package);
        FileStream fileStream = new FileStream(dmappFilePath, FileMode.Open, FileAccess.Read, FileShare.None);

        StringBuilder sb = new StringBuilder();

        using var formData = new MultipartFormDataContent();
        formData.Headers.Add("Ocp-Apim-Subscription-Key", key);
        formData.Add(new StringContent(name), "name");
        formData.Add(new StringContent(version), "version");
        formData.Add(new StringContent(contentType), "contentType");
        formData.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileStream.Name));

        string logInfo = $"--name {name} --version {version} --contentType {contentType} --file {Path.GetFileName(fileStream.Name)}";
        presenter.LogInformation("HTTP Post with info: " + logInfo);

        var response = await _httpClient.PostAsync(UploadPath, formData, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<PrivateArtifactModel>(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            throw new KeyException($"The upload api returned a {response.StatusCode} response. Body:" + response?.Content?.ToString() ?? "null");
        }

        throw new UploadPackageException($"The upload api returned a {response.StatusCode} response. Body:" + response?.Content?.ToString() ?? "null");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
