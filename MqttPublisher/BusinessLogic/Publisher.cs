using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttPublisher
{
    /// <inheritdoc cref="IPublisher" />
    public class Publisher : IPublisher
    {
        private const int _DELAY = 1000;
        private const string _TOPIC = "test/data";

        private readonly IMqttClient _mqttClient;
        private readonly ILogger _logger;
        private MqttClientOptions _mqttClientOptions;
        private string brokerHost;

        public Publisher(IMqttClient mqttClient, ILogger logger)
        {
            _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeClient();
        }

        /// <inheritdoc/>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                var result = await _mqttClient.ConnectAsync(_mqttClientOptions).ConfigureAwait(false);
                if (result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    _logger.Print($"There is no connection to the broker({brokerHost})");
                    return false;
                }
            }
            catch (MqttCommunicationException ex)
            {
                _logger.Print($"Exception occurred during the connection to the broker({brokerHost}): {ex.Message}");
                return false;
            }

            _logger.Print($"Connected to the broker({brokerHost})");
            return true;
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync().ConfigureAwait(false);
            _logger.Print("Disconnected from the broker");
        }

        /// <inheritdoc/>
        public async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
            {
                _logger.Print("There is no connection to the broker. Sending messages is not possible");
                return;
            }

            _logger.Print("Sending messages has been started...");

            var counter = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                var sentData = new SentData
                {
                    Timestamp = DateTime.Now,
                    Value = counter
                };
                try
                {
                    await SendMessageAsync(sentData, cancellationToken).ConfigureAwait(false);
                    await Task.Delay(_DELAY, cancellationToken).ConfigureAwait(false);
                    
                    if (counter == int.MaxValue)
                        counter = 0;
                    
                    counter++;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            if (cancellationToken.IsCancellationRequested)
                _logger.Print("Sending messages has been stopped");
        }

        private async Task SendMessageAsync(SentData sentData, CancellationToken cancellationToken)
        {
            string jsonData = JsonConvert.SerializeObject(sentData);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_TOPIC)
                .WithPayload(
                    Encoding.UTF8.GetBytes(jsonData))
                .Build();

            try
            {
                _ = await _mqttClient.PublishAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Print($"Error occurred during publishing: {ex.Message}");
            }
        }

        private void InitializeClient()
        {
            var appSettings = ConfigurationManager.AppSettings;

            var server = appSettings["server"];
            if (string.IsNullOrEmpty(server))
                server = "localhost";

            var portCfg = appSettings["port"];
            if (!int.TryParse(portCfg, out int port))
                port = 1883;

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(server, port)
                .WithCleanSession()
                .Build();

            brokerHost = $"{server}:{port}";
        }
    }
}
