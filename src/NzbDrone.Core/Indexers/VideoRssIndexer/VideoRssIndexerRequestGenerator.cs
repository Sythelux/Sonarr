using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.MediathekViewRSSIndexer;

public class VideoRssIndexerRequestGenerator : IIndexerRequestGenerator
{
    public VideoRssIndexerSettings Settings { get; set; }
    public Logger Logger { get; set; }

    public virtual IndexerPageableRequestChain GetRecentRequests()
    {
        var pageableRequests = new IndexerPageableRequestChain();

        pageableRequests.Add(GetRssRequests("Doctor Who"));

        return pageableRequests;
    }

    public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
    {
        var pageableRequests = new IndexerPageableRequestChain();

        foreach (var cleanSceneTitle in searchCriteria.CleanSceneTitles)
        {
            pageableRequests.Add(GetRssRequests(string.Format(Settings.EpisodeSearchMask, cleanSceneTitle, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber)));
        }

        return pageableRequests;
    }

    public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
    {
        var pageableRequests = new IndexerPageableRequestChain();

        foreach (var cleanSceneTitle in searchCriteria.CleanSceneTitles)
        {
            pageableRequests.Add(GetRssRequests(string.Format(Settings.SeasonSearchMask, cleanSceneTitle, searchCriteria.SeasonNumber)));
        }

        return pageableRequests;
    }

    public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
    {
        return new IndexerPageableRequestChain();
    }

    public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
    {
        return new IndexerPageableRequestChain();
    }

    public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
    {
        return new IndexerPageableRequestChain();
    }

    public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
    {
        return new IndexerPageableRequestChain();
    }

    private IEnumerable<IndexerRequest> GetRssRequests(string searchParameters)
    {
        var request = new IndexerRequest(Settings.BaseUrl.Trim().TrimEnd('/') + (searchParameters == null
            ? ""
            : "?query=" + Uri.EscapeDataString(searchParameters)), HttpAccept.Rss);

        Logger.Debug("GetRssRequests: " + request.Url);

        if (Settings.Cookie.IsNotNullOrWhiteSpace())
        {
            foreach (var cookie in HttpHeader.ParseCookies(Settings.Cookie))
            {
                request.HttpRequest.Cookies[cookie.Key] = cookie.Value;
            }
        }

        yield return request;
    }
}
