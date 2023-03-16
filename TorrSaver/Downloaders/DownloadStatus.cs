namespace TorrSaver.Downloaders;

public class DownloadStatus
{
    public DownloadStatus(TorrMedia media, CancellationToken baseToken)
    {
        Id = Guid.NewGuid();
        Media = media;
        CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(baseToken);
    }

    public Guid Id { get; set; }
    public TorrMedia Media { get; set; }
    public DownloadState State { get; set; }
    public long CompleteBytes { get; private set; }
    public int Speed { get; set; }
    public string? ErrorMessage { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
    
    private DateTime _lastBytesUpdate = DateTime.Now;

    public void AddBytes(int count)
    {
        var lastUpdate = _lastBytesUpdate;
        _lastBytesUpdate = DateTime.Now;
        CompleteBytes += count;
        
        var elapsed = DateTime.Now - lastUpdate;
        var speed = count / elapsed.TotalSeconds;
        Speed = (int)speed;
    }
}