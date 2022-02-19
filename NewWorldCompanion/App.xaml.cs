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
            containerRegistry.RegisterSingleton<ICraftingRecipeStore, CraftingRecipeStore>();
            containerRegistry.RegisterSingleton<IScreenCaptureHandler, ScreenCaptureHandler>();
            containerRegistry.RegisterSingleton<IScreenProcessHandler, ScreenProcessHandler>();
            containerRegistry.RegisterSingleton<IOcrHandler, OcrHandler>();
        }

        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }
    }
}
