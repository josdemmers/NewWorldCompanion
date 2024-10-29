using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Events
{
    public class PriceServerListUpdatedEvent : PubSubEvent
    {
    }

    public class PriceCacheUpdatedEvent : PubSubEvent
    {
    }

    public class SelectedServerChanged : PubSubEvent
    {
    }
}
