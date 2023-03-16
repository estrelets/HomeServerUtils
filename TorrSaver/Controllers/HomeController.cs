using Microsoft.AspNetCore.Mvc;
using TorrSaver.Downloaders;

namespace TorrSaver.Controllers;

public record HomeModel(DownloadStatus[] Statuses, TorrMedia?[] TorrMedias);

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TorrClient _client;
    private readonly Downloader _downloader;

    public HomeController(ILogger<HomeController> logger, Downloader downloader, TorrClient client)
    {
        _logger = logger;
        _downloader = downloader;
        _client = client;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var torrents = await _client.GetMedias(ct);
        var statuses = _downloader.GetList();
        var model = new HomeModel(statuses, torrents);
        return View(model);
    }

    public async Task<IActionResult> Start(string link, CancellationToken ct)
    {
        var torrents = await _client.GetMedias(ct);

        var media = torrents.FirstOrDefault(x => x.Link == link);

        if (media == null)
        {
            return NotFound();
        }

        _downloader.Download(media, ct);
        return RedirectToAction("Index");
    }

    public IActionResult Stop(Guid id)
    {
        _downloader.Delete(id);
        return RedirectToAction("Index");
    }
}