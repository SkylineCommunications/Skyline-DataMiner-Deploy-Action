namespace Catalog.Domain
{
    public class CatalogData
    {
		public string Version { get; set; } = "";
        public string Branch { get; set; } = "";
		public bool IsPreRelease { get; set; }
		public string Identifier { get; set; } = "";
		public string Name { get; set; } = "";
		public string ContentType { get; set; } = "";
		public string CommitterMail { get; set; } = "";
		public string ReleaseUri { get; set; } = "";

		public override bool Equals(object? obj)
		{
			return obj is CatalogData data &&
				   Version == data.Version &&
				   Branch == data.Branch &&
				   IsPreRelease == data.IsPreRelease &&
				   Identifier == data.Identifier &&
				   Name == data.Name &&
				   CommitterMail == data.CommitterMail &&
				   ReleaseUri == data.ReleaseUri &&
				   ContentType == data.ContentType;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Version, Branch, IsPreRelease, Identifier, Name, ContentType, CommitterMail, ReleaseUri);
		}
	}
}
