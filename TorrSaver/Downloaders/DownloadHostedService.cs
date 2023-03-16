namespace TorrSaver.Downloaders;

public class DownloadHostedService : IHostedService
{
    private readonly Downloader _downloader;

    public DownloadHostedService(Downloader downloader)
    {
        _downloader = downloader;
    }
    
    private CancellationTokenSource? _cts;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StopAsync(cancellationToken);
        _cts = new CancellationTokenSource();
        Task.Factory.StartNew(async () => await StartWorker(_cts.Token), TaskCreationOptions.LongRunning);  
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts = null;
        }
        
        return Task.CompletedTask;
    }

    private async Task StartWorker(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await _downloader.StartTasks(ct);
            await Task.Delay(TimeSpan.FromSeconds(0.5f), ct);
        }
    }
}