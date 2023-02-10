using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Indexers.MediathekViewRSSIndexer;

/// <author email="Sythelux Rikd">Sythelux Rikd</author>
public class VideoRssIndexer : HttpIndexerBase<VideoRssIndexerSettings>
{
    public override string Name => "Video RSS Feed";
    public override DownloadProtocol Protocol => DownloadProtocol.Torrent; // TODO: Download
    public override bool SupportsSearch => true;
    public override int PageSize => 0;

    private readonly IVideoRssParserFactory _videoRssParserFactory;
    private readonly ISeriesService _seriesService;

    public VideoRssIndexer(IVideoRssParserFactory videoRssParserFactory, IHttpClient httpClient, ISeriesService seriesService, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
        : base(httpClient, indexerStatusService, configService, parsingService, logger)
    {
        _videoRssParserFactory = videoRssParserFactory;
        _seriesService = seriesService;
    }

    public override IIndexerRequestGenerator GetRequestGenerator()
    {
        return new VideoRssIndexerRequestGenerator { Settings = Settings, Logger = _logger };
    }

    public override IParseIndexerResponse GetParser()
    {
        return _videoRssParserFactory.GetParser(Settings);
    }
}
