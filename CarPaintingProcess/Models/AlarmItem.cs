using System;

public class AlarmItem
{
    public string SensorName { get; set; } // ✅ 센서명 필드 추가 (중복 체크용)
    public string Message { get; set; }
    public string MiniMessage { get; set; }
    public string Value { get; set; }
    public DateTime Timestamp { get; set; }
    public string AlarmCode { get; set; }

    public override string ToString()
    {
        return $"{Timestamp:HH:mm:ss} | {Message} ({Value})";
    }
}
