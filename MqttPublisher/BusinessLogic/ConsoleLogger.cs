using System;

namespace MqttPublisher
{
    public class ConsoleLogger : ILogger
    {
        public void Print(string message)
        {
            Console.WriteLine(message);
        }
    }
}
