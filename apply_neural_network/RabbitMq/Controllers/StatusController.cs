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
    public class StatusController : Controller, IStatusController
    {

        public StatusController()
        {
        }

        [Route("/api/get/status/")]
        [HttpPost]
        public IActionResult GetStatus(string taskId)
        {
            using (var sql = new Postgres())
            {
                var datareader = sql.selectQuery(taskId);
                if (sql is null)
                {

                    return NotFound();
                }
                while (datareader.Read())
                {
                    string status = datareader.GetString(0);
                    return Ok(status);
                }
                return NotFound();
            }
        }
    }
}