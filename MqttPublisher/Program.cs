using MQTTnet;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace MqttPublisher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Console.CancelKeyPress += (sender, e) =>
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                }
            };

            using (var mqttClient = new MqttFactory().CreateMqttClient())
            {

                var publisher = new Publisher(mqttClient, new ConsoleLogger());
                var connectResult = await publisher.ConnectAsync();

                if (connectResult)
                {
                    await publisher.SendMessagesAsync(token);
                    await publisher.DisconnectAsync();
                }
            }

            Console.WriteLine("Click Enter for exit");
            Console.ReadLine();
        }
    }
}
