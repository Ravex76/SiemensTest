using System.Threading;
using System.Threading.Tasks;

namespace MqttPublisher
{
    public interface IPublisher
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task SendMessagesAsync(CancellationToken cancellationToken);
    }
}