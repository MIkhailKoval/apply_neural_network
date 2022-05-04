using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using apply_neural_network.RabbitMq;

using apply_neural_network.databases;

namespace apply_neural_network.RabbitMq.Controllers
{
    public class StatusController: Controller
    {

        public StatusController()
        {
        }

        [Route("/api/get/status/")]
        [HttpPost]
        public IActionResult GetStatus(string taskId)
        {
            using (var sqlite = new Sqllite()) {
            var sqlite_datareader = sqlite.selectQuery("select status from tasks where task_id = \'" + taskId + "\'");
                if (sqlite_datareader is null) {
                    
                    return NotFound();
                }
                while (sqlite_datareader.Read())
                {
                    string status = sqlite_datareader.GetString(0);
                    return Ok(status);
                }
                    return NotFound();
            }
        }
    }
}