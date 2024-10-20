using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class SettingsNWC
    {
        // App
        public bool DebugModeActive { get; set; } = false;

        // Overlay
        public bool TooltipEnabled { get; set; } = true;
        public bool ExtendedTooltipEnabled { get; set; } = false;
        public bool NamedItemsTooltipEnabled { get; set; } = false;
        public string PriceServerIdNwm { get; set; } = string.Empty;

        // Shape detection
        public int EmguAreaLower { get; set; } = 10000;
        public int EmguAreaUpper { get; set; } = 15000;
        public int EmguHysteresisLower { get; set; } = 10;
        public int EmguHysteresisUpper { get; set; } = 400;

        // OCR
        public int EmguThresholdMin { get; set; } = 90;
        public int EmguThresholdMax { get; set; } = 255;
        public int EmguThresholdMaxR { get; set; } = 200;
        public int EmguThresholdMaxG { get; set; } = 235;
        public int EmguThresholdMaxB { get; set; } = 255;

        // Named items
        public bool NamedItemsFilterTier2 { get; set; } = true;
        public bool NamedItemsFilterTier3 { get; set; } = true;
        public bool NamedItemsFilterTier4 { get; set; } = true;
        public bool NamedItemsFilterTier5 { get; set; } = true;
        public bool NamedItemsFilterItemClassArmor { get; set; } = true;
        public bool NamedItemsFilterItemClassJewelry { get; set; } = true;
        public bool NamedItemsFilterItemClassWeapon { get; set; } = true;
        public bool NamedItemsFilterStorageCollected { get; set; } = true;
        public bool NamedItemsFilterStorageMissing { get; set; } = true;
        public bool NamedItemsFilterStorageDuplicates { get; set; } = true;
        public bool NamedItemsFilterBindOnEquip { get; set; } = true;
        public bool NamedItemsFilterBindOnPickup { get; set; } = true;
    }
}
