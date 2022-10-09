namespace MqttPublisher
{
    /// <summary>
    /// It's responsible for information output 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Outputs message
        /// </summary>
        void Print(string message);
    }
}