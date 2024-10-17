using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Create connection to the server
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare queue to consume from
channel.QueueDeclare(queue: "task_queue",
                     // Declare queue as durable (survive server restarts)
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

// Ensure message is only sent to worker when it is not busy
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

Console.WriteLine(" [*] Waiting for messages.");

// Declare callback to handle incoming messages
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");

    // Simulate work
    int dots = message.Split('.').Length - 1;
    Thread.Sleep(dots * 1000);

    Console.WriteLine(" [x] Done");

    // Send acknowledgement
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

// Start a basic content-class consumer
channel.BasicConsume(queue: "task_queue",
                     // Set auto acknowledgment to false
                     autoAck: false,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();