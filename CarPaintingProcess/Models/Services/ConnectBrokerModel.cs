using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            if (await ConnectBroker())
            {
                await Consume("logs"); // 기존 logs 익스체인지 소비
                await Consume("alarm", ParseAndInsertAlarm); // 알람 익스체인지 소비
            }
            else
            {
                MessageBox.Show("🚨 RabbitMQ 연결에 실패했습니다. 다시 시도하세요.");
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
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RabbitMQ 연결 오류: {ex.Message}");
                return false;
            }
        }

        private async Task Consume(string exchange, Action<string> customAction = null)
        {
            try
            {
                if (channel == null)
                {
                    MessageBox.Show($"❌ 채널이 생성되지 않았습니다. ({exchange})");
                    return;
                }

                await Task.Run(() => channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Fanout));
                QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
                string queueName = queueDeclareResult.QueueName;
                await channel.QueueBindAsync(queue: queueName, exchange: exchange, routingKey: string.Empty);

                Console.WriteLine($" [*] Waiting for {exchange} messages...");
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    if (customAction != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => customAction(message));
                    }
                    else
                    {
                        Console.WriteLine($" [x] Received: {message}");
                        MessageReceived?.Invoke(message);
                    }
                    return Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🚨 {exchange} 메시지 수신 실패 : {ex.Message}");
            }
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
                MessageBox.Show($"🚨 메세지 게시 오류: {ex.Message}");
            }
        }

        private void ParseAndInsertAlarm(string alarmMessage)
        {
            try
            {
                string[] tokens = alarmMessage.Split(',');
                if (tokens.Length < 5)
                {
                    Console.WriteLine("⚠️ 알람 메시지 포맷이 올바르지 않습니다.");
                    return;
                }

                string sensorName = tokens[1].Trim();
                if (string.IsNullOrWhiteSpace(sensorName))
                {
                    Console.WriteLine("⚠️ 센서명이 비어 있어 알람을 등록할 수 없습니다.");
                    return;
                }

                string currentValue = tokens[2].Trim();
                string limitValue = tokens[3].Trim();
                string alarmType = tokens[4].Trim();

                string displayMsg = $"{sensorName} {alarmType} 임계치 발생 - 현재: {currentValue}, 기준: {limitValue}";
                string category = GetAlarmCategory(sensorName);

                var alarmItem = new AlarmItem
                {
                    Message = displayMsg,
                    MiniMessage = $"{sensorName} 경고",
                    Value = currentValue,
                    Timestamp = DateTime.Now,
                    AlarmCode = alarmType == "LOW" ? "LOW-ALARM" : "HIGH-ALARM",
                    SensorName = sensorName
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AlarmService.Instance.AddAlarm(category, alarmItem);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ 알람 파싱 중 오류: {ex.Message}");
            }
        }

        private string GetAlarmCategory(string sensorName)
        {
            if (sensorName.Contains("수위") || sensorName.Contains("점도") || sensorName.Contains("PH")) return "Hado";
            if (sensorName.Contains("전압") || sensorName.Contains("전류")) return "Gunjyo";
            if (sensorName.Contains("온도") || sensorName.Contains("습도") || sensorName.Contains("스프레이") || sensorName.Contains("페인트")) return "Dojang";
            return "Hado";
        }
    }
}