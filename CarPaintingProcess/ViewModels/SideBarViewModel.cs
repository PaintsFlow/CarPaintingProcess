using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace CarPaintingProcess.ViewModels
{
    public class SideBarViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; private set; }

        public SideBarViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                _regionManager.RequestNavigate("MainRegion", viewName);
            }
        }
    }
}
