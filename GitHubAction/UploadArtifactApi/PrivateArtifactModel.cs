using Newtonsoft.Json;

namespace UploadArtifactApi
{
    public class PrivateArtifactModel
    {
        [JsonProperty("artifactId")]
        public string? ArtifactId { get; set; }
    }
}