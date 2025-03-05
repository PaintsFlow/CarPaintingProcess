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
            if (await ConnectBroker())  // 연결이 성공하면
            {
                await Consume();  // 메시지 소비 시작
            }
            else
            {
                MessageBox.Show("🚨 RabbitMQ 연결에 실패했습니다. 다시 시도하세요.");
            }
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
                if (channel != null)
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
        
       
    }
}
