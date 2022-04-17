using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NewWorldCompanion.Extensions
{
    public static class ObservableCollectionExtensionMethods
    {
        public static int Remove<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            var itemsToRemove = collection.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                collection.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
    }
}
