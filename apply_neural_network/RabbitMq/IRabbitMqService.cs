namespace apply_neural_network.RabbitMq
{
    public interface IRabbitMqService
    {
        void SendMessage(string message);
    }
}