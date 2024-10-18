// https://www.rabbitmq.com/tutorials/tutorial-three-dotnet

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare fanout exchange
channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

// Declare a server-named queue
var queueName = channel.QueueDeclare().QueueName;
channel.QueueBind(queue: queueName,
                  exchange: "logs",
                  routingKey: string.Empty);

Console.WriteLine(" [*] Waiting for logs.");

// Declare callback to handle incoming messages
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    byte[] body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] {message}");
};
// Start a basic content-class consumer
channel.BasicConsume(queue: queueName,
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();