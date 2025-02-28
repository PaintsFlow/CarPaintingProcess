using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CarPaintingProcess.ViewModels
{
    public class MiniAlarmViewModel : BindableBase
    {
        public ObservableCollection<AlarmItem> Alarms { get; set; }

        public DelegateCommand<AlarmItem> DeleteAlarmCommand { get; private set; }

        public MiniAlarmViewModel()
        {
            // 모든 알람을 하나의 리스트로 합침
            Alarms = new ObservableCollection<AlarmItem>(
                AlarmService.Instance.AlarmsByCategory["Hado"]
                .Concat(AlarmService.Instance.AlarmsByCategory["Gunjyo"])
                .Concat(AlarmService.Instance.AlarmsByCategory["Dojang"])
            );

            DeleteAlarmCommand = new DelegateCommand<AlarmItem>(DeleteAlarm);
        }

        private void DeleteAlarm(AlarmItem alarm)
        {
            if (alarm != null)
            {
                // 카테고리를 자동으로 찾아서 삭제
                foreach (var category in AlarmService.Instance.AlarmsByCategory.Keys)
                {
                    if (AlarmService.Instance.AlarmsByCategory[category].Contains(alarm))
                    {
                        AlarmService.Instance.AlarmsByCategory[category].Remove(alarm);
                        break;
                    }
                }

                Alarms.Remove(alarm);
            }
        }
    }
}
