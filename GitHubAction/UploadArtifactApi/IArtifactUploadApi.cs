using Package.Domain.Exceptions;
using Package.Domain.Services;

namespace UploadArtifactApi;

public interface IArtifactUploadApi
{
    /// <summary>
    /// Uploads the package.
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <param name="contentType"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The uploaded package if succeeded.</returns>
    /// <exception cref="KeyException">When the <paramref name="key"/> isn't valid or authorized to execute this action.</exception>
    /// <exception cref="UploadPackageException">When something went wrong while uploading the package.</exception>
    Task<PrivateArtifactModel> ArtifactUploadV11PrivateArtifactPostAsync(byte[] package, string name, string version, string contentType, string key, CancellationToken cancellationToken, IPackagePresenter presenter);
}