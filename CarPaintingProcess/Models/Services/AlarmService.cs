using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class AlarmService
{
    private static AlarmService _instance;
    public static AlarmService Instance => _instance ??= new AlarmService();

    public Dictionary<string, ObservableCollection<AlarmItem>> AlarmsByCategory { get; set; }

    // 🔹 최근 알람 수신 기록 (센서명 + AlarmCode 기준)
    private Dictionary<string, DateTime> _recentAlarms = new Dictionary<string, DateTime>();

    private AlarmService()
    {
        AlarmsByCategory = new Dictionary<string, ObservableCollection<AlarmItem>>
        {
            { "Hado", new ObservableCollection<AlarmItem>() },
            { "Gunjyo", new ObservableCollection<AlarmItem>() },
            { "Dojang", new ObservableCollection<AlarmItem>() }
        };
    }

    public void AddAlarm(string category, AlarmItem alarm)
    {
        if (AlarmsByCategory.ContainsKey(category))
        {
            // ✅ 센서명 + AlarmCode 조합을 키로 사용하여 중복 체크
            string alarmKey = $"{alarm.SensorName}-{alarm.AlarmCode}";

            // 🔹 최근 3분 내 동일 센서+알람코드 알람이 있었다면 등록하지 않음
            if (_recentAlarms.TryGetValue(alarmKey, out DateTime lastReceivedTime))
            {
                if ((DateTime.Now - lastReceivedTime).TotalSeconds < 180) // 3분 제한
                {
                    //Console.WriteLine($"⏳ 알람 중복 방지: {alarm.SensorName} ({alarm.AlarmCode})");
                    return;
                }
            }

            // 🔹 새로운 알람 등록 및 최근 기록 저장
            AlarmsByCategory[category].Add(alarm);
            _recentAlarms[alarmKey] = DateTime.Now;

            // 🔹 3분 후 자동 제한 해제 (Task.Delay 사용)
            Task.Run(async () =>
            {
                await Task.Delay(180000); // 3분 대기
                _recentAlarms.Remove(alarmKey);
                Console.WriteLine($"✅ 알람 제한 해제: {alarm.SensorName} ({alarm.AlarmCode})");
            });
        }
    }

    public void RemoveAlarm(string category, AlarmItem alarm)
    {
        if (AlarmsByCategory.ContainsKey(category) &&
            AlarmsByCategory[category].Contains(alarm))
        {
            AlarmsByCategory[category].Remove(alarm);
        }
    }
}

// 알람 정보 구조
