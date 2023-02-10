using System;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Indexers.MediathekViewRSSIndexer;

public interface IVideoRssParserFactory
{
    VideoRssParser GetParser(VideoRssIndexerSettings settings);
}

public class VideoRssParserFactory : IVideoRssParserFactory
{
    private readonly ISeriesService _seriesService;
    protected readonly Logger _logger;

    private readonly ICached<VideoRssIndexerParserSettings> _settingsCache;

    public VideoRssParserFactory(ICacheManager cacheManager, ISeriesService seriesService, Logger logger)
    {
        _settingsCache = cacheManager.GetCache<VideoRssIndexerParserSettings>(GetType());
        _seriesService = seriesService;
        _logger = logger;
    }

    public VideoRssParser GetParser(VideoRssIndexerSettings indexerSettings)
    {
        var key = indexerSettings.ToJson();
        var parserSettings = _settingsCache.Get(key, () => DetectParserSettings(indexerSettings), TimeSpan.FromDays(7));

        return new VideoRssParser
        {
            UseGuidInfoUrl = false,
            SeriesService = _seriesService,
            UseEnclosureUrl = parserSettings.UseEnclosureUrl,
            UseEnclosureLength = parserSettings.UseEnclosureLength,
            ParseSizeInDescription = parserSettings.ParseSizeInDescription,
        };
    }

    private VideoRssIndexerParserSettings DetectParserSettings(VideoRssIndexerSettings indexerSettings)
    {
        var settings = new VideoRssIndexerParserSettings();
        return settings;
    }
}
