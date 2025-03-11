using Prism.Mvvm;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Prism.Regions;

public class AlarmViewModel : BindableBase
{
    private readonly IDialogService _dialogService;
    private readonly IRegionManager _regionManager;

    public ObservableCollection<AlarmItem> HadoAlarms => AlarmService.Instance.AlarmsByCategory["Hado"];
    public ObservableCollection<AlarmItem> GunjyoAlarms => AlarmService.Instance.AlarmsByCategory["Gunjyo"];
    public ObservableCollection<AlarmItem> DojangAlarms => AlarmService.Instance.AlarmsByCategory["Dojang"];

    public DelegateCommand<AlarmItem> DeleteAlarmCommand { get; }
    public DelegateCommand<string> NavigateCommand { get; private set; }

    public AlarmViewModel(IDialogService dialogService, IRegionManager regionManager)
    {
        _dialogService = dialogService;
        _regionManager = regionManager;
        DeleteAlarmCommand = new DelegateCommand<AlarmItem>(DeleteAlarm);

        NavigateCommand = new DelegateCommand<string>(Navigate);
    }

    private void DeleteAlarm(AlarmItem alarm)
    {
        if (alarm == null) return;

        foreach (var categoryKey in AlarmService.Instance.AlarmsByCategory.Keys)
        {
            var alarmsInCategory = AlarmService.Instance.AlarmsByCategory[categoryKey];
            if (alarmsInCategory.Contains(alarm))
            {
                alarmsInCategory.Remove(alarm);
                break;
            }
        }

        RaisePropertyChanged(nameof(HadoAlarms));
        RaisePropertyChanged(nameof(GunjyoAlarms));
        RaisePropertyChanged(nameof(DojangAlarms));
    }

    private void Navigate(string viewName)
    {
        if (!string.IsNullOrEmpty(viewName))
        {
            _regionManager.RequestNavigate("MainRegion", viewName);
        }
    }
}
