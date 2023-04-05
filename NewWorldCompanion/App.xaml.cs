using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using NewWorldCompanion.Interfaces;
using NewWorldCompanion.Services;
using NewWorldCompanion.Views;
using NLog.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using System.Threading;
using System.Windows;

namespace NewWorldCompanion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static Mutex? _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "NewWorldCompanion";

            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register services
            containerRegistry.RegisterSingleton<ICooldownManager, CooldownManager>();
            containerRegistry.RegisterSingleton<ICraftingRecipeManager, CraftingRecipeManager>();
            containerRegistry.RegisterSingleton<IHttpClientHandler, HttpClientHandler>();
            containerRegistry.RegisterSingleton<IScreenCaptureHandler, ScreenCaptureHandler>();
            containerRegistry.RegisterSingleton<IScreenProcessHandler, ScreenProcessHandler>();
            containerRegistry.RegisterSingleton<INewWorldDataStore, NewWorldDataStore>();
            containerRegistry.RegisterSingleton<IOcrHandler, OcrHandler>();
            containerRegistry.RegisterSingleton<IOverlayHandler, OverlayHandler>();
            containerRegistry.RegisterSingleton<IPriceManager, PriceManager>();
            containerRegistry.RegisterSingleton<ISettingsManager, SettingsManager>();
            containerRegistry.RegisterSingleton<IStorageManager, StorageManager>();
            containerRegistry.RegisterSingleton<IVersionManager, VersionManager>();
            containerRegistry.RegisterSingleton<IRelatedPriceManager, RelatedPriceManager>();
            
            // Register Metro
            containerRegistry.RegisterSingleton<IDialogCoordinator, DialogCoordinator>();
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(loggingBuilder =>
                loggingBuilder.AddNLog(configFileRelativePath: "Config/NLog.config"));

            return new DryIocContainerExtension(new Container(CreateContainerRules()).WithDependencyInjectionAdapter(serviceCollection));
        }
    }
}