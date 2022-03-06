using NewWorldCompanion.Interfaces;
using NewWorldCompanion.Services;
using NewWorldCompanion.Views;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            containerRegistry.RegisterSingleton<ICraftingRecipeManager, CraftingRecipeManager>();
            containerRegistry.RegisterSingleton<IHttpClientHandler, HttpClientHandler>();
            containerRegistry.RegisterSingleton<IScreenCaptureHandler, ScreenCaptureHandler>();
            containerRegistry.RegisterSingleton<IScreenProcessHandler, ScreenProcessHandler>();
            containerRegistry.RegisterSingleton<INewWorldDataStore, NewWorldDataStore>();
            containerRegistry.RegisterSingleton<IOcrHandler, OcrHandler>();
            containerRegistry.RegisterSingleton<IOverlayHandler, OverlayHandler>();
            containerRegistry.RegisterSingleton<IPriceManager, PriceManager>();
            containerRegistry.RegisterSingleton<ISettingsManager, SettingsManager>();
        }

        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }
    }
}
