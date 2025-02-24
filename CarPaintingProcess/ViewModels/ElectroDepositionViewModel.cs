using Prism.Mvvm;

namespace CarPaintingProcess.ViewModels
{
    public class ElectroDepositionViewModel : BindableBase
    {
        private string _str;
        public string Str
        {
            get { return _str; }
            set { SetProperty(ref _str, value); }
        }

        public ElectroDepositionViewModel()
        {

        }
    }
}
