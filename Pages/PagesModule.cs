using Core;
using Pages.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Pages
{
    public class PagesModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", nameof(SelectImagesSourcesPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ImagesSourcesList>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<SelectImagesSourcesPage>();
        }
    }
}