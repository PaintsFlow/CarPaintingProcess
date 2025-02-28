using Prism.Ioc;
using Prism.DryIoc;
using System.Windows;
using CarPaintingProcess.Views;
using Prism.Regions;
using CarPaintingProcess.Views.Controls;
using CarPaintingProcess.ViewModels;
using System;
using System.ServiceProcess;
using System.Diagnostics;
using System.ComponentModel.Design;

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
            containerRegistry.RegisterForNavigation<DefectDetectionView, DefectDetectionViewModel>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Prism에서 제공하는 DI 컨테이너로부터 RegionManager 가져오기
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("LeftSideRegion", typeof(SideBarView));
            regionManager.RegisterViewWithRegion("MainRegion", typeof(PaintingView));
        }

        
    }
}
