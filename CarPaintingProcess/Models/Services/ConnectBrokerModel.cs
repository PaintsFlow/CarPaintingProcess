using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using DryIoc;
using Org.BouncyCastle.Bcpg.OpenPgp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace CarPaintingProcess.Models.Services
{
    public class ConnectBrokerModel
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

        public async void Consumerfunc()
        {
            if (connection == null || !connection.IsOpen) // 연결이 없거나 닫혀 있으면
            {
                if (!await ConnectBroker()) // 연결 실패 시
                {
                    return;
                }
            }

            await Consume(); // 연결이 이미 되어 있거나, 새로 연결되면 메시지 소비 시작
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

        private async Task<bool> ConnectBroker()
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
                Console.WriteLine("✅ RabbitMQ 연결 시도 중...");

                connection = await factory.CreateConnectionAsync();
                channel = await connection.CreateChannelAsync();

                Console.WriteLine("✅ RabbitMQ 연결 성공!");
                return true;  // 연결 성공 시 true 반환

            }
            catch (Exception ex)
            {
                MessageBox.Show($"RabbitMQ 연결 오류:{ex.Message}");
                return false; // 연결 실패 시 false 반환
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
                // declare a server-named queue
                QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync(queue: "persistentQueue",
                    durable: true,        // 서버가 재시작되어도 유지
                    exclusive: false,     // 여러 연결에서 사용 가능
                    autoDelete: false,    // 사용하지 않아도 자동 삭제되지 않음
                    arguments: null);

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
                MessageBox.Show($"🚨 메세지 수신 실패 : {ex.Message}");
            }
        }

        // exchange : control
        // [airsprayPressure:0 , paintFlow:1], [down:0, up:1]
        // ex) 0,1 = airsprayPressure up
        public async Task Produce(string message)
        {
            try
            {
                if (channel != null)
                {
                    var body = Encoding.UTF8.GetBytes(message);

                    await channel.BasicPublishAsync(exchange: "control", routingKey: string.Empty, body: body);
                    Console.WriteLine($"Sent : {message}");

                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🚨 메세지 게시 오류: {ex.Message}");
            }
        }       
    }
}
