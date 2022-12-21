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
        public int PriceServerId { get; set; } = 1;

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
    }
}
