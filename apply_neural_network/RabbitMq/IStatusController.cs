using Microsoft.AspNetCore.Mvc;

namespace apply_neural_network.RabbitMq
{
    public interface IStatusController
    {
        public IActionResult GetStatus(string fileId);
    }
}