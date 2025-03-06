using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// AlarmService, AlarmItem 등의 사용을 위해 필요하다면 아래 using 추가
// using CarPaintingProcess.Models.Services;

namespace CarPaintingProcess.Models.Services
{
    public class ConnectBrokerModel
    {
        private static ConnectBrokerModel staticConnectBroker;
        private IConnection connection;
        private IChannel channel;  // 원본 코드 기준(IModel 대신 IChannel을 사용)
                                   // (RabbitMQ.Client 버전에 따라 IModel 사용일 수도 있음)

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

        // ------------------------------------------------------
        // 원본 Consumerfunc에 alarm 소비 부분만 추가 호출
        // ------------------------------------------------------
        public async void Consumerfunc()
        {
            if (await ConnectBroker())  // 연결이 성공하면
            {
                await Consume();      // 기존 logs 익스체인지 소비
                await ConsumeAlarm(); // 새롭게 추가한 alarm 익스체인지 소비
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

        // ------------------------------------------------------
        // 기존 Consume() 메서드 (logs 익스체인지)
        // 코드 변경 없이 그대로 사용
        // ------------------------------------------------------
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
                MessageBox.Show($"🚨 메세지 수신 실패 : {ex.Message}");
            }
        }

        // ------------------------------------------------------
        // 추가: 알람 익스체인지("alarm")를 소비하는 메서드
        // ------------------------------------------------------
        private async Task ConsumeAlarm()
        {
            try
            {
                if (channel == null)
                {
                    MessageBox.Show("❌ 채널이 생성되지 않았습니다. (alarm)");
                    return;
                }
                // alarm 익스체인지 선언
                await Task.Run(() => channel.ExchangeDeclareAsync(exchange: "alarm", type: ExchangeType.Fanout));

                // 서버-이름 방식 큐 선언
                QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
                string alarmQueueName = queueDeclareResult.QueueName;

                // 바인딩
                await channel.QueueBindAsync(queue: alarmQueueName, exchange: "alarm", routingKey: string.Empty);

                Console.WriteLine(" [*] Waiting for alarm messages...");

                var alarmConsumer = new AsyncEventingBasicConsumer(channel);
                alarmConsumer.ReceivedAsync += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                   // Console.WriteLine($" [ALARM] Received: {message}");

                    // UI 스레드에서 ParseAndInsertAlarm(message) 실행
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
                MessageBox.Show($"🚨 알람 메시지 수신 실패 : {ex.Message}");
            }
        }

        // ------------------------------------------------------
        // 추가: 알람 메시지를 파싱하고 AlarmService에 등록하는 부분
        // 실제 프로젝트에 맞게 수정해서 사용
        // -----------------------------------------------------
        // -
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

                string sensorName = tokens[1].Trim(); // ✅ 공백 제거
                if (string.IsNullOrWhiteSpace(sensorName)) // ✅ null 체크 추가
                {
                    Console.WriteLine("⚠️ 센서명이 비어 있어 알람을 등록할 수 없습니다.");
                    return;
                }

                string currentValue = tokens[2].Trim();
                string limitValue = tokens[3].Trim();
                string alarmType = tokens[4].Trim(); // LOW or HIGH

                string minidisplayMsg = $"{sensorName} 경고 ";
                string displayMsg = $"{sensorName} {alarmType} 임계치 발생 - 현재: {currentValue}, 기준: {limitValue}";
                string category = GetAlarmCategory(sensorName);

                var alarmItem = new AlarmItem
                {
                    Message = displayMsg,
                    MiniMessage= minidisplayMsg,
                    Value = currentValue,
                    Timestamp = DateTime.Now,
                    AlarmCode = alarmType == "LOW" ? "LOW-ALARM" : "HIGH-ALARM",
                    SensorName = sensorName // ✅ null 체크 후 저장
                };

                // 🛠 알람 등록 (자동 중복 체크됨)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AlarmService.Instance.AddAlarm(category, alarmItem);
                });

               // Console.WriteLine($"🚨 알람 등록 요청 -> [{category}] {displayMsg}");
            }
            catch (Exception ex)
            {
               // Console.WriteLine($"⚠️ 알람 파싱 중 오류: {ex.Message}");
            }
        }


        // 센서명->카테고리 분류 (예시)
        private string GetAlarmCategory(string sensorName)
        {
            // 프로젝트 상황에 맞게 수정
            if (sensorName.Contains("수위") || sensorName.Contains("점도") || sensorName.Contains("PH"))
                return "Hado";

            if (sensorName.Contains("전압") || sensorName.Contains("전류"))
                return "Gunjyo";

            if (sensorName.Contains("온도") || sensorName.Contains("습도") || sensorName.Contains("스프레이") || sensorName.Contains("페인트"))
                return "Dojang";

            // 기타
            return "Hado";
        }
    }
}
