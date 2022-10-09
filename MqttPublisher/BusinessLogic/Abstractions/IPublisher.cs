using System.Threading;
using System.Threading.Tasks;

namespace MqttPublisher
{
    /// <summary>
    /// Provides interaction with MQTT broker
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// Connects to the broker
        /// </summary>
        Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnect from the broker
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Sends data to the broker
        /// </summary>
        Task SendMessagesAsync(CancellationToken cancellationToken);
    }
}