namespace Catalog.Domain
{
	/// <summary>
	/// IMPORTANT: Do not use this class as the key of a Dictionary or HashSet. HashCode is overridden but the contents are not immutable.
	/// </summary>
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

		// Used during unit testing to assert data.
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

		// Needed to match with Equals
		public override int GetHashCode()
		{
			return HashCode.Combine(Version, Branch, IsPreRelease, Identifier, Name, ContentType, CommitterMail, ReleaseUri);
		}
	}
}
