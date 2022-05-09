using Microsoft.AspNetCore.Mvc;

using apply_neural_network.databases;

namespace apply_neural_network.RabbitMq.Controllers
{
    public class StatusController : Controller, IStatusController
    {

        public StatusController()
        {
        }
        /// <summary>
        /// Get status of task by its task id.
        /// </summary>
        /// <response code="200">Get status</response>
        /// <response code="404">Task id not found in database</response>
        /// <response code="500">Something went wrong. Server Error</response>
        [Route("/api/get/status/")]
        [HttpGet]
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