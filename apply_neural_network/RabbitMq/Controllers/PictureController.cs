using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using apply_neural_network.RabbitMq;

public class Picture {
    public byte[] image {get;}

    public Picture(byte[] image) {
        this.image = image;
    }
}
public class PictureController : Controller
{
        IWebHostEnvironment _appEnvironment;
 	    private readonly IRabbitMqService _mqService;
        public PictureController(IWebHostEnvironment appEnvironment,IRabbitMqService mqService)
        {
            _appEnvironment = appEnvironment;
            _mqService = mqService;
        }
        public IActionResult Index()
        {
            return View(new List<Picture>());
        }
        [Route("/api/upload")]
       [HttpPost]
        public Tuple<string, string> AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                var taskId = Guid.NewGuid().ToString();
                string path = "/pictures_data/" + taskId + '#' + uploadedFile.FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    uploadedFile.CopyTo(fileStream);
                }
                _mqService.SendMessage(taskId + '#' + uploadedFile.FileName);
                return new Tuple<string, string>(taskId, taskId + '#' + uploadedFile.FileName);
            } else {
                return null;
            }
            
        }

}
