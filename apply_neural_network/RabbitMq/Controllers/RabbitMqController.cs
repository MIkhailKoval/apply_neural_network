using Microsoft.AspNetCore.Mvc;
using apply_neural_network.RabbitMq;

#pragma warning disable CS1591
public class RabbitMqController : Controller
{
    private readonly IRabbitMqService _mqService;

    public RabbitMqController(IRabbitMqService mqService)
    {
        _mqService = mqService;
    }

    public IActionResult SendMessage(string message)
    {
        _mqService.SendMessage(message);

        return Ok("Сообщение отправлено");
    }
}