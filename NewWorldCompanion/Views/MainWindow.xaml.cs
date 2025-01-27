﻿using MahApps.Metro.Controls;
using NewWorldCompanion.Views.Tabs;
using NewWorldCompanion.Views.Tabs.Config;
using NewWorldCompanion.Views.Tabs.Debug;
using NewWorldCompanion.Views.Tabs.Items;
using Prism.Regions;
using System;

namespace NewWorldCompanion.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(IRegionManager regionManager)
        {
            InitializeComponent();

            if (regionManager == null)
            {
                throw new ArgumentNullException(nameof(regionManager));
            }

            // Regions
            regionManager.RegisterViewWithRegion("CraftingView", typeof(CraftingView));
            regionManager.RegisterViewWithRegion("CooldownView", typeof(CooldownView));
            regionManager.RegisterViewWithRegion("ItemsView", typeof(ItemsView));
            regionManager.RegisterViewWithRegion("StorageView", typeof(StorageView));
            regionManager.RegisterViewWithRegion("NamedItemsView", typeof(NamedItemsView));

            regionManager.RegisterViewWithRegion("ConfigView", typeof(ConfigView));
            regionManager.RegisterViewWithRegion("ConfigOverlayView", typeof(ConfigOverlayView));
            regionManager.RegisterViewWithRegion("ConfigScreenProcessView", typeof(DebugScreenProcessView));
            regionManager.RegisterViewWithRegion("ConfigScreenOCRView", typeof(DebugScreenOCRView));
            regionManager.RegisterViewWithRegion("DebugView", typeof(DebugView));
            regionManager.RegisterViewWithRegion("DebugScreenCaptureView", typeof(DebugScreenCaptureView));
            regionManager.RegisterViewWithRegion("DebugScreenProcessView", typeof(DebugScreenProcessView));
            regionManager.RegisterViewWithRegion("DebugScreenOCRView", typeof(DebugScreenOCRView));
            regionManager.RegisterViewWithRegion("DebugScreenCountOCRView", typeof(DebugScreenCountOCRView));
        }
    }
}
