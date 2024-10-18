// https://www.rabbitmq.com/tutorials/tutorial-six-dotnet

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare queue to consume from
channel.QueueDeclare(queue: "rpc_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

// Ensure message is only sent to worker when it is not busy
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

// Start a basic content-class consumer
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(queue: "rpc_queue",
                     // Set auto acknowledgment to false
                     autoAck: false,
                     consumer: consumer);
Console.WriteLine(" [x] Awaiting RPC requests");

// Declare callback to handle incoming messages
consumer.Received += (model, ea) =>
{
    string response = string.Empty;

    var body = ea.Body.ToArray();
    var props = ea.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    // Set reply correlation ID to match request
    replyProps.CorrelationId = props.CorrelationId;

    try
    {
        // Calculate fibonacci number
        var message = Encoding.UTF8.GetString(body);
        int n = int.Parse(message);
        Console.WriteLine($" [.] Fib({message})");
        response = Fib(n).ToString();
    }
    catch (Exception e)
    {
        Console.WriteLine($" [.] {e.Message}");
        response = string.Empty;
    }
    finally
    {
        // Publish message 
        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish(exchange: string.Empty,
                             // Callback routing key provided by client
                             routingKey: props.ReplyTo,
                             // Provide correlation ID to client
                             basicProperties: replyProps,
                             body: responseBytes);

        // Send acknowledgement
        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    }
};

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

// Assumes only valid positive integer input.
// Don't expect this one to work for big numbers, and it's probably the slowest recursive implementation possible.
static int Fib(int n)
{
    if (n is 0 or 1)
    {
        return n;
    }

    return Fib(n - 1) + Fib(n - 2);
}