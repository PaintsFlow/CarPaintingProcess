using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class AlarmService
{
    private static AlarmService _instance;
    public static AlarmService Instance => _instance ??= new AlarmService();

    public Dictionary<string, ObservableCollection<AlarmItem>> AlarmsByCategory { get; set; }

    private AlarmService()
    {
        AlarmsByCategory = new Dictionary<string, ObservableCollection<AlarmItem>>
        {
            { "Hado", new ObservableCollection<AlarmItem>
                {
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" },
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" },
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" },
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" },
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" },
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" },
                    new AlarmItem { Message="하도 온도 이상", Value="85°C", Timestamp=DateTime.Now, AlarmCode="E101" }

                }
            },
            { "Gunjyo", new ObservableCollection<AlarmItem>
                {
                    new AlarmItem { Message="습도 초과", Value="75%", Timestamp=DateTime.Now, AlarmCode="E201" },
                    new AlarmItem { Message="습도 초과", Value="75%", Timestamp=DateTime.Now, AlarmCode="E201" },
                    new AlarmItem { Message="습도 초과", Value="75%", Timestamp=DateTime.Now, AlarmCode="E201" },
                    new AlarmItem { Message="습도 초과", Value="75%", Timestamp=DateTime.Now, AlarmCode="E201" },
                    new AlarmItem { Message="습도 초과", Value="75%", Timestamp=DateTime.Now, AlarmCode="E201" },
                    new AlarmItem { Message="습도 초과", Value="75%", Timestamp=DateTime.Now, AlarmCode="E201" }

                }
            },
            { "Dojang", new ObservableCollection<AlarmItem>
                {
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" },
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" },
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" },
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" },
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" },
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" },
                    new AlarmItem { Message="도장 압력 낮음", Value="1.2bar", Timestamp=DateTime.Now, AlarmCode="E301" }

                }
            }
        };
    }

    public void RemoveAlarm(string category, AlarmItem alarm)
    {
        if (AlarmsByCategory.ContainsKey(category) && AlarmsByCategory[category].Contains(alarm))
            AlarmsByCategory[category].Remove(alarm);
    }

    public void AddAlarm(string category, AlarmItem alarm)
    {
        if (AlarmsByCategory.ContainsKey(category))
            AlarmsByCategory[category].Add(alarm);
    }
}
