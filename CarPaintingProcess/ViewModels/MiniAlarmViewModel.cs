using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;

public class MiniAlarmViewModel : BindableBase
{
    private ObservableCollection<AlarmItem> _alarms;
    public ObservableCollection<AlarmItem> Alarms
    {
        get => _alarms;
        private set => SetProperty(ref _alarms, value);
    }

    public DelegateCommand<AlarmItem> DeleteAlarmCommand { get; private set; }

    public MiniAlarmViewModel()
    {
        _alarms = new ObservableCollection<AlarmItem>(
            AlarmService.Instance.AlarmsByCategory["Hado"]
            .Concat(AlarmService.Instance.AlarmsByCategory["Gunjyo"])
            .Concat(AlarmService.Instance.AlarmsByCategory["Dojang"])
        );
        foreach (var category in AlarmService.Instance.AlarmsByCategory.Values)
        {
            category.CollectionChanged += OnAlarmsChanged;
        }

        DeleteAlarmCommand = new DelegateCommand<AlarmItem>(DeleteAlarm);
    }

    private void OnAlarmsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Alarms = new ObservableCollection<AlarmItem>(
            AlarmService.Instance.AlarmsByCategory["Hado"]
            .Concat(AlarmService.Instance.AlarmsByCategory["Gunjyo"])
            .Concat(AlarmService.Instance.AlarmsByCategory["Dojang"])
        );
    }

    private void DeleteAlarm(AlarmItem alarm)
    {
        if (alarm == null) return;

        foreach (var category in AlarmService.Instance.AlarmsByCategory.Keys)
        {
            if (AlarmService.Instance.AlarmsByCategory[category].Contains(alarm))
            {
                AlarmService.Instance.AlarmsByCategory[category].Remove(alarm);
                break;
            }
        }
    }
}
