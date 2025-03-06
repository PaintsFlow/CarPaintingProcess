using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarPaintingProcess.Models.Services
{
    public partial class ConnectBrokerModel
    {
        private static ConnectBrokerModel staticConnectBroker;
        private IConnection connection;
        private IChannel channel;

        private ConnectBrokerModel() { }

        public event Action<string> MessageReceived;

        public static ConnectBrokerModel GetInstance()
        {
            if (staticConnectBroker == null)
            {
                staticConnectBroker = new ConnectBrokerModel();
            }
            return staticConnectBroker;
        }

        public async Task<bool> ConnectBroker()
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory()
                {
                    HostName = "211.187.0.113",
                    UserName = "guest",
                    Password = "guest",
                    Port = 5672
                };

                connection = await factory.CreateConnectionAsync();
                channel = await connection.CreateChannelAsync();

                Console.WriteLine("✅ RabbitMQ 연결 성공!");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RabbitMQ 연결 오류: {ex.Message}");
                return false;
            }
        }

        public async void Consumerfunc()
        {
            if (await ConnectBroker())
            {
                await Consume();      // logs 익스체인지 소비
                await ConsumeAlarm(); // 알람 익스체인지 소비 (파셜 클래스에서 실행)
            }
            else
            {
                MessageBox.Show("🚨 RabbitMQ 연결에 실패했습니다. 다시 시도하세요.");
            }
        }

        private async Task Consume()
        {
            try
            {
                if (channel == null)
                {
                    MessageBox.Show("❌ 채널이 생성되지 않았습니다.");
                    return;
                }

                await Task.Run(() => channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout));
                QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
                string queueName = queueDeclareResult.QueueName;
                await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);

                Console.WriteLine(" [*] Waiting for logs...");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Received: {message}");

                    MessageReceived?.Invoke(message);
                    return Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🚨 메시지 수신 실패: {ex.Message}");
            }
        }

        public async void Producerfunc(string message)
        {
            if (connection == null || !connection.IsOpen)
            {
                if (!await ConnectBroker())
                {
                    return;
                }
            }
            await Produce(message);
        }

        public async Task Produce(string message)
        {
            try
            {
                if (channel != null)
                {
                    var body = Encoding.UTF8.GetBytes(message);
                    await channel.BasicPublishAsync(exchange: "control", routingKey: string.Empty, body: body);
                    Console.WriteLine($"Sent : {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🚨 메시지 게시 오류: {ex.Message}");
            }
        }
    }
}
