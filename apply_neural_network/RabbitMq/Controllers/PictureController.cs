using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using apply_neural_network.RabbitMq;
using apply_neural_network.RabbitMq.Controllers;
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
    public String taskId { get; set; }
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
    [Route("/api/download/result")]
    [HttpPost]
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
                return Ok("Task failed");
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

    [Route("/api/upload")]
    [HttpPost]
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
