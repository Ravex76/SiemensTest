using FluentAssertions;
using MQTTnet.Client;
using Moq;
using MqttPublisher;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading;
using MQTTnet;
using MQTTnet.Exceptions;
using System.Linq;

namespace MqttPublisher.Tests
{
    [TestFixture]
    public class PublisherTests
    {
        private Mock<IMqttClient> _mockMqttClient;
        private Mock<ILogger> _mockLogger;
        private Publisher _publisher;

        [SetUp]
        public void SetUp()
        {
            _mockMqttClient = new Mock<IMqttClient>();
            _mockLogger = new Mock<ILogger>();
            _publisher = new Publisher(_mockMqttClient.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockLogger.Reset();
            _mockMqttClient.Reset();
        }

        [Test]
        public async Task ConnectAsync_ReturnsTrue()
        {
            _mockMqttClient
                .Setup(x => x.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            
            var result = await _publisher.ConnectAsync();

            result.Should().BeTrue();
            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg.Contains("Connected to the broker"))));
        }

        [Test]
        public async Task ConnectAsync_ReturnsFalse([Values]MqttClientConnectResultCode resultCode)
        {
            if (resultCode == MqttClientConnectResultCode.Success)
                return;
            
            var mqttClientConnectResult = new MqttClientConnectResult();
            mqttClientConnectResult
                .GetType()
                .GetProperty("ResultCode")
                .SetValue(mqttClientConnectResult, resultCode, null);

            _mockMqttClient
                .Setup(x => x.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mqttClientConnectResult);

            var result = await _publisher.ConnectAsync();

            result.Should().BeFalse();
            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg.Contains("There is no connection to the broker"))));
        }

        [Test]
        public async Task ConnectAsync_InternalException_ReturnsFalse()
        {
            _mockMqttClient
                .Setup(x => x.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MqttCommunicationException(new Exception()));

            var result = await _publisher.ConnectAsync();

            result.Should().BeFalse();
            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg.Contains("Exception occurred during the connection to the broker"))));
        }

        [Test]
        public async Task DisconnectAsync_ExpectedBehavior()
        {
            await _publisher.DisconnectAsync();
            
            _mockMqttClient
                .Verify(x => x.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()));
            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg == "Disconnected from the broker")));
        }

        [Test]
        public async Task SendMessagesAsync_ClientNotConnected_NoSending()
        {
            _mockMqttClient
                .SetupGet(x => x.IsConnected)
                .Returns(false);

            await _publisher.SendMessagesAsync(new CancellationToken());

            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg == "There is no connection to the broker. Sending messages is not possible")));
            _mockMqttClient
                .Verify(x => x.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendMessagesAsync_ClientConnected_SendingMessages()
        {
            _mockMqttClient
                .SetupGet(x => x.IsConnected)
                .Returns(true);
      
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            _ = _publisher.SendMessagesAsync(token);

            await Task.Delay(3000);
            cancellationTokenSource.Cancel();
            
            _mockMqttClient
                .Verify(x => x.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Between(2, 3, Range.Inclusive));
            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg == "Sending messages has been started...")));
            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg == "Sending messages has been stopped")));
        }

        [Test]
        public async Task SendMessagesAsync_InternalException_ExpectedBehavior()
        {
            _mockMqttClient
                .SetupGet(x => x.IsConnected)
                .Returns(true);
            _mockMqttClient
                .Setup(x => x.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            _ = _publisher.SendMessagesAsync(token);

            await Task.Delay(500);
            cancellationTokenSource.Cancel();

            _mockLogger
                .Verify(x => x.Print(It.Is<string>(msg => msg.Contains("Error occurred during publishing"))));
        }
    }
}
