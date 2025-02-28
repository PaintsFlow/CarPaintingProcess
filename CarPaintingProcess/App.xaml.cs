using Prism.Ioc;
using Prism.DryIoc;
using System.Windows;
using CarPaintingProcess.Views;
using Prism.Regions;
using CarPaintingProcess.Views.Controls;
using CarPaintingProcess.ViewModels;

namespace CarPaintingProcess
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ProcessAView>("ProcessAView");
            containerRegistry.RegisterForNavigation<ProcessBView>("ProcessBView");
            containerRegistry.RegisterForNavigation<SearchView>("SearchView");
            containerRegistry.RegisterForNavigation<AlarmView>("AlarmView");
            containerRegistry.RegisterForNavigation<MiniAlarmView, MiniAlarmViewModel>();
            containerRegistry.RegisterForNavigation<AlarmView, AlarmViewModel>();

        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Prism에서 제공하는 DI 컨테이너로부터 RegionManager 가져오기
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("SideBarRegion", typeof(SideBarView));
            regionManager.RegisterViewWithRegion("AlarmRegion", typeof(MiniAlarmView));

        }
    }
}
