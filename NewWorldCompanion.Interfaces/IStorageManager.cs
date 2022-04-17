using NewWorldCompanion.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Interfaces
{
    public interface IStorageManager
    {
        ObservableCollection<Item> Items
        {
            get;
        }

        void SaveStorage();
    }
}