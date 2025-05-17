using FlwnMediaAPI.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FlwnMediaAPI.Controllers;

[EnableCors("flwn")]
[ApiController]
[Route("")]
public class FlwnMediaController(ILogger<FlwnMediaController> logger) : ControllerBase
{
    private static readonly Random _random = new();
    private string _mediaPath = "/opt/flwnfiles/media";

    private List<MediaFile> GetFiles()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") _mediaPath = "Z:/opt/flwnfiles/media";

        // recursively get all files in the directory and subdirectoriess
        var files = Directory.GetFiles(_mediaPath, "*", SearchOption.AllDirectories);
        var fileList = new List<MediaFile>();
        foreach (var e in files)
        {
            var file = new MediaFile();
            file.FileName = Path.GetFileName(e);
            file.Link = $"https://files.flwn.dev/media/{Uri.EscapeDataString(file.FileName)}";
            fileList.Add(file);
        }

        return fileList;
    }

    [HttpGet]
    public ActionResult<IEnumerable<MediaFile>> GetAll()
    {
        return GetFiles();
    }

    [HttpGet("random")]
    public ActionResult<MediaFile> GetRandom()
    {
        // all files except Thumbs.db
        var files = GetFiles().Where(x => !x.FileName.Equals("Thumbs.db")).ToList();
        // get query parameter
        var query = Request.Query["onlyvideo"];
        if (query.Count > 0)
        {
            files = files.Where(x => x.FileName.EndsWith(".mp4") || x.FileName.EndsWith(".webm") || x.FileName.EndsWith(".mov")).ToList();
        }

        var random = new Random();

        return files[random.Next(files.Count)];
    }

    [HttpGet("visitrandom")]
    public ActionResult GetVisitRandom()
    {
        // all files except Thumbs.db
        var files = GetFiles().Where(x => !x.FileName.Equals("Thumbs.db")).ToList();
        // get query parameter
        var query = Request.Query["onlyvideo"];
        if (query.Count > 0)
        {
            files = files.Where(x => x.FileName.EndsWith(".mp4") || x.FileName.EndsWith(".webm") || x.FileName.EndsWith(".mov")).ToList();
        }

        return Redirect(files[_random.Next(files.Count)].Link);
    }


    [HttpGet("randommp4")]
    public IResult GetRandomMp4()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") _mediaPath = "Z:/opt/flwnfiles/media";

        // recursively get all files in the directory and subdirectoriess
        var files = Directory.GetFiles(_mediaPath, "*", SearchOption.AllDirectories);
        var mp4Files = files.Where(x => x.EndsWith(".mp4")).ToList();

        var mp4File = mp4Files[_random.Next(mp4Files.Count)];
        var realIp = Request.Headers["CF-Connecting-IP"];
        if (realIp.Count == 0) realIp = Request.Headers["X-Forwarded-For"];
        logger.Log(LogLevel.Information, $"Streaming {mp4File} | {realIp.ToString()}");

        // stream random mp4 file as content
        return Results.Stream(
            new FileStream(mp4File, FileMode.Open, FileAccess.Read),
            contentType: "video/mp4"
        );
    }

    [HttpGet("votv")]
    public ActionResult GetVotv()
    {
        // get all video files
        var files = GetFiles().Where(x => x.FileName.EndsWith(".mp4") || x.FileName.EndsWith(".webm") || x.FileName.EndsWith(".mov")).ToList();

        var file = string.Join("\n", files.Select(x => $"{x.FileName}\n{x.Link}"));
        return Content(file, "text/plain");
    }

    // stats endpoints
    [HttpGet("stats")]
    public ActionResult<Stats> GetStats()
    {
        var files = GetFiles();
        var stats = new Stats();
        stats.FileCount = files.Count;

        // Image extensions
        stats.ImageCount = files.Count(x => x.FileName.EndsWith(".png"));
        stats.ImageCount += files.Count(x => x.FileName.EndsWith(".jpg"));
        stats.ImageCount += files.Count(x => x.FileName.EndsWith(".jpeg"));
        stats.ImageCount += files.Count(x => x.FileName.EndsWith(".gif"));
        stats.ImageCount += files.Count(x => x.FileName.EndsWith(".webp"));

        // Video extensions
        stats.VideoCount = files.Count(x => x.FileName.EndsWith(".mp4"));
        stats.VideoCount += files.Count(x => x.FileName.EndsWith(".webm"));
        stats.VideoCount += files.Count(x => x.FileName.EndsWith(".mov"));
        return stats;
    }
}