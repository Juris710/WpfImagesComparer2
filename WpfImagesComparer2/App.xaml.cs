using Prism.Ioc;
using Prism.Modularity;
using Reactive.Bindings.Schedulers;
using Reactive.Bindings;
using System.Windows;
using WpfImagesComparer2.Views;

namespace WpfImagesComparer2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ReactivePropertyScheduler.SetDefault(new ReactivePropertyWpfScheduler(Dispatcher));
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Core.Properties.Settings.Default.Save();
            base.OnExit(e);
        }
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Pages.PagesModule>();
        }
    }
}
