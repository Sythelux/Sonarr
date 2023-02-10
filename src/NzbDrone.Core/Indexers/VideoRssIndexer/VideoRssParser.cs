using System;
using System.Diagnostics;
using System.Xml.Linq;
using FFMpegCore;
using Instances;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Indexers.MediathekViewRSSIndexer;

public class VideoRssParser : RssParser
{
    protected override bool PreProcess(IndexerResponse indexerResponse)
    {
        return base.PreProcess(indexerResponse);
    }

    protected override ReleaseInfo PostProcessItem(XElement item, ReleaseInfo releaseInfo)
    {
        var postProcessItem = base.PostProcessItem(item, releaseInfo);
        if (releaseInfo.Title.Contains("Originalversion"))
        {
            releaseInfo.Title = releaseInfo.Title.Replace("Originalversion", "english");
        }
        else
        {
            releaseInfo.Title += " - (german)";
        }

        var duration = Convert.ToInt64(item?.Element("duration")?.Value);
        var enclosure = item?.Element("enclosure");
        if (enclosure != null)
        {
            releaseInfo.Codec = enclosure.Attribute("type")?.Value;
        }

        releaseInfo.InfoUrl = item?.Element("websiteUrl")?.Value;
        releaseInfo.Origin = item?.Element(XName.Get("creator", "http://purl.org/dc/elements/1.1/"))?.Value;
        releaseInfo.Source = item?.Element("duration")?.Value;

        var category = item?.Element("category")?.Value; // item?.Document?.Element("rss")?.Element("channel")?.Element("description")?.Value;
        if (category != null)
        {
            var series = SeriesService.FindByTitle(category + (duration / 60 > 30 ? "2005" : "")); // TODO: settings value
            if (series != null)
            {
                releaseInfo.TvdbId = series.TvdbId;
            }
        }

        // releaseInfo.Duration = ;
        var link = item?.Element("link")?.Value;
        if (link != null)
        {
            var ffOptions = GlobalFFOptions.Current;
            ffOptions.ExtensionOverrides.Clear();
            ffOptions.WorkingDirectory = ".";
            ffOptions.BinaryFolder = "/usr/bin/";
            try
            {
                var analysis = FFProbe.Analyse(new Uri(link), ffOptions: ffOptions);
                var videoStream = analysis.PrimaryVideoStream;
                if (videoStream != null)
                {
                    releaseInfo.Codec = videoStream.CodecName;
                    releaseInfo.Container = videoStream.Profile;
                    releaseInfo.Title += " - [" + videoStream.Height + "p]";

                    // releaseInfo.Resolution = "WEBDL-" + videoStream.Height + "p";
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        postProcessItem.DownloadProtocol = DownloadProtocol.Torrent;
        return postProcessItem;
    }

    protected override long GetSize(XElement item)
    {
        var enclosure = item?.Element("enclosure");
        return enclosure != null
            ? Convert.ToInt64(enclosure.Attribute("length")?.Value)
            : base.GetSize(item);
    }

    public ISeriesService SeriesService { get; set; }
}
