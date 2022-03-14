using NewWorldCompanion.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Interfaces
{
    public interface ICooldownManager
    {
        List<CooldownTimer> CooldownTimers
        {
            get;
        }

        void AddCooldown(CooldownTimer cooldownTimer);
        void RemoveCooldown(CooldownTimer cooldownTimer);

        void SaveCooldowns();
    }
}
