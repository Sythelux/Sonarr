namespace NzbDrone.Core.Indexers.MediathekViewRSSIndexer;

public class VideoRssIndexerParserSettings
{
    public bool UseEnclosureUrl { get; set; }
    public bool UseEnclosureLength { get; set; }
    public bool ParseSizeInDescription { get; set; }
    public string SizeElementName { get; set; }
}
