using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using NewWorldCompanion.Interfaces;
using NewWorldCompanion.Services;
using NewWorldCompanion.Views;
using NLog.Extensions.Logging;
using Prism.Ioc;
using Prism.Unity;
using System.Windows;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace NewWorldCompanion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
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

            // Register Metro
            containerRegistry.RegisterSingleton<IDialogCoordinator, DialogCoordinator>();
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(loggingBuilder =>
                loggingBuilder.AddNLog(configFileRelativePath: "Config/NLog.config"));

            var container = new UnityContainer();
            container.BuildServiceProvider(serviceCollection);

            return new UnityContainerExtension(container);
        }


        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }
    }
}
