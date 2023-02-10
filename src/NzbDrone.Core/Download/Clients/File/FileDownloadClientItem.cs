using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients.Ffmpeg;

public class FileDownloadClientItem : DownloadClientItem
{
    private readonly string _url;
    private readonly string _file;
    private readonly Action _downloadAction;
    private Task _task;
    private bool _running = true;

    public FileDownloadClientItem(RemoteEpisode remoteEpisode, string url, string file, Action downloadAction)
    {
        _url = url;
        _file = file;
        CanMoveFiles = true;
        Category = null;
        DownloadId = (_url + _file).SHA256Hash().ToUpper();
        IsEncrypted = false;
        Status = DownloadItemStatus.Queued;
        OutputPath = new OsPath(file);
        Removed = false;
        SeedRatio = 0;
        Title = remoteEpisode.Release.Title;
        TotalSize = remoteEpisode.Release.Size;
        _downloadAction = downloadAction;
    }

    public void StopAndDelete()
    {
        _running = false;
        Message = _task.Exception?.Message;
        if (File.Exists(_file))
        {
            File.Delete(_file);
        }
    }

    public void Start()
    {
        Task.Run(() => { _downloadAction.Invoke(); });
        _task = Task.Run(() =>
        {
            var fileInfoFinalPart = new FileInfo(_file + ".part");
            var fileInfoFinal = new FileInfo(_file);
            var watch = Stopwatch.StartNew();
            while (!fileInfoFinal.Exists && _running)
            {
                Thread.Sleep(100);
                if (fileInfoFinalPart.Exists)
                {
                    RemainingSize = TotalSize - fileInfoFinalPart.Length;
                    var downloadSpeed = fileInfoFinalPart.Length / (double)watch.ElapsedMilliseconds;
                    RemainingTime = downloadSpeed == 0 ? null : TimeSpan.FromMilliseconds(RemainingSize / downloadSpeed);
                    if (fileInfoFinalPart.Length > 0)
                    {
                        Status = DownloadItemStatus.Downloading;
                    }
                }
            }

            Status = _running ? DownloadItemStatus.Completed : DownloadItemStatus.Failed;

            // File.Move(_file, OutputP);

            CanBeRemoved = Status == DownloadItemStatus.Completed;
        });
        Message = _task.Exception?.Message;
    }
}
