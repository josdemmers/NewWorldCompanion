﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Interfaces
{
    public interface IVersionManager
    {
        string CurrentVersion { get; }
        string LatestVersion { get; }
    }
}
