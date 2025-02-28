using System;

public class AlarmItem
{
    public string Message { get; set; }     // 알람 메시지 (예: "온도 이상")
    public string Value { get; set; }       // 알람 값 (예: "85°C")
    public DateTime Timestamp { get; set; } // 발생 시간
    public string AlarmCode { get; set; }   // 알람 코드 (예: "E101")

    public override string ToString()
    {
        return $"{Timestamp:HH:mm:ss} | {Message} ({Value})";
    }
}
