using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class CooldownTimer
    {
        public string Name { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(24);
        public DateTime StartTime { get; set; } = DateTime.Now;

        [JsonIgnore]
        public bool Ready
        {
            get { return DateTime.Now > StartTime + Duration; }
        }

        [JsonIgnore]
        public TimeSpan RemainingTime
        {
            get
            {
                TimeSpan remainder = Duration - (DateTime.Now - StartTime);
                return remainder > TimeSpan.Zero ? remainder : TimeSpan.Zero;
            }
        }
    }
}
