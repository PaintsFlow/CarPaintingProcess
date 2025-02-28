using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;

namespace CarPaintingProcess.ViewModels
{
    public class AlarmViewModel : BindableBase
    {
        public ObservableCollection<AlarmItem> HadoAlarms => AlarmService.Instance.AlarmsByCategory["Hado"];
        public ObservableCollection<AlarmItem> GunjyoAlarms => AlarmService.Instance.AlarmsByCategory["Gunjyo"];
        public ObservableCollection<AlarmItem> DojangAlarms => AlarmService.Instance.AlarmsByCategory["Dojang"];

        public DelegateCommand<AlarmItem> DeleteHadoAlarmCommand { get; private set; }
        public DelegateCommand<AlarmItem> DeleteGunjyoAlarmCommand { get; private set; }
        public DelegateCommand<AlarmItem> DeleteDojangAlarmCommand { get; private set; }

        public AlarmViewModel()
        {
            DeleteHadoAlarmCommand = new DelegateCommand<AlarmItem>(alarm => AlarmService.Instance.RemoveAlarm("Hado", alarm));
            DeleteGunjyoAlarmCommand = new DelegateCommand<AlarmItem>(alarm => AlarmService.Instance.RemoveAlarm("Gunjyo", alarm));
            DeleteDojangAlarmCommand = new DelegateCommand<AlarmItem>(alarm => AlarmService.Instance.RemoveAlarm("Dojang", alarm));
        }
    }
}
