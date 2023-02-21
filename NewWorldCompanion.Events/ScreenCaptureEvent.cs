using Prism.Events;

namespace NewWorldCompanion.Events
{
    public class ScreenCaptureReadyEvent : PubSubEvent
    {
    }

    public class MouseCoordinatesUpdatedEvent : PubSubEvent
    {
    }

    public class MouseDeltaUpdatedEvent : PubSubEvent<double>
    {

    }
}
