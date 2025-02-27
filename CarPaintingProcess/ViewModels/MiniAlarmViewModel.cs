using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;

namespace CarPaintingProcess.ViewModels
{
    public class MiniAlarmViewModel : BindableBase
    {
        private ObservableCollection<AlarmItem> _alarms;
        public ObservableCollection<AlarmItem> Alarms
        {
            get => _alarms;
            set => SetProperty(ref _alarms, value);
        }

        public DelegateCommand<AlarmItem> DeleteAlarmCommand { get; private set; }

        public MiniAlarmViewModel()
        {
            // 🔥 예제 알람 데이터 추가 🔥
            Alarms = new ObservableCollection<AlarmItem>
            {
                new AlarmItem { Message="Temperature High", Value="95°C", Timestamp=DateTime.Now.AddMinutes(-10), AlarmCode="E101" },
                new AlarmItem { Message="Low Water Level",  Value="5.3m",  Timestamp=DateTime.Now.AddMinutes(-5),  AlarmCode="E102" },
                new AlarmItem { Message="Viscosity Issue",  Value="Too High", Timestamp=DateTime.Now.AddMinutes(-2), AlarmCode="E103" },
                 new AlarmItem { Message="Viscosity Issue",  Value="Too High", Timestamp=DateTime.Now.AddMinutes(-2), AlarmCode="E103" },
                  new AlarmItem { Message="Viscosity Issue",  Value="Too High", Timestamp=DateTime.Now.AddMinutes(-2), AlarmCode="E103" },
                   new AlarmItem { Message="Viscosity Issue",  Value="Too High", Timestamp=DateTime.Now.AddMinutes(-2), AlarmCode="E103" },
                    new AlarmItem { Message="Viscosity Issue",  Value="Too High", Timestamp=DateTime.Now.AddMinutes(-2), AlarmCode="E103" },
                     new AlarmItem { Message="Viscosity Issue",  Value="Too High", Timestamp=DateTime.Now.AddMinutes(-2), AlarmCode="E103" }

            };

            DeleteAlarmCommand = new DelegateCommand<AlarmItem>(DeleteAlarm);
        }

        private void DeleteAlarm(AlarmItem alarm)
        {
            if (alarm != null && Alarms.Contains(alarm))
            {
                Alarms.Remove(alarm);
            }
        }
    }

    public class AlarmItem
    {
        public string Message { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string AlarmCode { get; set; }
    }
}
