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
            containerRegistry.RegisterForNavigation<MiniAlarmView, MiniAlarmViewModel>();
            containerRegistry.RegisterForNavigation<SideBarView, SideBarViewModel>();
            containerRegistry.RegisterForNavigation<AlarmView, AlarmViewModel>();
            containerRegistry.RegisterForNavigation<DefectDetectionView, DefectDetectionViewModel>();
            containerRegistry.RegisterForNavigation<DryView, DryViewModel>();
            containerRegistry.RegisterForNavigation<ElectroDepositionView, ElectroDepositionViewModel>();
            containerRegistry.RegisterForNavigation<PaintingView, PaintingViewModel>();
            containerRegistry.RegisterForNavigation<SearchView, SearchViewModel>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Prism에서 제공하는 DI 컨테이너로부터 RegionManager 가져오기
            var regionManager = Container.Resolve<IRegionManager>();

            //초기 화면
            regionManager.RegisterViewWithRegion("SideBarRegion", typeof(Views.Controls.SideBarView));
            regionManager.RegisterViewWithRegion("AlarmRegion", typeof(MiniAlarmView));
            regionManager.RequestNavigate("MainRegion", "ElectroDepositionView");
        }
    }
}
