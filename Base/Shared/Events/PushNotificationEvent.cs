using System;

namespace Heleus.Apps.Shared
{
    public enum PushNotificationEventType
    {
        UserInteraction,
        NoneUserInteraction,
        Silent
    }

    public class PushNotificationEvent
    {
        public Uri Scheme;
        public PushNotificationEventType EventType;

        public PushNotificationEvent(Uri scheme, PushNotificationEventType eventType)
        {
            Scheme = scheme;
            EventType = eventType;
        }
    }
}
