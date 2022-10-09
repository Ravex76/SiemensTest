using System;

namespace MqttPublisher
{
    /// <summary>
    /// Provides information output to the console
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <inheritdoc/>
        public void Print(string message)
        {
            Console.WriteLine(message);
        }
    }
}
