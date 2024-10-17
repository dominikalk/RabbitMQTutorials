using System.Text;
using RabbitMQ.Client;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare direct exchange
channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);

var severity = (args.Length > 0) ? args[0] : "info";
var message = (args.Length > 1)
              ? string.Join(" ", args.Skip(1).ToArray())
              : "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

// Publish message to named exchange instead of default exchange 
channel.BasicPublish(exchange: "direct_logs",
                     // Define routing key for exchange to utilise
                     routingKey: severity,
                     basicProperties: null,
                     body: body);
Console.WriteLine($" [x] Sent '{severity}':'{message}'");