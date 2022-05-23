using Package.Domain.Exceptions;
using System.Net;
using Newtonsoft.Json;

namespace UploadArtifactApi;

public class HttpArtifactUploadApi : IArtifactUploadApi, IDisposable
{
    private readonly HttpClient _httpClient;
    private const string UploadPath = "api/key-artifact-upload/v1-0/private/artifact";

    public HttpArtifactUploadApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PrivateArtifactModel> ArtifactUploadV10PrivateArtifactPostAsync(
        FileStream fileStream, 
        string name, 
        string version, 
        string contentType, 
        string key, 
        CancellationToken cancellationToken)
    {
        using var formData = new MultipartFormDataContent();
        formData.Headers.Add("Dcp-Key", key);
        formData.Add(new StringContent(name), "name"); 
        formData.Add(new StringContent(version), "version");
        formData.Add(new StringContent(contentType), "contentType");
        formData.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileStream.Name));

        var response = await _httpClient.PostAsync(UploadPath, formData, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<PrivateArtifactModel>(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            throw new KeyException($"The upload api returned a {response.StatusCode} response.");
        }

        throw new UploadPackageException($"The upload api returned a {response.StatusCode} response.");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}