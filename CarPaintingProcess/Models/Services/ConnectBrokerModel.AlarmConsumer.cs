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
        private async Task ConsumeAlarm()
        {
            try
            {
                if (channel == null)
                {
                    MessageBox.Show("❌ 채널이 생성되지 않았습니다. (alarm)");
                    return;
                }

                await Task.Run(() => channel.ExchangeDeclareAsync(exchange: "alarm", type: ExchangeType.Fanout));
                QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
                string alarmQueueName = queueDeclareResult.QueueName;

                await channel.QueueBindAsync(queue: alarmQueueName, exchange: "alarm", routingKey: string.Empty);

                Console.WriteLine(" [*] Waiting for alarm messages...");

                var alarmConsumer = new AsyncEventingBasicConsumer(channel);
                alarmConsumer.ReceivedAsync += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ParseAndInsertAlarm(message);
                    });

                    return Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(queue: alarmQueueName, autoAck: true, consumer: alarmConsumer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🚨 알람 메시지 수신 실패: {ex.Message}");
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

                string minidisplayMsg = $"{sensorName} 경고 ";
                string displayMsg = $"{sensorName} {alarmType} 임계치 발생 - 현재: {currentValue}, 기준: {limitValue}";
                string category = GetAlarmCategory(sensorName);

                var alarmItem = new AlarmItem
                {
                    Message = displayMsg,
                    MiniMessage = minidisplayMsg,
                    Value = currentValue,
                    Timestamp = DateTime.Now,
                    AlarmCode = alarmType == "LOW" ? "LOW-ALARM" : "HIGH-ALARM",
                    SensorName = sensorName
                };

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
            if (sensorName.Contains("수위") || sensorName.Contains("점도") || sensorName.Contains("PH"))
                return "Hado";

            if (sensorName.Contains("전압") || sensorName.Contains("전류"))
                return "Gunjyo";

            if (sensorName.Contains("온도") || sensorName.Contains("습도") || sensorName.Contains("스프레이") || sensorName.Contains("페인트"))
                return "Dojang";

            return "Hado";
        }
    }
}
