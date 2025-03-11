using Prism.Mvvm;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;

public class ControlDialogViewModel : BindableBase, IDialogAware
{
    public string Title => "도장 공정 제어";

    private string _currentStateString = "대기 중";
    public string CurrentStateString
    {
        get => _currentStateString;
        set => SetProperty(ref _currentStateString, value);
    }

    public DelegateCommand SetOnCommand { get; }
    public DelegateCommand SetOffCommand { get; }
    public DelegateCommand CloseDialogCommand { get; }

    public event Action<IDialogResult> RequestClose;

    public ControlDialogViewModel()
    {
        SetOnCommand = new DelegateCommand(() => CurrentStateString = "작동 중");
        SetOffCommand = new DelegateCommand(() => CurrentStateString = "정지됨");
        CloseDialogCommand = new DelegateCommand(CloseDialog);
    }

    private void CloseDialog()
    {
        var result = new DialogResult(ButtonResult.OK);
        RequestClose?.Invoke(result);
    }

    public bool CanCloseDialog() => true;
    public void OnDialogClosed() { }
    public void OnDialogOpened(IDialogParameters parameters) { }
}
