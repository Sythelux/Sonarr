using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Ffmpeg;

/// <author email="Sythelux Rikd">Sythelux Rikd</author>
public class FileDownloadClient : DownloadClientBase<FileSettings>
{
    public override string Name => "File";

    public override DownloadProtocol Protocol => DownloadProtocol.Torrent;

    // private Dictionary<string, Task<string>> downloadTasks;
    private readonly IHttpClient _httpClient;
    private static readonly Dictionary<string, FileDownloadClientItem> _downloadTasks = new Dictionary<string, FileDownloadClientItem>();

    public FileDownloadClient(IHttpClient httpClient, IConfigService configService, IDiskProvider diskProvider, IRemotePathMappingService remotePathMappingService, Logger logger)
        : base(configService, diskProvider, remotePathMappingService, logger)
    {
        _httpClient = httpClient;
    }

    public override string Download(RemoteEpisode remoteEpisode)
    {
        var url = remoteEpisode.Release.DownloadUrl;
        var title = remoteEpisode.Release.Title;

        if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
        {
            throw new NotSupportedException("Full season releases are not supported with File.");
        }

        title = FileNameBuilder.CleanFileName(title);

        var file = Path.Combine(Settings.StorageFolder, title + ".mp4");

        var downloadClientItem = new FileDownloadClientItem(remoteEpisode, url, file, () =>
        {
            try
            {
                DownloadFile(url, file);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        });
        downloadClientItem.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this);
        if (_downloadTasks.ContainsKey(downloadClientItem.DownloadId))
        {
            return null;
        }

        _downloadTasks.Add(downloadClientItem.DownloadId, downloadClientItem);
        downloadClientItem.Start();

        return downloadClientItem.DownloadId;
    }

    public void DownloadFile(string url, string fileName)
    {
        var fileNamePart = fileName + ".part";

        try
        {
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            _logger.Debug("Downloading [{0}] to [{1}]", url, fileName);

            var stopWatch = Stopwatch.StartNew();
            using (var fileStream = new FileStream(fileNamePart, FileMode.Create, FileAccess.ReadWrite))
            {
                var request = new HttpRequest(url);
                request.AllowAutoRedirect = true;
                request.ResponseStream = fileStream;
                var response = _httpClient.Get(request);

                if (response.Headers.ContentType != null && response.Headers.ContentType.Contains("text/html"))
                {
                    throw new HttpException(request, response, "Site responded with html content.");
                }
            }

            stopWatch.Stop();
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.Move(fileNamePart, fileName);
            _logger.Debug("Downloading Completed. took {0:0}s", stopWatch.Elapsed.Seconds);
        }
        finally
        {
            if (File.Exists(fileNamePart))
            {
                File.Delete(fileNamePart);
            }
        }
    }

    public override IEnumerable<DownloadClientItem> GetItems()
    {
        return _downloadTasks.Values;
    }

    public override void RemoveItem(DownloadClientItem item, bool deleteData)
    {
        if (item is FileDownloadClientItem fItem)
        {
            if (_downloadTasks.ContainsKey(fItem.DownloadId))
            {
                fItem.StopAndDelete();
                _downloadTasks.Remove(fItem.DownloadId);
            }
        }
    }

    public override DownloadClientInfo GetStatus()
    {
        var downloadClientInfo = new DownloadClientInfo
        {
            IsLocalhost = true
        };
        return downloadClientInfo;
    }

    protected override void Test(List<ValidationFailure> failures)
    {
        failures.AddIfNotNull(TestFolder(Settings.StorageFolder, "StorageFolder"));

        // failures.AddIfNotNull(TestFolder(Settings.StrmFolder, "StrmFolder"));
    }
}
