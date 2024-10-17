using System.Text;
using RabbitMQ.Client;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare fanout exchange
channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

// Publish message to named exchange instead of default exchange 
channel.BasicPublish(exchange: "logs",
                     routingKey: string.Empty,
                     basicProperties: null,
                     body: body);
Console.WriteLine($" [x] Sent {message}");

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}