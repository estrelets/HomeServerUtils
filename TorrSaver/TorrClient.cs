using System.Collections.Concurrent;
using PlaylistsNET.Models;

// ReSharper disable InconsistentNaming

namespace TorrSaver;

public record TorrMedia(string Name, string Link);

public class TorrClient
{
    private readonly string _baseUrl;
    private readonly HttpClient _client;

    public TorrClient(Settings settings)
    {
        _baseUrl = settings.BaseUrl;
        _client = new HttpClient();
    }

    public async Task<TorrMedia[]> GetMedias(CancellationToken ct)
    {
        var result = new ConcurrentBag<TorrMedia>();
        await ParseSubMedias(_baseUrl + "/playlistall/all.m3u", result, ct);
        return result
            .OrderBy(x => x.Name)
            .ToArray();
    }

    private async Task<bool> ParseSubMedias(string link, ConcurrentBag<TorrMedia> result, CancellationToken ct)
    {
        if (!link.Contains(".m3u"))
        {
            return false;
        }
        
        var m3u = await DownloadM3U(link, ct);

        if (m3u == null)
        {
            return false;
        }

        if (m3u.PlaylistEntries.Count > 0)
        {
            await Parallel.ForEachAsync(
                m3u.PlaylistEntries, 
                ct, 
                async (subMedia, ct) => await ParseMedias(subMedia, result, ct));

            return true;
        }

        return false;
    }
    
    private async Task ParseMedias(M3uPlaylistEntry? media, ConcurrentBag<TorrMedia> result, CancellationToken ct)
    {
        if (media == null)
        {
            return;
        }

        if (await ParseSubMedias(media.Path, result, ct))
        {
            return;
        }

        var name = media.Title;
        var link = media.Path;

        result.Add(new TorrMedia(name, link));
    }

    private async Task<M3uPlaylist?> DownloadM3U(string link, CancellationToken ct)
    {
        var m3uContent = await _client.GetStringAsync(link, ct);
        return new PlaylistsNET.Content.M3uContent().GetFromString(m3uContent);
    }
}

