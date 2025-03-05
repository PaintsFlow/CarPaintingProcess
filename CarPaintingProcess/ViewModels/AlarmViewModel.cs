using Prism.Mvvm;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;

public class AlarmViewModel : BindableBase
{
    private readonly IDialogService _dialogService;

    public ObservableCollection<AlarmItem> HadoAlarms => AlarmService.Instance.AlarmsByCategory["Hado"];
    public ObservableCollection<AlarmItem> GunjyoAlarms => AlarmService.Instance.AlarmsByCategory["Gunjyo"];
    public ObservableCollection<AlarmItem> DojangAlarms => AlarmService.Instance.AlarmsByCategory["Dojang"];

    public DelegateCommand<AlarmItem> DeleteAlarmCommand { get; }
    public DelegateCommand ShowControlCommand { get; }

    public AlarmViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        DeleteAlarmCommand = new DelegateCommand<AlarmItem>(DeleteAlarm);
        
        // 🔥 "제어 하기" 버튼을 눌렀을 때 `ControlDialog`를 띄움
        ShowControlCommand = new DelegateCommand(ShowControl);
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

    private void ShowControl()
    {
        // 🔥 `ControlDialog`를 띄움
        _dialogService.ShowDialog("ControlDialog", null, callback =>
        {
            if (callback.Result == ButtonResult.OK)
            {
                // 팝업에서 "확인"을 눌렀을 때 처리할 로직
            }
        });
    }
}
