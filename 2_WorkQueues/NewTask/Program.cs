using System.Text;
using RabbitMQ.Client;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare queue to publish to
channel.QueueDeclare(queue: "task_queue",
                     // Declare queue as durable (survive server restarts)
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var message = GetMessage(args); 
var body = Encoding.UTF8.GetBytes(message);

// Mark messages as persistent
var properties = channel.CreateBasicProperties();
properties.Persistent = true;

// Publish message to queue
channel.BasicPublish(exchange: string.Empty,
                     routingKey: "task_queue",
                     basicProperties: properties,
                     body: body);
Console.WriteLine($" [x] Sent {message}");

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}