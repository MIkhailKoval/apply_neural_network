using Microsoft.AspNetCore.Mvc;
using apply_neural_network.RabbitMq;
using apply_neural_network.databases;

public class Picture
{
    public byte[] image { get; }

    public Picture(byte[] image)
    {
        this.image = image;
    }
}

public struct UploadingResult
{
    /// <summary>
    /// task id in format uuid
    /// </summary>
    public String taskId { get; set; }
    /// <summary>
    /// file id in format uuid + '#' + input filename
    /// </summary>
    public String fileId { get; set; }
    public UploadingResult(String taskId, String fileId)
    {
        this.taskId = taskId;
        this.fileId = fileId;
    }
}

public class PictureController : Controller
{
    IWebHostEnvironment _appEnvironment;
    private readonly IRabbitMqService _mqService;
    private readonly IStatusController _statusController;
    public PictureController(IWebHostEnvironment appEnvironment, IRabbitMqService mqService, IStatusController statusController)
    {
        _appEnvironment = appEnvironment;
        _mqService = mqService;
        _statusController = statusController;
    }

    /// <summary>
    /// Download prediction by file id, if task was succesed.
    /// </summary>
    /// <response code="200">Return prediction for photo</response>
    /// <response code="400">Bad file id</response>
    /// <response code="409">Status in a bad state</response>
    /// <response code="500">Something went wrong. Server Error</response>
    [Route("/api/download/result")]
    [HttpGet]
    public IActionResult DownloadResult(String fileId)
    {
        var index = fileId.IndexOf('#');
        if (index == -1)
        {
            return BadRequest("bad file id!");
        }
        var taskId = fileId.Substring(0, index);
        var res = (ObjectResult)_statusController.GetStatus(taskId);
        switch (res.Value)
        {
            case "PENDING":
            case "STARTED":
                return Ok("Result doesn't ready. Try it later!");
            case "FAILURE":
                return Ok("Task failed!");
            case "SUCCESS":
                var result = GetResult(taskId);
                if (result is null)
                {
                    return StatusCode(500);
                }
                else
                {
                    return Ok(result);
                }
            default:
                return Conflict();
        }
    }

    /// <summary>
    /// Add file in queue of tasks, which will be processed by neural network.
    /// </summary>
    /// <response code="200">File Added</response>
    /// <response code="400">Empty file was uploaded</response>
    /// <response code="500">Something went wrong. Server Error</response>
    [Route("/api/upload")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AddFile(IFormFile uploadedFile)
    {
        if (uploadedFile != null)
        {
            var taskId = Guid.NewGuid().ToString();
            var fileId = taskId + '#' + uploadedFile.FileName;
            string path = "/pictures_data/" + fileId;

            // сохраняем файл в папку Files в каталоге wwwroot
            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
            {
                uploadedFile.CopyTo(fileStream);
            }
            _mqService.SendMessage(fileId);
            return Ok(new UploadingResult(taskId, fileId));
        }
        else
        {
            return BadRequest("empty file was uploaded!");
        }

    }
    String? GetResult(String taskId)
    {
        using (var sql = new Postgres())
        {
            var datareader = sql.selectQuery(taskId);
            if (sql is null)
            {

                return null;
            }
            while (datareader.Read())
            {
                string result = datareader.GetString(1);
                return result;
            }
        }
        return null;
    }
}
