using System.Buffers;
using System.Collections.Concurrent;

namespace TorrSaver.Downloaders;

public class Downloader
{
    private const int BufferSize = 1024 * 1024;                                             
                                                                                        
    private readonly Settings _settings;                                                    
    private readonly HttpClient _httpClient = new HttpClient();                             
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;                   
    private readonly ConcurrentDictionary<TorrMedia, DownloadStatus> _downloads = new();
                             
    public Downloader(Settings settings)
    {
        _settings = settings;
    }

    public async Task StartTasks(CancellationToken ct)
    {
        foreach (var downloadStatus in _downloads.Values)
        {
            if (downloadStatus.State == DownloadState.None)
            {
                await Task.Factory.StartNew(
                    async () => await Download(downloadStatus, downloadStatus.CancellationTokenSource.Token), 
                    TaskCreationOptions.LongRunning);
            }
        }
    }
    
    private async Task Download(DownloadStatus status, CancellationToken ct)
    {
        if (status.State != DownloadState.None)
        {
            return;
        }

        status.State = DownloadState.InProgress;
        byte[]? buffer = null;

        var path = Path.Combine(_settings.Directory, status.Media.Name);
    
        try
        {
            await using var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            await using var stream = await _httpClient.GetStreamAsync(status.Media.Link, ct);

            while (stream.CanRead)
            {
                buffer = _arrayPool.Rent(BufferSize);

                var read = await stream.ReadAsync(buffer, 0, BufferSize, ct);

                if (read == 0)
                {
                    break;
                }

                status.AddBytes(read);

                await file.WriteAsync(buffer, 0, read, ct);
                _arrayPool.Return(buffer);
            }

            status.State = DownloadState.Finished;
        }
        catch (Exception ex)
        {
            status.State = DownloadState.Error;
            status.ErrorMessage = ex.ToString();
    
            if (buffer != null)
            {
                _arrayPool.Return(buffer);
            }
        }
    }
    
    public DownloadStatus[] GetList()
    {
        return _downloads.Values.ToArray();
    }

    public DownloadStatus Download(TorrMedia media, CancellationToken ct)
    {
        if (_downloads.TryGetValue(media, out var started))
        {
            if (started.State == DownloadState.Error)
            {
                Delete(started);
                return Download(media, ct);
            }
            return started;
        }

        return _downloads.AddOrUpdate(media, tm => new DownloadStatus(tm, ct), (_, x) => x);
    }

    public void Delete(DownloadStatus status)
    {
        status.CancellationTokenSource.Cancel();
        _downloads.Remove(status.Media, out var _);
    }

    public void Delete(Guid id)
    {
        var status = _downloads.Values.FirstOrDefault(x => x.Id == id);

        if (status != null)
        {
            Delete(status);
        }
    }
}