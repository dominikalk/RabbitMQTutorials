// https://www.rabbitmq.com/tutorials/tutorial-five-dotnet

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare topic exchange
channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

// Declare a server-named queue
var queueName = channel.QueueDeclare().QueueName;

// Exit console if no binding key is provided
if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [binding_key...]",
                            Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

foreach (var bindingKey in args)
{
    // Bind queue to exchange with routing key
    channel.QueueBind(queue: queueName,
                      exchange: "topic_logs",
                      routingKey: bindingKey);
}

Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

// Declare callback to handle incoming messages
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var routingKey = ea.RoutingKey;
    Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
};
// Start a basic content-class consumer
channel.BasicConsume(queue: queueName,
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();