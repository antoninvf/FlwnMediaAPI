using FlwnMediaAPI.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FlwnMediaAPI.Controllers;

[EnableCors("flwn")]
[ApiController]
[Route("")]
public class FlwnMediaController : ControllerBase
{
    private readonly ILogger<FlwnMediaController> _logger;

    public FlwnMediaController(ILogger<FlwnMediaController> logger)
    {
        _logger = logger;
    }

    private string path = "/opt/flwnfiles/media";

    private List<MediaFile> getFiles()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") path = "Z:/opt/flwnfiles/media";

        // recursively get all files in the directory and subdirectoriess
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        var fileList = new List<MediaFile>();
        foreach (var e in files)
        {
            var file = new MediaFile();
            file.FileName = Path.GetFileName(e);
            file.Link = $"https://m.flwn.dev/{Uri.EscapeDataString(file.FileName)}";
            fileList.Add(file);
        }

        return fileList;
    }

    [HttpGet]
    public ActionResult<IEnumerable<MediaFile>> GetAll()
    {
        return getFiles();
    }

    [HttpGet("random")]
    public ActionResult<MediaFile> GetRandom()
    {
        // all files except Thumbs.db
        var files = getFiles().Where(x => !x.FileName.Equals("Thumbs.db")).ToList();
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
        var files = getFiles().Where(x => !x.FileName.Equals("Thumbs.db")).ToList();
        // get query parameter
        var query = Request.Query["onlyvideo"];
        if (query.Count > 0)
        {
            files = files.Where(x => x.FileName.EndsWith(".mp4") || x.FileName.EndsWith(".webm") || x.FileName.EndsWith(".mov")).ToList();
        }
        
        var random = new Random();

        return Redirect(files[random.Next(files.Count)].Link);
    }
    
    [HttpGet("votv")]
    public ActionResult GetVotv()
    {
        // get all video files
        var files = getFiles().Where(x => x.FileName.EndsWith(".mp4") || x.FileName.EndsWith(".webm") || x.FileName.EndsWith(".mov")).ToList();

        var file = string.Join("\n", files.Select(x => $"{x.FileName}\n{x.Link}"));
        return Content(file, "text/plain");
    }

    // stats endpoints
    [HttpGet("stats")]
    public ActionResult<Stats> GetStats()
    {
        var files = getFiles();
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