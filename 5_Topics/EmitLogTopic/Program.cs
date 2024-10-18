// https://www.rabbitmq.com/tutorials/tutorial-five-dotnet

using System.Text;
using RabbitMQ.Client;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare topic exchange
channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
var message = (args.Length > 1)
              ? string.Join(" ", args.Skip(1).ToArray())
              : "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

// Publish message to named exchange instead of default exchange
channel.BasicPublish(exchange: "topic_logs",
                     // Define routing key for exchange to utilise
                     routingKey: routingKey,
                     basicProperties: null,
                     body: body);
Console.WriteLine($" [x] Sent '{routingKey}':'{message}'");