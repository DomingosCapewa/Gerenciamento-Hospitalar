public interface IRabbitPublisher
{
    void Publish(string queueName, object payload);
}
